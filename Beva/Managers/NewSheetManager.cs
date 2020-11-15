using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beva.Managers
{
    public class NewSheetManager
    {
        // To store a reference to the commandData.
        private readonly ExternalCommandData m_commandData;

        // To store the roof views templates info in the Revit.
        private List<View> m_roofViewTemplates;

        // To store the floor views templates info in the Revit.
        private List<View> m_floorViewTemplates;

        // To store the roof views templates info in the Revit.
        private List<View> m_elevationViewTemplates;

        public NewSheetManager(ExternalCommandData commandData)
        {
            this.m_commandData = commandData;

            Initialize();
        }

        private void Initialize()
        {
            Document doc = m_commandData.Application.ActiveUIDocument.Document;

            m_roofViewTemplates = new List<View>();
            m_floorViewTemplates = new List<View>();
            m_elevationViewTemplates = new List<View>();

            FilteredElementCollector templatesTypesElementCollector = Utils.GetElementsOfType(doc, typeof(View), BuiltInCategory.OST_Views);
            var m_templatesTypes = templatesTypesElementCollector.Cast<View>().ToList();

            FillListTemplates(m_templatesTypes);
        }

        private void FillListTemplates(List<View> viewTemplates)
        {
            foreach (View viewItem in viewTemplates)
            {
                if (viewItem.IsTemplate)
                {
                    switch (viewItem.ViewType)
                    {
                        case ViewType.FloorPlan:
                            {
                                m_floorViewTemplates.Add(viewItem);
                                break;
                            }
                        case ViewType.CeilingPlan:
                            {
                                m_roofViewTemplates.Add(viewItem);
                                break;
                            }
                        case ViewType.Elevation:
                            {
                                m_elevationViewTemplates.Add(viewItem);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            m_floorViewTemplates = m_floorViewTemplates.OrderBy(c => c.Name).ToList();
            m_roofViewTemplates = m_roofViewTemplates.OrderBy(c => c.Name).ToList();
            m_elevationViewTemplates = m_elevationViewTemplates.OrderBy(c => c.Name).ToList();
        }

        public ReadOnlyCollection<View> RoofViewTemplates
        {
            get
            {
                return new ReadOnlyCollection<View>(m_roofViewTemplates);
            }
        }

        public ReadOnlyCollection<View> FloorViewTemplates
        {
            get
            {
                return new ReadOnlyCollection<View>(m_floorViewTemplates);
            }
        }

        public ReadOnlyCollection<View> ElevationViewTemplates
        {
            get
            {
                return new ReadOnlyCollection<View>(m_elevationViewTemplates);
            }
        }

        public ExternalCommandData CommandData => m_commandData;
    }
}
