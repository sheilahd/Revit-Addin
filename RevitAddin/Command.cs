#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace RevitAddin
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var buildingManager = new BuildingManager(commandData);

                DialogResult result = DialogResult.None;
                using (frmBuildingCreationData form = new frmBuildingCreationData(buildingManager))
                {
                    result = form.ShowDialog();
                }

                if (result == DialogResult.OK)
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
                        walls = CreateWalls(doc, buildingManager, ref corners, ref levelBottom, ref levelTop);

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

                        CreateFloor(doc, buildingManager, levelBottom, wallThickness, ref corners);

                        AddRoof(doc, buildingManager, walls);

                        t.Commit();

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

        private List<Wall> CreateWalls(Document doc, BuildingManager buildingManager, ref List<XYZ> corners, ref Level levelBottom, ref Level levelTop)
        {
            double widthParam = buildingManager.m_width * Constants.MeterToFeet;
            double depthParam = buildingManager.m_length * Constants.MeterToFeet;
            double heightParam = buildingManager.m_height * Constants.MeterToFeet;

            corners.Add(new XYZ(buildingManager.m_dimX, buildingManager.m_dimY, buildingManager.m_dimZ));
            corners.Add(new XYZ(widthParam, buildingManager.m_dimY, buildingManager.m_dimZ));
            corners.Add(new XYZ(widthParam, depthParam, buildingManager.m_dimZ));
            corners.Add(new XYZ(buildingManager.m_dimX, depthParam, buildingManager.m_dimZ));

            if (!Utils.GetBottomAndTopLevels(doc, ref levelBottom, ref levelTop))
            {
                TaskDialog.Show("Create walls", "Unable to determine wall bottom and top levels");
                return null;
            }

            List<Element> wallsTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(WallType), BuiltInCategory.OST_Walls));
            Debug.Assert(0 < wallsTypes.Count, "expected at least one wall type" + " to be loaded into project");
            WallType wallTypeSelect = buildingManager.m_wallTypeSelect as WallType;
            WallType wallType = wallsTypes.Cast<WallType>().First<Element>(ft => ft.Id == wallTypeSelect.Id) as WallType;

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
                wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(buildingManager.m_dimZ);
                wall.WallType = wallType;

                walls.Add(wall);
            }

            return walls;
        }

        private void CreateFloor(Document doc, BuildingManager buildingManager, Level levelBottom, double wallThickness, ref List<XYZ> corners)
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
                p1.Set(buildingManager.m_dimZ);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Create Floors", ex.Message);
            }
        }

        public void AddRoof(Document doc, BuildingManager buildingManager, List<Wall> walls)
        {
            List<Element> roofTypes = new List<Element>(Utils.GetElementsOfType(doc, typeof(RoofType), BuiltInCategory.OST_Roofs));

            ElementId roofTypeSelect = buildingManager.m_roofTypeSelect as ElementId;
            RoofType roofType = roofTypes.Cast<RoofType>().First<Element>(rt => rt.Id == roofTypeSelect) as RoofType;

            if (roofType == null)
            {
                TaskDialog.Show("Add roof", "Cannot find (" + roofTypeSelect + "). Maybe you use a different template? Try with DefaultMetric.rte.");
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
