using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Beva.Managers
{
    public class NewProjManager
    {
        // To store a reference to the commandData.
        private readonly ExternalCommandData m_commandData;

        // To store the roof types info in the Revit.
        private List<RoofType> m_roofTypes;

        // To store the wall types info in the Revit.
        private List<WallType> m_wallTypes;

        public NewProjManager(ExternalCommandData commandData)
        {
            this.m_commandData = commandData;

            Initialize();
        }

        private void Initialize()
        {
            Document doc = m_commandData.Application.ActiveUIDocument.Document;

            // Search all the roof types in the Revit
            FilteredElementCollector roofTypesElementCollector = Utils.GetElementsOfType(doc, typeof(RoofType), BuiltInCategory.OST_Roofs);
            m_roofTypes = roofTypesElementCollector.Cast<RoofType>()
                .OrderBy(rt => rt.FamilyName)
                .ToList();

            // Search all the wall types in the Revit
            FilteredElementCollector wallTypesElementCollector = Utils.GetElementsOfType(doc, typeof(WallType), BuiltInCategory.OST_Walls);
            m_wallTypes = wallTypesElementCollector.Cast<WallType>()
                .OrderBy(wt => wt.FamilyName)
                .ToList();
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

        public ExternalCommandData CommandData => m_commandData;
    }
}
