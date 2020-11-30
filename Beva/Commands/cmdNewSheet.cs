using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Beva.FormData;
using Beva.Forms;
using Beva.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        View3D view3d = View3DExist(doc);

                        if (view3d != null)
                        {
                            List<ElevationMarker> elevationsMarkers = new List<ElevationMarker>();
                            elevationsMarkers = ViewsElevationMarksExists(doc);
                            if (elevationsMarkers.Count() == 0)
                            {
                                CreateElevationMarkersWest(doc, uiDoc);
								CreateElevationMarkersNorth(doc, uiDoc);
								CreateElevationMarkersEast(doc, uiDoc);
								CreateElevationMarkersSouth(doc, uiDoc);
							} else
                            {
								ExistElevationMarkersWest(doc, uiDoc);
								ExistElevationMarkersNorth(doc, uiDoc);
								ExistElevationMarkersEast(doc, uiDoc);
								ExistElevationMarkersSouth(doc, uiDoc);								
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Create Sheets", "Dosen't exist View 3D in project.");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        TaskDialog.Show("Create Sheets", ex.Message);
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

            XYZ pPointMarker = GlobalData.corners[0]; //new XYZ(-12 * p.X, p.Y, p.Z);
            XYZ qPointMarker = GlobalData.corners[1]; //new XYZ(-12 * q.X, q.Y, q.Z);

            double valAngle = 1.5;
            double width = 0.0;
            double height = GlobalData.Height;
            double depth = GlobalData.Depth;
            double angle = 0.0;
            double auxDepth = 4.5;

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
                    yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			double auxDepth = 4.5;

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
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			double auxDepth = 4.5;

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
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			double auxDepth = 4.5;

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
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			XYZ midpointMarkerElevation = new XYZ(midpointMarker.X - depth, midpointMarker.Y, midpointMarker.Z);
			double yMaxBB1 = 0.0;
			double yMinBB1 = 0.0;
			width = vPointMarker.GetLength();
			angle = valAngle * Math.PI;

			if (GlobalData.roofDrawing)
			{
				double roofHeignt = (80 * midpointMarker.Y) / 100 + 0.5;

				if (GlobalData.slabDrawing)
				{
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			bb1.Min = new XYZ(-wallType.Width / 2, yMinBB1, 0);
			bb1.Max = new XYZ(width + wallType.Width / 2, yMaxBB1, 0);

			elevationView.CropBox = bb1;
			Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
			double offset = 0.4 + depth*2;
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
			XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y + depth, midpointMarker.Z);
			double yMaxBB1 = 0.0;
			double yMinBB1 = 0.0;
			width = vPointMarker.GetLength();
			angle = valAngle * Math.PI;

			if (GlobalData.roofDrawing)
			{
				double roofHeignt = (80 * midpointMarker.X) / 100 + 0.5;

				if (GlobalData.slabDrawing)
				{
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			bb1.Min = new XYZ(midpointMarkerElevation.X + GlobalData.Width - wallType.Width / 2, yMinBB1, 0);
			bb1.Max = new XYZ(midpointMarkerElevation.X + GlobalData.Width + depth + wallType.Width, yMaxBB1, 0); //

			elevationView.CropBox = bb1;
			Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
			double offset = 0.5 + GlobalData.Width + depth;
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
			XYZ midpointMarkerElevation = new XYZ(midpointMarker.X + depth, midpointMarker.Y, midpointMarker.Z);
			double yMaxBB1 = 0.0;
			double yMinBB1 = 0.0;
			width = vPointMarker.GetLength();
			angle = valAngle * Math.PI;

			if (GlobalData.roofDrawing)
			{
				double roofHeignt = (80 * midpointMarker.Y) / 100 + 0.5;

				if (GlobalData.slabDrawing)
				{
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			bb1.Min = new XYZ(-wallType.Width / 2, yMinBB1, 0);
			bb1.Max = new XYZ(width + wallType.Width / 2, yMaxBB1, 0);

			elevationView.CropBox = bb1;
			Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
			double offset = 0.5 + (depth * 2);
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
			XYZ midpointMarkerElevation = new XYZ(midpointMarker.X, midpointMarker.Y - depth, midpointMarker.Z);
			double yMaxBB1 = 0.0;
			double yMinBB1 = 0.0;
			width = vPointMarker.GetLength();
			angle = valAngle * Math.PI;

			if (GlobalData.roofDrawing)
			{
				double roofHeignt = (80 * midpointMarker.X) / 100 + 0.5;

				if (GlobalData.slabDrawing)
				{
					yMaxBB1 = midpointMarker.Z + height + roofHeignt + GlobalData.HeightFloor;
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
			bb1.Min = new XYZ(-midpointMarkerElevation.X - depth - wallType.Width / 2, yMinBB1, 0);
			bb1.Max = new XYZ(-midpointMarkerElevation.X + wallType.Width / 2, yMaxBB1, 0); //

			elevationView.CropBox = bb1;
			Parameter param = elevationView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
			double offset = 0.5 + GlobalData.Width + depth;
			param.Set(offset);
			Line l = Line.CreateBound(midpointMarkerElevation, midpointMarkerElevation + XYZ.BasisZ);
			ElementTransformUtils.RotateElement(doc, marker.Id, l, angle);
		}
	}
}
