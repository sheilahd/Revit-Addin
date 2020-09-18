using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Hazen.FormData;
using Hazen.Forms;
using Hazen.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Hazen.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class cmdNewProj : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var newProjManager = new NewProjManager(commandData);

                DialogResult result = DialogResult.None;
                using (frmNewProj form = new frmNewProj(newProjManager))
                {
                    result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        NewProjData data = form.FormData;

                        CreateHouse(commandData, data);

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
            Autodesk.Revit.Creation.Application createApp = app.Application.Create;
            Autodesk.Revit.Creation.Document createDoc = doc.Create;

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Basic House");

                List<XYZ> corners = new List<XYZ>(4);
                List<Wall> walls = new List<Wall>();
                // Determine the levels where the walls will be located:
                Level levelBottom = null;
                Level levelTop = null;
                walls = CreateWalls(doc, data, ref corners, ref levelBottom, ref levelTop);

                double wallThickness = walls[0].WallType.Width;

                //
                // Add door and windows to the first wall;
                //

                XYZ midpoint = Utils.Midpoint(corners[0], corners[1]);
                XYZ p = Utils.Midpoint(corners[0], midpoint);
                XYZ q = Utils.Midpoint(midpoint, corners[1]);
                double tagOffset = 3 * wallThickness;

                //double windowHeight = 1 * LabConstants.MeterToFeet;
                double windowHeight = levelBottom.Elevation + 0.3 * (
                  levelTop.Elevation - levelBottom.Elevation);

                p = new XYZ(p.X, p.Y, windowHeight);
                q = new XYZ(q.X, q.Y, windowHeight);
                Autodesk.Revit.DB.View view = doc.ActiveView;

                midpoint += tagOffset * XYZ.BasisY;

                p += tagOffset * XYZ.BasisY;
                q += tagOffset * XYZ.BasisY;

                CreateFloor(doc, data, levelBottom, wallThickness, ref corners);

                AddRoof(doc, data, walls);

                t.Commit();
            }
        }

        private List<Wall> CreateWalls(Document doc, NewProjData formData, ref List<XYZ> corners, ref Level levelBottom, ref Level levelTop)
        {
            double widthParam = formData.Width * Constants.MeterToFeet;
            double depthParam = formData.Length * Constants.MeterToFeet;
            double heightParam = formData.Height * Constants.MeterToFeet;

            corners.Add(new XYZ(formData.X, formData.Y, formData.Z));
            corners.Add(new XYZ(widthParam, formData.Y, formData.Z));
            corners.Add(new XYZ(widthParam, depthParam, formData.Z));
            corners.Add(new XYZ(formData.X, depthParam, formData.Z));

            if (!Utils.GetBottomAndTopLevels(doc, ref levelBottom, ref levelTop))
            {
                TaskDialog.Show("Create walls", "Unable to determine wall bottom and top levels");
                return null;
            }

            List<Element> wallsTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(WallType), BuiltInCategory.OST_Walls));
            Debug.Assert(0 < wallsTypes.Count, "expected at least one wall type" + " to be loaded into project");
            WallType wallType = wallsTypes.Cast<WallType>().First<Element>(ft => ft.Id == formData.WallType.Id) as WallType;

            if (wallType == null)
            {
                TaskDialog.Show("Create walls", "Unable to determine wall type.");
                return null;
            }

            BuiltInParameter topLevelParam = BuiltInParameter.WALL_HEIGHT_TYPE;
            ElementId levelBottomId = levelBottom.Id;
            ElementId topLevelId = levelTop.Id;
            List<Wall> walls = new List<Wall>(4);

            List<Curve> geomLine = new List<Curve>();

            for (int i = 0; i < 4; ++i)
            {
                Line line = Line.CreateBound(corners[i], corners[3 == i ? 0 : i + 1]);
                Wall wall = Wall.Create(doc, line, levelBottomId, false); // 2013
                Parameter param = wall.get_Parameter(topLevelParam);
                param.Set(topLevelId);
                wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(formData.Z);
                wall.WallType = wallType;

                walls.Add(wall);
            }

            return walls;
        }

        private void CreateFloor(Document doc, NewProjData formData, Level levelBottom, double wallThickness, ref List<XYZ> corners)
        {
            try
            {
                Autodesk.Revit.Creation.Document createDoc = doc.Create;

                double w = 0.5 * wallThickness;
                corners[0] -= w * (XYZ.BasisX + XYZ.BasisY);
                corners[1] += w * (XYZ.BasisX - XYZ.BasisY);
                corners[2] += w * (XYZ.BasisX + XYZ.BasisY);
                corners[3] -= w * (XYZ.BasisX - XYZ.BasisY);
                CurveArray profile = new CurveArray();
                for (int i = 0; i < 4; ++i)
                {
                    Line line = Line.CreateBound( // 2014
                      corners[i], corners[3 == i ? 0 : i + 1]);

                    profile.Append(line);
                }

                List<Element> floorTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(FloorType), BuiltInCategory.OST_Floors));

                Debug.Assert(0 < floorTypes.Count, "expected at least one floor type" + " to be loaded into project");

                FloorType floorType = floorTypes.Cast<FloorType>().FirstOrDefault(); //First<Element>(ft => ft.Id == floorTypeSelect.Id) as FloorType;

                XYZ normal = XYZ.BasisZ;

                bool structural = false;
                Floor floor = createDoc.NewFloor(profile, floorType, levelBottom, structural, normal);
                Parameter p1 = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                p1.Set(formData.Z);
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
            List<XYZ> dts = new List<XYZ>(5);
            dts.Add(new XYZ(-dt, -dt, 0.0));
            dts.Add(new XYZ(dt, -dt, 0.0));
            dts.Add(new XYZ(dt, dt, 0.0));
            dts.Add(new XYZ(-dt, dt, 0.0));
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

            // Get the level2 from the wall 

            ElementId idLevel2 = walls[0].get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
            //Level level2 = (Level)_doc.get_Element(idLevel2); // 2012
            Level level2 = (Level)doc.GetElement(idLevel2); // since 2013

            ModelCurveArray mapping = new ModelCurveArray();

            FootPrintRoof aRoof = doc.Create.NewFootPrintRoof(
              footPrint, level2, roofType, out mapping);

            foreach (ModelCurve modelCurve in mapping)
            {
                aRoof.set_DefinesSlope(modelCurve, true);
                aRoof.set_SlopeAngle(modelCurve, 0.8);
            }
        }
    }
}
