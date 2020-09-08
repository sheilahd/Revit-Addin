using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddin
{
    public class BuildingManager
    {
        // To store a reference to the commandData.
        private readonly ExternalCommandData m_commandData;

        // To store the roof types info in the Revit.
        private List<RoofType> m_roofTypes;

        // To store the wall types info in the Revit.
        private List<WallType> m_wallTypes;

        public BuildingManager(ExternalCommandData commandData)
        {
            this.m_commandData = commandData;

            Initialize();
        }

        private void Initialize()
        {
            Document doc = m_commandData.Application.ActiveUIDocument.Document;

            // Search all the roof types in the Revit
            FilteredElementCollector roofTypesElementCollector = new FilteredElementCollector(doc);
            roofTypesElementCollector.OfClass(typeof(RoofType));
            m_roofTypes = roofTypesElementCollector.Cast<RoofType>().ToList();

            // Search all the wall types in the Revit
            FilteredElementCollector wallTypesElementCollector = new FilteredElementCollector(doc);
            wallTypesElementCollector.OfClass(typeof(WallType));
            m_wallTypes = wallTypesElementCollector.Cast<WallType>().ToList();
        }

        public ReadOnlyCollection<RoofType> RoofTypes
        {
            get
            {
                return new ReadOnlyCollection<RoofType>(m_roofTypes);
            }
        }

        public ReadOnlyCollection<WallType> WallTypes
        {
            get
            {
                return new ReadOnlyCollection<WallType>(m_wallTypes);
            }
        }
    }
}
