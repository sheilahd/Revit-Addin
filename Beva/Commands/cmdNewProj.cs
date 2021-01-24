using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Beva.FormData;
using Beva.Forms;
using Beva.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Beva.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class CmdNewProj : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var newProjManager = new NewProjManager(commandData);

                DialogResult result = DialogResult.None;
                using (FrmNewProj form = new FrmNewProj(newProjManager))
                {
                    result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        NewProjData data = form.FormData;

                        RestoreDefaultPartialGlobalClass();

                        CreateHouse(commandData, data);

                        GetSetProjectLocation(commandData, data);

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

        private void CreateHouse(ExternalCommandData commandData, NewProjData data)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            UIDocument uidoc = app.ActiveUIDocument;

            ViewFamilyType viewFamilyType = GetViewFamiliyType(doc);

            using (Transaction t = new Transaction(doc))
            {
                if (t.Start("Create Basic House") == TransactionStatus.Started)
                {
                    View3D view3d = View3D.CreateIsometric(doc, viewFamilyType.Id);
                    view3d.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(3);
                    view3d.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set(6);

                    CreateLevel(doc, data.Height);

                    List<XYZ> corners = new List<XYZ>(4);
                    
                    Level levelBottom = null;
                    Level levelTop = null;

                    List<Wall> walls = CreateWalls(doc, ref corners, data, ref levelBottom, ref levelTop);

                    double wallThickness = walls[0].WallType.Width;

                    if (data.DrawingSlab)
                    {
                        CreateFloor(doc, data, levelBottom, wallThickness, ref corners);

                        GlobalData.slabDrawing = true;
                    }

                    GlobalData.corners = corners;

                    if (data.DrawingRoof)
                    {
                        AddRoof(doc, data, walls);

                        GlobalData.roofDrawing = true;
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

            SetActiveView3D(uidoc, doc);
        }

        private List<Wall> CreateWalls(Document doc, ref List<XYZ> corners, NewProjData formData, ref Level levelBottom, ref Level levelTop)
        {
            if (!Utils.GetBottomAndTopLevels(doc, ref levelBottom, ref levelTop))
            {
                TaskDialog.Show("Create walls", "Unable to determine wall bottom and top levels");
                return null;
            }

            List<Element> wallsTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(WallType), BuiltInCategory.OST_Walls));
            Debug.Assert(0 < wallsTypes.Count, "expected at least one wall type" + " to be loaded into project");
            WallType wallType = wallsTypes.Cast<WallType>().First<Element>(ft => ft.Id == formData.WallType.Id) as WallType;
            GlobalData.wallType = wallType;
            double wallThickness = wallType.Width / 2;
            
            if (wallType == null)
            {
                TaskDialog.Show("Create walls", "Unable to determine wall type.");
                return null;
            }
            
            double widthParam = formData.Width;
            double depthParam = formData.Length;
            double heightParam = formData.Height;
            double xParam = wallThickness;
            double yParam = wallThickness;
            double zParam = 0;

            GlobalData.X = xParam;
            GlobalData.Y = yParam;
            GlobalData.Z = zParam;
            GlobalData.Depth = depthParam;
            GlobalData.Width = widthParam;
            GlobalData.Height = heightParam;

            corners.Add(new XYZ(xParam, yParam, zParam));
            corners.Add(new XYZ(xParam, (widthParam - yParam), zParam));
            corners.Add(new XYZ((depthParam - xParam), (widthParam - yParam), zParam));
            corners.Add(new XYZ((depthParam - xParam), yParam, zParam));

            BuiltInParameter topLevelParam = BuiltInParameter.WALL_HEIGHT_TYPE;
            ElementId levelBottomId = levelBottom.Id;
            levelTop.Elevation = heightParam;
            ElementId topLevelId = levelTop.Id;
            List<Wall> walls = new List<Wall>(4);
            
            List<Curve> geomLine = new List<Curve>();

            for (int i = 0; i < 4; ++i)
            {
                Line line = Line.CreateBound(corners[i], corners[3 == i ? 0 : i + 1]);
                geomLine.Add(line);
                Wall wall = Wall.Create(doc, line, levelBottomId, false);

                Parameter param = wall.get_Parameter(topLevelParam);
                param.Set(topLevelId);
                Parameter paramLocation = wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM);
                paramLocation.Set(2);
                wall.WallType = wallType;
                walls.Add(wall);
            }

            GlobalData.geomLine = geomLine;

            return walls;
        }

        private void CreateFloor(Document doc, NewProjData formData, Level levelBottom, double wallThickness, ref List<XYZ> corners)
        {
            try
            {
                Autodesk.Revit.Creation.Document createDoc = doc.Create;

                double w = 0.5 * wallThickness;
                corners[0] -= w * (XYZ.BasisX + XYZ.BasisY);
                corners[1] -= w * (XYZ.BasisX - XYZ.BasisY);
                corners[2] += w * (XYZ.BasisX + XYZ.BasisY);
                corners[3] += w * (XYZ.BasisX - XYZ.BasisY);

                CurveArray profile = new CurveArray();
                for (int i = 0; i < 4; ++i)
                {
                    Line line = Line.CreateBound( // 2014
                      corners[i], corners[3 == i ? 0 : i + 1]);

                    profile.Append(line);
                }

                List<Element> floorTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(FloorType), BuiltInCategory.OST_Floors));

                Debug.Assert(0 < floorTypes.Count, "expected at least one floor type" + " to be loaded into project");

                FloorType floorType = floorTypes.Cast<FloorType>().FirstOrDefault();

                XYZ normal = XYZ.BasisZ;
                bool structural = false;
                Floor floor = createDoc.NewFloor(profile, floorType, levelBottom, structural, normal);

                Parameter pFloor = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
                GlobalData.HeightFloor = pFloor.AsDouble();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Create Floors", ex.Message);
            }
        }

        private void AddRoof(Document doc, NewProjData formData, List<Wall> walls)
        {
            List<Element> roofTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(RoofType), BuiltInCategory.OST_Roofs));
            RoofType roofType = roofTypes.Cast<RoofType>().First<Element>(rt => rt.Id == formData.RoofType.Id) as RoofType;

            if (roofType == null)
            {
                TaskDialog.Show("Add roof", "Cannot find (" + formData.RoofType + "). Maybe you use a different template? Try with DefaultMetric.rte.");
            }
                        
            double wallThickness = walls[0].Width;

            double dt = wallThickness / 2.0;
            List<XYZ> dts = new List<XYZ>(5)
            {
                new XYZ(-dt, -dt, 0.0),
                new XYZ(-dt, dt, 0.0),
                new XYZ(dt, dt, 0.0),
                new XYZ(dt, -dt, 0.0)
            };
            dts.Add(dts[0]);

            CurveArray footPrint = new CurveArray();
            for (int i = 0; i <= 3; i++)
            {
                LocationCurve locCurve = (LocationCurve)walls[i].Location;
                XYZ pt1 = locCurve.Curve.GetEndPoint(0) + dts[i];
                XYZ pt2 = locCurve.Curve.GetEndPoint(1) + dts[i + 1];
                Line line = Line.CreateBound(pt1, pt2);
                footPrint.Append(line);
            }

            ElementId idLevel2 = walls[0].get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
            Level level2 = (Level)doc.GetElement(idLevel2);

            ModelCurveArray mapping = new ModelCurveArray();
                        
            FootPrintRoof aRoof = doc.Create.NewFootPrintRoof(
              footPrint, level2, roofType, out mapping);

            foreach (ModelCurve modelCurve in mapping)
            {
                aRoof.set_DefinesSlope(modelCurve, true);
                aRoof.set_SlopeAngle(modelCurve, 0.8);
            }

            Parameter pRoof = aRoof.get_Parameter(BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM);//ACTUAL_MAX_RIDGE_HEIGHT_PARAM);
            GlobalData.ThicknessRoof = pRoof.AsDouble();
        }

        private void GetSetProjectLocation(ExternalCommandData commandData, NewProjData data)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Basic House");

                ProjectLocation currentLocation = doc.ActiveProjectLocation;

                XYZ newOrigin = new XYZ(0, 0, 0);

                ProjectPosition projectPosition = currentLocation.GetProjectPosition(newOrigin);

                double angle = 0.0;
                double eastWest = data.X;
                double northSouth = data.Y;
                double elevation = data.Z;

                ProjectPosition newPosition = doc.Application.Create.NewProjectPosition(eastWest, northSouth, elevation, angle);
                if (null != newPosition)
                {
                    currentLocation.SetProjectPosition(newOrigin, newPosition);
                }

                t.Commit();
            }
        }

        private View3D Get3dView(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(View3D));
            foreach (View3D v in collector)
            {
                if (!v.IsTemplate)
                {
                    return v;
                }
            }

            return null;
        }

        private void SetActiveView3D(UIDocument uidoc, Document doc)
        {
            View3D view = Get3dView(doc);
            if (null == view)
            {
                TaskDialog.Show("View 3D", "Sorry, not suitable 3D view found");
            }
            else
            {
                uidoc.ActiveView = view;
            }
        }

        private ViewFamilyType GetViewFamiliyType(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector = collector.OfClass(typeof(ViewFamilyType));
            ViewFamilyType viewFamilyType = collector.Cast<ViewFamilyType>().FirstOrDefault(vfp => vfp.ViewFamily == ViewFamily.ThreeDimensional);

            return viewFamilyType;
        }

        private void CreateLevel(Document doc, double elevation)
        {
            FilteredElementCollector levels = Utils.GetElementsOfType(doc, typeof(Level), BuiltInCategory.OST_Levels);
            int levelsCount = levels.Cast<Level>().ToList().Count();
            if (levelsCount == 0)
            {
                Level level = Level.Create(doc, 0.0);
                level.Name = "Level 1";

                Level level2 = Level.Create(doc, elevation);
                level2.Name = "Level 2";
            }
            else if (levelsCount == 1)
            {
                Level level = Level.Create(doc, elevation);
                level.Name = "Level 2";
            }
        }

        private void RestoreDefaultPartialGlobalClass()
        {
            GlobalData.X = 0.0;
            GlobalData.Y = 0.0;
            GlobalData.Z = 0.0;
            GlobalData.Depth = 0.0;
            GlobalData.Width = 0.0;
            GlobalData.Height = 0.0;
            GlobalData.slabDrawing = false;
            GlobalData.roofDrawing = false;
            GlobalData.HeightFloor = 0.0;
            GlobalData.ThicknessRoof = 0.0;
            GlobalData.geomLine = new List<Curve>();
            GlobalData.corners = new List<XYZ>();
            GlobalData.wallType = null;
        }
    }
}
