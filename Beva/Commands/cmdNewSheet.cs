using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Beva.FormData;
using Beva.Forms;
using Beva.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Beva.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class cmdNewSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var newSheetManager = new NewSheetManager(commandData);

                DialogResult result = DialogResult.None;
                using (frmNewSheet form = new frmNewSheet(newSheetManager))
                {
                    result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        NewSheetData data = form.FormData;

                        CreateElevationMarkers(commandData, data);

                        CreateSheets(commandData, data);

                        return Result.Succeeded;
                    }
                }

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;

                return Result.Failed;
            }
        }

        private void CreateElevationMarkers(ExternalCommandData commandData, NewSheetData data)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            UIDocument uiDoc = new UIDocument(doc);
            using (Transaction t = new Transaction(doc))
            {
                if (t.Start("Create Elevation Views") == TransactionStatus.Started)
                {
                    try
                    {
                        View3D view3d = View3DExist(doc);

                        if (view3d != null)
                        {
                            List<ElevationMarker> elevationsMarkers = new List<ElevationMarker>();
                            elevationsMarkers = ViewsElevationMarksExists(doc);

                            int countElevationMarkers = elevationsMarkers.Count();

                            //si existen elevaciones creadas entonces se duplican las existentes con las mismas características que tienen; de lo contrario se crean nuevas elevaciones
                            if (data.SelectNorthElevationViewTemplate)
                            {
                                if (countElevationMarkers == 0)
                                {
                                    CreateElevationMarkersNorth(doc, uiDoc);
                                }
                                else
                                {
                                    ExistElevationMarkersNorth(doc, uiDoc);
                                }
                            }

                            if (data.SelectSouthElevationViewTemplate)
                            {
                                if (countElevationMarkers == 0)
                                {
                                    CreateElevationMarkersSouth(doc, uiDoc);
                                }
                                else
                                {
                                    ExistElevationMarkersSouth(doc, uiDoc);
                                }
                            }

                            if (data.SelectWestElevationViewTemplate)
                            {
                                if (countElevationMarkers == 0)
                                {
                                    CreateElevationMarkersWest(doc, uiDoc);
                                }
                                else
                                {
                                    ExistElevationMarkersWest(doc, uiDoc);
                                }
                            }

                            if (data.SelectEastElevationViewTemplate)
                            {
                                if (countElevationMarkers == 0)
                                {
                                    CreateElevationMarkersEast(doc, uiDoc);
                                }
                                else
                                {
                                    ExistElevationMarkersEast(doc, uiDoc);
                                }
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Create Elevation Views", "Dosen't exist View 3D in project.");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        TaskDialog.Show("Create Elevation Views", ex.Message);
                    }

                    if (TransactionStatus.Committed != t.Commit())
                    {
                        TaskDialog.Show("Failure", "Transaction could not be commited");
                    }
                }
                else
                {
                    t.RollBack();
                }
            }
        }

        private void CreateSheets(ExternalCommandData commandData, NewSheetData data)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            UIDocument uiDoc = new UIDocument(doc);
            using (Transaction t = new Transaction(doc))
            {
                if (t.Start("Create Sheets") == TransactionStatus.Started)
                {
                    try
                    {
                        if (null == commandData)
                        {
                            throw new ArgumentNullException("commandData");
                        }

                        List<ElevationMarker> elevationsMarkers = new List<ElevationMarker>();
                        elevationsMarkers = ViewsElevationMarksExists(doc);
                        Family family = null;
                        family = LoadTitleBlockId(doc, data);

                        if (elevationsMarkers.Count() > 0)
                        {
                            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                .OfClass(typeof(FamilySymbol));

                            string nameTitleBlock = data.TitleBlockViewTemplate.Name;

                            foreach (FamilySymbol e in title_block_instances)
                            {
                                if (e.FamilyName == nameTitleBlock)
                                {
                                    family = e.Family;
                                    break;
                                }
                            }
                        }

                        if (data.SelectFloorViewTemplate)
                        {
                            GenerateFloorViewSheet(doc, data, family);
                        }

                        if (data.SelectRoofViewTemplate)
                        {
                            GenerateRoofViewSheet(doc, data, family);
                        }

                        if (data.SelectNorthElevationViewTemplate)
                        {
                            GenerateNorthElevationViewSheet(doc, data, family);
                        }                        

                        if (data.SelectSouthElevationViewTemplate)
                        {
                            GenerateSouthElevationViewSheet(doc, data, family);
                        }

                        if (data.SelectWestElevationViewTemplate)
                        {
                            GenerateWestElevationViewSheet(doc, data, family);
                        }

                        if (data.SelectEastElevationViewTemplate)
                        {
                            GenerateEastElevationViewSheet(doc, data, family);
                        }

                        t.Commit();
                    }
                    catch (Exception ex)
                    {
                        if ((t != null) && t.HasStarted() && !t.HasEnded())
                            t.RollBack();
                    }
                }
                else
                {
                    t.RollBack();
                }
            }
        }

        private View3D View3DExist(Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>();
            if (collector.Count() > 0)
            {
                return collector.First() as View3D;
            }
            else
            {
                return null;
            }
        }

        private List<ElevationMarker> ViewsElevationMarksExists(Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfClass(typeof(ElevationMarker)).Cast<ElevationMarker>();
            return collector.ToList();
        }

        private void CreateElevationMarkersWest(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[0];
            XYZ qPointMarker = GlobalData.corners[1];

            double valAngle = 1.5;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;
            double auxDepth = 0.5;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X - wallType.Width / 2, pPointMarker.Y - wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X - wallType.Width / 2, qPointMarker.Y + wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X - auxDepth, midpointMarker.Y, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.Y) / 100;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor + 0.2;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 0.2;
                    yMinBB1 = midpointMarker.Z - 0.2;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 0.2;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "West Elevation";

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(midpointMarkerElevation.X - width / 2 - 0.4, yMinBB1, 0);
            bb1.Max = new XYZ(midpointMarkerElevation.X + width / 2 + 0.4, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 0.4 + depth + auxDepth;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void CreateElevationMarkersNorth(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[1];
            XYZ qPointMarker = GlobalData.corners[2];

            double valAngle = 1.0;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;
            double auxDepth = 0.5;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X - wallType.Width / 2, pPointMarker.Y + wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X + wallType.Width / 2, qPointMarker.Y + wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y + auxDepth, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.X) / 100;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor + 0.2;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 0.2;
                    yMinBB1 = midpointMarker.Z - 0.2;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 0.2;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "North Elevation";

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(midpointMarkerElevation.X - width / 2 - 0.4, yMinBB1, 0);
            bb1.Max = new XYZ(width + 0.4, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 0.5 + GlobalData.Width + auxDepth;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void CreateElevationMarkersEast(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[2]; //new XYZ(-12 * p.X, p.Y, p.Z);
            XYZ qPointMarker = GlobalData.corners[3]; //new XYZ(-12 * q.X, q.Y, q.Z);

            double valAngle = 0.5;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;
            double auxDepth = 0.5;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X + wallType.Width / 2, pPointMarker.Y + wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X + wallType.Width / 2, qPointMarker.Y - wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X + auxDepth, midpointMarker.Y, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.Y) / 100;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor + 0.2;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 0.2;
                    yMinBB1 = midpointMarker.Z - 0.2;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 0.2;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "East Elevation";

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(midpointMarkerElevation.X - width / 2 - 0.4, yMinBB1, 0);
            bb1.Max = new XYZ(midpointMarkerElevation.X + width / 2 + 0.4, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 0.5 + depth + auxDepth;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void CreateElevationMarkersSouth(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[3]; //new XYZ(-12 * p.X, p.Y, p.Z);
            XYZ qPointMarker = GlobalData.corners[0]; //new XYZ(-12 * q.X, q.Y, q.Z);

            double valAngle = 2.0;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;
            double auxDepth = 0.5;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X + wallType.Width / 2, pPointMarker.Y - wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X - wallType.Width / 2, qPointMarker.Y - wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y - auxDepth, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.X) / 100;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor + 0.2;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 0.2;
                    yMinBB1 = midpointMarker.Z - 0.2;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 0.2;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 0.2;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "South Elevation";

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(midpointMarkerElevation.X - width / 2 - 0.4, yMinBB1, 0);
            bb1.Max = new XYZ(width + 0.4, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 0.5 + GlobalData.Width + auxDepth;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void ExistElevationMarkersWest(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[0];
            XYZ qPointMarker = GlobalData.corners[1];

            double valAngle = 1.0;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X - wallType.Width / 2, pPointMarker.Y - wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X - wallType.Width / 2, qPointMarker.Y + wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X - wallType.Width - 4.5, midpointMarker.Y, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.Y) / 100 + 2.5;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 2.5;
                    yMinBB1 = midpointMarker.Z - 2.5;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 2.5;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "West Elevation";
            elevationView.Scale = scale;

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(-wallType.Width / 2 - 2.5, yMinBB1, 0);
            bb1.Max = new XYZ(width + wallType.Width / 2 + 2.5, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 2.5 + depth + wallType.Width + 4.5;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void ExistElevationMarkersNorth(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[1];
            XYZ qPointMarker = GlobalData.corners[2];

            double valAngle = 0.5;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X - wallType.Width / 2, pPointMarker.Y + wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X + wallType.Width / 2, qPointMarker.Y + wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y + wallType.Width + 4.5, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.X) / 100 + 2.5;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 2.5;
                    yMinBB1 = midpointMarker.Z - 2.5;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 2.5;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "North Elevation";
            elevationView.Scale = scale;

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(midpointMarkerElevation.X + GlobalData.Width - depth + 2.5, yMinBB1, 0);
            bb1.Max = new XYZ(midpointMarkerElevation.X + GlobalData.Width + 7.5, yMaxBB1, 0); //

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 2.5 + GlobalData.Width + wallType.Width + 4.5;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void ExistElevationMarkersEast(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[2];
            XYZ qPointMarker = GlobalData.corners[3];

            double valAngle = 0.0;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X + wallType.Width / 2, pPointMarker.Y + wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X + wallType.Width / 2, qPointMarker.Y - wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X + wallType.Width + 4.5, midpointMarker.Y, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.Y) / 100 + 2.5;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 2.5;
                    yMinBB1 = midpointMarker.Z - 2.5;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 2.5;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "East Elevation";
            elevationView.Scale = scale;

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(-wallType.Width / 2 - 2.5, yMinBB1, 0);
            bb1.Max = new XYZ(width + wallType.Width / 2 + 2.5, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 2.5 + depth + wallType.Width + 4.5;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private void ExistElevationMarkersSouth(Document doc, UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View view = uiDoc.ActiveView;
            int scale = view.Scale;

            ViewFamilyType vft = new FilteredElementCollector(doc).
                    OfClass(typeof(ViewFamilyType)).
                    Cast<ViewFamilyType>().
                    FirstOrDefault(x => ViewFamily.Elevation == x.ViewFamily);

            XYZ pPointMarker = GlobalData.corners[3];
            XYZ qPointMarker = GlobalData.corners[0];

            double valAngle = 1.5;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;

            WallType wallType = GlobalData.wallType;

            if (!GlobalData.slabDrawing)
            {
                pPointMarker = new XYZ(pPointMarker.X + wallType.Width / 2, pPointMarker.Y - wallType.Width / 2, pPointMarker.Z);
                qPointMarker = new XYZ(qPointMarker.X - wallType.Width / 2, qPointMarker.Y - wallType.Width / 2, qPointMarker.Z);
            }

            XYZ vPointMarker = qPointMarker - pPointMarker;
            XYZ midpointMarker = pPointMarker + 0.5 * vPointMarker;
            XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y - wallType.Width - 4.5, midpointMarker.Z);
            double yMaxBB1 = 0.0;
            double yMinBB1 = 0.0;
            width = vPointMarker.GetLength();
            angle = valAngle * Math.PI;

            if (GlobalData.roofDrawing)
            {
                double roofHeignt = (80 * midpointMarker.X) / 100 + 2.5;

                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
                    yMinBB1 = midpointMarker.Z - GlobalData.HeightFloor - 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.ThicknessRoof + 2.5;
                    yMinBB1 = midpointMarker.Z - 2.5;
                }
            }
            else
            {
                if (GlobalData.slabDrawing)
                {
                    yMaxBB1 = midpointMarker.Z + height + GlobalData.HeightFloor + 2.5;
                }
                else
                {
                    yMaxBB1 = midpointMarker.Z + height + 2.5;
                }
            }

            ElevationMarker marker = ElevationMarker.CreateElevationMarker(doc, vft.Id, midpointMarkerElevation, scale);
            ViewSection elevationView = marker.CreateElevation(doc, view.Id, 0);
            elevationView.Name = "South Elevation";
            elevationView.Scale = scale;

            BoundingBoxXYZ bb1 = elevationView.get_BoundingBox(null);
            bb1.Min = new XYZ(-midpointMarkerElevation.X - 7.5, yMinBB1, 0);
            bb1.Max = new XYZ(-midpointMarkerElevation.X + depth - 2.5, yMaxBB1, 0);

            elevationView.CropBox = bb1;
            Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
            double offset = 2.5 + GlobalData.Width + wallType.Width + 4.5;
            param.Set(offset);
            Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
        }

        private Autodesk.Revit.DB.View GetViewPlan(Document doc, ViewType vType)
        {
            FilteredElementCollector viewCollPlan = new FilteredElementCollector(doc);
            viewCollPlan.OfClass(typeof(ViewPlan));
            Autodesk.Revit.DB.View viewPlan = null;

            foreach (Autodesk.Revit.DB.View curView in viewCollPlan)
            {
                if ((!curView.IsTemplate) && (curView.ViewType == vType))
                {
                    viewPlan = curView;
                    break;
                }
            }

            return viewPlan;
        }

        private Autodesk.Revit.DB.View GetViewElevation(Document doc, ViewType vType, string containName, string containSubName)
        {
            FilteredElementCollector viewsColl = new FilteredElementCollector(doc);
            IEnumerable<Autodesk.Revit.DB.View> viewsEnum = viewsColl.OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where(v => !v.IsTemplate && v.ViewType == vType);
            Autodesk.Revit.DB.View viewElevation = null;

            foreach (Autodesk.Revit.DB.View curView in viewsEnum)
            {
                if ((curView.Name.Contains(containName)) && (curView.Name.Contains(containSubName)))
                {
                    viewElevation = curView;
                    break;
                }
            }

            return viewElevation;
        }

        private void GenerateFloorViewSheet(Document doc, NewSheetData data, Family family)
        {
            Autodesk.Revit.DB.View curView = GetViewPlan(doc, ViewType.FloorPlan);
            curView = AsignViewTemplateToFloorView(curView, data);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForFloorViewTemplate(doc, curView, data) : GenerateSheetAndInfoForFloorViewInstanceTemplate(doc, curView, data); //GenerateSheetAndInfoForFloorViewTemplate(doc, curView, data);
            ViewSet viewSetColl = GetAllFloorViewSet(doc);

            PlaceViews(doc, viewSetColl, newSheet, data.TitleBlockViewTemplate.Name);
        }

        private void GenerateRoofViewSheet(Document doc, NewSheetData data, Family family)
        {
            Autodesk.Revit.DB.View curView = GetViewPlan(doc, ViewType.CeilingPlan);
            curView = AsignViewTemplateToRoofView(curView, data);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForRoofViewTemplate(doc, curView, data) : GenerateSheetAndInfoForRoofViewInstanceTemplate(doc, curView, data);
            ViewSet viewSetColl = GetAllRoofViewSet(doc);

            PlaceViews(doc, viewSetColl, newSheet, data.TitleBlockViewTemplate.Name);
        }

        private void GenerateNorthElevationViewSheet(Document doc, NewSheetData data, Family family)
        {
            string name = "North";
            string type = "Elevation";
            Autodesk.Revit.DB.View curView = GetViewElevation(doc, ViewType.Elevation, type, name);
            curView = AsignViewTemplateToElevationView(curView, data, name);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForElevationViewTemplate(doc, curView, data, name) : GenerateSheetAndInfoForElevationViewInstanceTemplate(doc, curView, data, name);
            ViewSet viewSetColl = GetAllElevationViewSet(doc, name, type);

            PlaceViews(doc, viewSetColl, newSheet);
        }

        private void GenerateSouthElevationViewSheet(Document doc, NewSheetData data, Family family)
        {
            string name = "South";
            string type = "Elevation";
            Autodesk.Revit.DB.View curView = GetViewElevation(doc, ViewType.Elevation, type, name);
            curView = AsignViewTemplateToElevationView(curView, data, name);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForElevationViewTemplate(doc, curView, data, name) : GenerateSheetAndInfoForElevationViewInstanceTemplate(doc, curView, data, name);
            ViewSet viewSetColl = GetAllElevationViewSet(doc, name, type);

            PlaceViews(doc, viewSetColl, newSheet);
        }

        private void GenerateWestElevationViewSheet(Document doc, NewSheetData data, Family family)
        {
            string name = "West";
            string type = "Elevation";
            Autodesk.Revit.DB.View curView = GetViewElevation(doc, ViewType.Elevation, type, name);
            curView = AsignViewTemplateToElevationView(curView, data, name);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForElevationViewTemplate(doc, curView, data, name) : GenerateSheetAndInfoForElevationViewInstanceTemplate(doc, curView, data, name);
            ViewSet viewSetColl = GetAllElevationViewSet(doc, name, type);

            PlaceViews(doc, viewSetColl, newSheet);
        }

        private void GenerateEastElevationViewSheet(Document doc, NewSheetData data, Family family)
        {
            string name = "East";
            string type = "Elevation";
            Autodesk.Revit.DB.View curView = GetViewElevation(doc, ViewType.Elevation, type, name);
            curView = AsignViewTemplateToElevationView(curView, data, name);

            ViewSheet newSheet = family != null ? GenerateSheetAndInfoForElevationViewTemplate(doc, curView, data, name) : GenerateSheetAndInfoForElevationViewInstanceTemplate(doc, curView, data, name);
            ViewSet viewSetColl = GetAllElevationViewSet(doc, name, type);

            PlaceViews(doc, viewSetColl, newSheet);
        }

        private Autodesk.Revit.DB.View AsignViewTemplateToRoofView(Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            Autodesk.Revit.DB.View roofViewTemplate = data.RoofViewTemplate as Autodesk.Revit.DB.View;
            if (roofViewTemplate != null)
            {
                curView.ViewTemplateId = roofViewTemplate.Id;
            }

            return curView;
        }

        private Autodesk.Revit.DB.View AsignViewTemplateToFloorView(Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            Autodesk.Revit.DB.View floorViewTemplate = data.FloorViewTemplate as Autodesk.Revit.DB.View;
            if (floorViewTemplate != null)
            {
                curView.ViewTemplateId = floorViewTemplate.Id;
            }

            return curView;
        }

        private Autodesk.Revit.DB.View AsignViewTemplateToElevationView(Autodesk.Revit.DB.View curView, NewSheetData data, string elevationName)
        {
            Autodesk.Revit.DB.View elevationViewTemplate = null;
            switch (elevationName)
            {
                case "North":
                    {
                        elevationViewTemplate = data.NorthElevationViewTemplate as Autodesk.Revit.DB.View;
                        break;
                    }
                case "South":
                    {
                        elevationViewTemplate = data.SouthElevationViewTemplate as Autodesk.Revit.DB.View;
                        break;
                    }
                case "West":
                    {
                        elevationViewTemplate = data.WestElevationViewTemplate as Autodesk.Revit.DB.View;
                        break;
                    }
                case "East":
                    {
                        elevationViewTemplate = data.EastElevationViewTemplate as Autodesk.Revit.DB.View;
                        break;
                    }
                default:
                    break;
            }

            if (elevationViewTemplate != null)
            {
                curView.ViewTemplateId = elevationViewTemplate.Id;
            }

            return curView;
        }

        private ViewSheet GenerateSheetAndInfoForRoofViewTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = curView.ViewType.ToString();
            newSheet_.SheetNumber = data.NameSheetRoofViewTemplate;
            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);

            return newSheet_;
        }

        private ViewSheet GenerateSheetAndInfoForRoofViewInstanceTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks);

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = curView.ViewType.ToString();
            newSheet_.SheetNumber = data.NameSheetRoofViewTemplate;
            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);

            return newSheet_;
        }

        private ViewSheet GenerateSheetAndInfoForFloorViewTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = curView.ViewType.ToString();
            newSheet_.SheetNumber = data.NameSheetFloorViewTemplate;
            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);
            return newSheet_;
        }

        private ViewSheet GenerateSheetAndInfoForFloorViewInstanceTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks);

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = curView.ViewType.ToString();
            newSheet_.SheetNumber = data.NameSheetFloorViewTemplate;
            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);

            return newSheet_;
        }

        private ViewSheet GenerateSheetAndInfoForElevationViewTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data, string nameViewElevation)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = nameViewElevation + " " + curView.ViewType.ToString();

            switch (nameViewElevation)
            {
                case "North":
                    {
                        newSheet_.SheetNumber = data.NameSheetNorthElevationViewTemplate;
                        break;
                    }
                case "South":
                    {
                        newSheet_.SheetNumber = data.NameSheetSouthElevationViewTemplate;
                        break;
                    }
                case "West":
                    {
                        newSheet_.SheetNumber = data.NameSheetWestElevationViewTemplate;
                        break;
                    }
                case "East":
                    {
                        newSheet_.SheetNumber = data.NameSheetEastElevationViewTemplate;
                        break;
                    }
                default:
                    break;
            }

            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);

            return newSheet_;
        }

        private ViewSheet GenerateSheetAndInfoForElevationViewInstanceTemplate(Document doc, Autodesk.Revit.DB.View curView, NewSheetData data, string nameViewElevation)
        {
            ElementId tblockId = null;
            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks);

            foreach (FamilySymbol e in title_block_instances)
            {
                if (e.FamilyName == data.TitleBlockViewTemplate.Name)
                {
                    tblockId = e.Id;
                    break;
                }
            }

            ViewSheet newSheet_;
            newSheet_ = ViewSheet.Create(doc, tblockId);
            newSheet_.Name = nameViewElevation + " " + curView.ViewType.ToString();

            switch (nameViewElevation)
            {
                case "North":
                    {
                        newSheet_.SheetNumber = data.NameSheetNorthElevationViewTemplate;
                        break;
                    }
                case "South":
                    {
                        newSheet_.SheetNumber = data.NameSheetSouthElevationViewTemplate;
                        break;
                    }
                case "West":
                    {
                        newSheet_.SheetNumber = data.NameSheetWestElevationViewTemplate;
                        break;
                    }
                case "East":
                    {
                        newSheet_.SheetNumber = data.NameSheetEastElevationViewTemplate;
                        break;
                    }
                default:
                    break;
            }

            newSheet_ = GenerateCommonInfo(doc, newSheet_, data);

            return newSheet_;
        }

        private ViewSheet GenerateCommonInfo(Document doc, ViewSheet newSheet, NewSheetData data)
        {
            var parm = doc.ProjectInformation.get_Parameter(BuiltInParameter.PROJECT_NAME);
            parm.Set(data.ProjectName);

            parm = doc.ProjectInformation.get_Parameter(BuiltInParameter.PROJECT_NUMBER);
            parm.Set(data.ProjectNumber);

            parm = doc.ProjectInformation.get_Parameter(BuiltInParameter.CLIENT_NAME);
            parm.Set("Client");

            parm = newSheet.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY);
            parm.Set(data.DrawnBy);

            parm = newSheet.get_Parameter(BuiltInParameter.SHEET_CHECKED_BY);
            parm.Set(data.CheckedBy);

            parm = newSheet.get_Parameter(BuiltInParameter.SHEET_APPROVED_BY);
            parm.Set(data.ApprovedBy);

            return newSheet;
        }

        private void PlaceViews(Document doc, ViewSet views, ViewSheet sheet)
        {
            bool ifImperialOrMetric = false;
            switch (doc.DisplayUnitSystem)
            {
                case DisplayUnit.METRIC:
                    {
                        ifImperialOrMetric = false;
                        break;
                    }
                case DisplayUnit.IMPERIAL:
                    {
                        ifImperialOrMetric = true;
                        break;
                    }
                default:
                    break;
            }

            double xDistance = 0;
            double yDistance = 0;
            CalculateDistance(sheet.Outline, views.Size, ref xDistance, ref yDistance);

            double tempU = sheet.Outline.Min.U;
            double tempV = sheet.Outline.Min.V;
            int n = 1;
            Viewport vp = null;
            foreach (Autodesk.Revit.DB.View v in views)
            {
                UV location = new UV(tempU, tempV);
                Autodesk.Revit.DB.View view = v;
                Rescale(view, xDistance, yDistance, ifImperialOrMetric);
                try
                {
                    vp = Viewport.Create(view.Document, sheet.Id, view.Id, new XYZ(0, 0, 0));
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("The view '" + view.Name +
                        "' can't be added, it may have already been placed in another sheet.");
                }

                switch (n)
                {
                    case 1:
                        {
                            vp.SetBoxCenter(new XYZ((location.U + xDistance) / 2, (location.V + yDistance) / 2, 0));
                            break;
                        }
                    case 2:
                        {
                            vp.SetBoxCenter(new XYZ(((location.U + xDistance) / 2) + xDistance, (location.V + yDistance) / 2, 0));
                            break;
                        }
                    case 3:
                        {
                            vp.SetBoxCenter(new XYZ((location.U + xDistance) / 2, ((location.V + yDistance) / 2) + yDistance, 0));
                            break;
                        }
                    case 4:
                        {
                            vp.SetBoxCenter(new XYZ(((location.U + xDistance) / 2) + xDistance, (location.V + yDistance) / 2 + yDistance, 0));
                            break;
                        }
                    default:
                        break;
                }

                n++;
            }
        }

        private void PlaceViews(Document doc, ViewSet views, ViewSheet sheet, string nameTitleBlock)
        {
            Parameter p;
            double width = 0.0;
            double height = 0.0;

            FilteredElementCollector title_block_instances = new FilteredElementCollector(doc);
            title_block_instances = title_block_instances.OfCategory(BuiltInCategory.OST_TitleBlocks)
               .OfClass(typeof(FamilyInstance));

            foreach (FamilyInstance e in title_block_instances)
            {
                string trimFamilyInstance = e.Name.Replace(" ", string.Empty);
                string trimNameTitleBlock = nameTitleBlock.Replace(" ", string.Empty);
                if (trimFamilyInstance == trimNameTitleBlock)
                {
                    p = e.get_Parameter(
                                  BuiltInParameter.SHEET_WIDTH);
                    width = p.AsDouble();

                    p = e.get_Parameter(
                      BuiltInParameter.SHEET_HEIGHT);
                    height = p.AsDouble();
                }
            }

            Autodesk.Revit.DB.View viewColl = null;

            foreach (Autodesk.Revit.DB.View v in views)
            {
                viewColl = v;
                double vpX = viewColl.Outline.Max.U - viewColl.Outline.Min.U;
                double vpY = viewColl.Outline.Max.V - viewColl.Outline.Min.V;

                double xDistance = 0;
                double yDistance = 0;
                CalculateDistance(sheet.Outline, views.Size, ref xDistance, ref yDistance);

                BoundingBoxUV puntoSheet = sheet.Outline;
                viewColl.Scale = Rescale(vpX, vpY, viewColl, width, height);

                try
                {
                    switch (viewColl.ViewType)
                    {
                        case ViewType.FloorPlan:
                            {
                                foreach (ElementId eId in sheet.GetAllViewports())
                                {
                                    doc.Delete(eId);
                                }

                                Viewport vp = Viewport.Create(viewColl.Document, sheet.Id, viewColl.Id, new XYZ(0, 0, 0));

                                break;
                            }
                        case ViewType.CeilingPlan:
                            {
                                foreach (ElementId eId in sheet.GetAllViewports())
                                {
                                    doc.Delete(eId);
                                }

                                Viewport vp = Viewport.Create(viewColl.Document, sheet.Id, viewColl.Id, new XYZ(0, 0, 0));

                                XYZ punto = vp.GetBoxOutline().MinimumPoint;

                                if (puntoSheet.Max.U > puntoSheet.Max.V)
                                {
                                    vp.SetBoxCenter(new XYZ(vp.GetBoxCenter().X + ((puntoSheet.Max.U - puntoSheet.Min.U) / 2), vp.GetBoxCenter().Y + ((puntoSheet.Max.V - puntoSheet.Min.V) / 2), vp.GetBoxCenter().Z));
                                }
                                else
                                {
                                    vp.SetBoxCenter(new XYZ(vp.GetBoxCenter().X + ((puntoSheet.Max.U - puntoSheet.Min.U) / 2), vp.GetBoxCenter().Y + ((puntoSheet.Max.V - puntoSheet.Min.V) / 2), vp.GetBoxCenter().Z));
                                }
                                break;
                            }
                        case ViewType.Elevation:
                            {
                                break;
                            }
                        default:
                            break;
                    }
                }
                catch (ArgumentException /*ae*/)
                {
                    throw new InvalidOperationException("The view '" + viewColl.Name +
                        "' can't be added, it may have already been placed in another sheet.");
                }
            }
        }

        private bool DistanceUp(double realX, double realY, int scale, double width, double height)
        {
            bool result = false;

            double vScaleX = realX / scale;
            double vScaleY = realY / scale;

            if ((vScaleX > width) || (vScaleY > height))
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private bool DistanceDown(double realX, double realY, int scale, double width, double height)
        {
            bool result = false;

            double vScaleX = realX / scale;
            double vScaleY = realY / scale;

            if ((vScaleX < width) && (vScaleY < height))
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void CalculateDistance(BoundingBoxUV bBox, int amount, ref double x, ref double y)
        {
            double xLength = (bBox.Max.U - bBox.Min.U);
            double yLength = (bBox.Max.V - bBox.Min.V);

            double xNewLength = 0.0;
            double yNewLength = 0.0;
            switch (amount)
            {
                case 1:
                    {
                        xNewLength = xLength;
                        yNewLength = yLength;

                        break;
                    }
                case 2:
                    {
                        if (xLength > yLength)
                        {
                            xNewLength = xLength / 2;
                            yNewLength = yLength;
                        }
                        else
                        {
                            xNewLength = xLength;
                            yNewLength = yLength / 2;
                        }

                        break;
                    }
                case 3:
                case 4:
                    {
                        xNewLength = xLength / 2;
                        yNewLength = yLength / 2;

                        break;
                    }

                default:
                    break;
            }

            x = xNewLength;
            y = yNewLength;
        }

        private ViewSet GetAllRoofViewSet(Document doc)
        {
            ViewSet m_allViews = new ViewSet();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator itor = collector.OfClass(typeof(Autodesk.Revit.DB.View)).GetElementIterator();
            itor.Reset();
            while (itor.MoveNext())
            {
                Autodesk.Revit.DB.View view = itor.Current as Autodesk.Revit.DB.View;
                if (null == view || view.IsTemplate)
                {
                    continue;
                }
                else
                {
                    if (view.ViewType == ViewType.CeilingPlan)
                    {
                        m_allViews.Insert(view);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return m_allViews;
        }

        private ViewSet GetAllFloorViewSet(Document doc)
        {
            ViewSet m_allViews = new ViewSet();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator itor = collector.OfClass(typeof(Autodesk.Revit.DB.View)).GetElementIterator();
            itor.Reset();
            while (itor.MoveNext())
            {
                Autodesk.Revit.DB.View view = itor.Current as Autodesk.Revit.DB.View;
                // skip view templates because they're invisible in project browser
                if (null == view || view.IsTemplate)
                {
                    continue;
                }
                else
                {
                    if (view.ViewType == ViewType.FloorPlan)
                    {
                        m_allViews.Insert(view);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return m_allViews;
        }

        private ViewSet GetAllElevationViewSet(Document doc, string nameExist, string typeExist)
        {
            ViewSet m_allViews = new ViewSet();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator itor = collector.OfClass(typeof(Autodesk.Revit.DB.View)).GetElementIterator();
            itor.Reset();
            while (itor.MoveNext())
            {
                Autodesk.Revit.DB.View view = itor.Current as Autodesk.Revit.DB.View;
                if (null == view || view.IsTemplate)
                {
                    continue;
                }
                else
                {
                    if ((view.ViewType == ViewType.Elevation) && (view.Name.Contains(nameExist)) && (view.Name.Contains(typeExist)))
                    {
                        m_allViews.Insert(view);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return m_allViews;
        }

        private Family LoadTitleBlockId(Document doc, NewSheetData data)
        {
            if (data.SelectTitleBlockViewTemplate)
            {
                Family family = null;
                if (data.TitleBlockViewTemplate.Path != string.Empty)
                {
                    doc.LoadFamily(data.TitleBlockViewTemplate.Path, out family);
                }

                return family;
            }
            else
            {
                return null;
            }
        }

        private void Rescale(Autodesk.Revit.DB.View view, double x, double y, bool imprialOrMetric)
        {
            int scalePrev = 0;

            if (imprialOrMetric)
            {
                UV outline = new UV(view.Outline.Max.U - view.Outline.Min.U,
                view.Outline.Max.V - view.Outline.Min.V);

                int index = GlobalImperialScale.imperialScalesList.FindIndex(c => c.valueInteger == view.Scale);
                scalePrev = view.Scale;
                double vpX = 0.0;
                double vpY = 0.0;
                double realvpX = outline.U * scalePrev;
                double realvpY = outline.V * scalePrev;

                if ((outline.U > x) || (outline.V > y))
                {
                    index++;
                    while (index <= GlobalImperialScale.imperialScalesList.Count)
                    {
                        ImperialScale impScales = new ImperialScale();
                        impScales = GlobalImperialScale.imperialScalesList.ElementAt(index);

                        vpX = realvpX / impScales.valueInteger;
                        vpY = realvpY / impScales.valueInteger;

                        if ((vpX > x) || (vpY > y))
                        {

                        }
                        else
                        {
                            scalePrev = impScales.valueInteger;
                            break;
                        }

                        index++;
                    }
                }
                else
                {
                    index--;

                    while (index >= 0)
                    {
                        ImperialScale impScales = new ImperialScale();
                        impScales = GlobalImperialScale.imperialScalesList.ElementAt(index);

                        vpX = (realvpX / 2) / impScales.valueInteger;
                        vpY = realvpY / impScales.valueInteger;

                        view.Scale = impScales.valueInteger;
                        outline = new UV(view.Outline.Max.U - view.Outline.Min.U,
                            view.Outline.Max.V - view.Outline.Min.V);
                        if ((outline.U <= x) && (outline.V <= y))
                        {
                            scalePrev = impScales.valueInteger;
                        }
                        else
                        {
                            break;
                        }

                        index--;
                    }
                }
            }
            else
            {
                UV outline = new UV(view.Outline.Max.U - view.Outline.Min.U,
                view.Outline.Max.V - view.Outline.Min.V);

                int index = GlobalMetricScale.metricScalesList.FindIndex(c => c.valueInteger == view.Scale);
                scalePrev = view.Scale;
                double vpX = 0.0;
                double vpY = 0.0;
                double realvpX = outline.U * scalePrev;
                double realvpY = outline.V * scalePrev;

                if ((outline.U > x) || (outline.V > y))
                {
                    index++;
                    while (index <= GlobalMetricScale.metricScalesList.Count)
                    {
                        MetricScale metScales = new MetricScale();
                        metScales = GlobalMetricScale.metricScalesList.ElementAt(index);

                        vpX = realvpX / metScales.valueInteger;
                        vpY = realvpY / metScales.valueInteger;

                        if ((vpX > x) || (vpY > y))
                        {

                        }
                        else
                        {
                            scalePrev = metScales.valueInteger;
                            break;
                        }

                        index++;
                    }
                }
                else
                {
                    index--;

                    while (index >= 0)
                    {
                        MetricScale metScales = new MetricScale();
                        metScales = GlobalMetricScale.metricScalesList.ElementAt(index);

                        vpX = (realvpX / 2) / metScales.valueInteger;
                        vpY = realvpY / metScales.valueInteger;

                        view.Scale = metScales.valueInteger;
                        outline = new UV(view.Outline.Max.U - view.Outline.Min.U,
                            view.Outline.Max.V - view.Outline.Min.V);
                        if ((outline.U <= x) && (outline.V <= y))
                        {
                            scalePrev = metScales.valueInteger;
                        }
                        else
                        {
                            break;
                        }

                        index--;
                    }
                }
            }

            view.Scale = scalePrev;
        }

        private int Rescale(double vpX, double vpY, Autodesk.Revit.DB.View viewColl, double width, double height)
        {
            double realVpX = vpX;
            double realVpY = vpY;

            int scale = viewColl.Scale;
            List<ImperialScale> listImperialScales = new List<ImperialScale>();
            listImperialScales = GlobalImperialScale.imperialScalesList.OrderBy(c => c.valueInteger).ToList();
            int index = listImperialScales.FindIndex(c => c.valueInteger == scale);

            if ((realVpX > width) || (realVpY > height))
            {
                index++;
                while (index <= listImperialScales.Count)
                {
                    ImperialScale impScales = new ImperialScale();
                    impScales = listImperialScales.ElementAt(index);

                    if (DistanceUp((vpX * scale), (vpY * scale), impScales.valueInteger, width, height))
                    {
                        scale = impScales.valueInteger;
                        break;
                    }

                    index++;
                }
            }
            else if ((realVpX < width) && (realVpY < height))
            {
                index--;
                int scalePrev = viewColl.Scale;
                while (index >= 0)
                {
                    ImperialScale impScales = new ImperialScale();
                    impScales = listImperialScales.ElementAt(index);

                    if (DistanceDown((vpX * scale), (vpY * scale), impScales.valueInteger, width, height))
                    {
                        break;
                    }
                    else
                    {
                        scalePrev = impScales.valueInteger;
                    }

                    index--;
                }

                scale = scalePrev;
            }

            return scale;
        }
    }
}
