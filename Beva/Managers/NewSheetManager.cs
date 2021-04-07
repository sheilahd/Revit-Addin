using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        // To store the title blocks templates info in the Revit.
        private List<FamilySymbol> m_titleBlocksTemplates;

        // To store the title blocks string name info in the Revit.
        private List<objSelectList> m_listTitleBlocksNames;

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
            m_titleBlocksTemplates = new List<FamilySymbol>();
            m_listTitleBlocksNames = new List<objSelectList>();

            FilteredElementCollector templatesTypesElementCollector = Utils.GetElementsOfType(doc, typeof(View), BuiltInCategory.OST_Views);
            var m_templatesTypes = templatesTypesElementCollector.Cast<View>().Where(v => v.IsTemplate).ToList();

            FillListViewsTemplates(m_templatesTypes);

            if (m_templatesTypes.Count > 0)
            {
                FilteredElementCollector titleBlocksElementCollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks);
                m_titleBlocksTemplates = titleBlocksElementCollector.OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().OrderBy(c => c.Name).ToList();

                foreach (var item in m_titleBlocksTemplates)
                {
                    List<objSelectList> objList = new List<objSelectList>();
                    objSelectList obj = new objSelectList
                    {
                        Name = item.FamilyName,
                        Value = item.Id.IntegerValue.ToString(),
                        Path = string.Empty
                    };

                    objList.Add(obj);

                    m_listTitleBlocksNames = objList.OrderBy(c => c.Name).ToList();
                }

                ifTamplate = true;
            } else
            {
                FileInfo[] fInfo = null;

                switch (doc.DisplayUnitSystem)
                {
                    case DisplayUnit.METRIC:
                        {
                            string folder = @"C:\ProgramData\Autodesk\RVT " + doc.Application.VersionNumber + @"\Libraries\US Metric\";
                            DirectoryInfo d = new DirectoryInfo(folder);
                            fInfo = d.GetFiles("*.rfa", SearchOption.AllDirectories);
                            break;
                        }
                    case DisplayUnit.IMPERIAL:
                        {
                            string folder = @"C:\ProgramData\Autodesk\RVT " + doc.Application.VersionNumber + @"\Libraries\US Imperial\";
                            DirectoryInfo d = new DirectoryInfo(folder);
                            fInfo = d.GetFiles("*.rfa", SearchOption.AllDirectories);
                            break;
                        }
                    default:
                        break;
                }

                List<objSelectList> objList = new List<objSelectList>();
                foreach (var item in fInfo)
                {
                   if (item.Directory.Name.Equals("Titleblocks"))
                    {
                        objSelectList obj = new objSelectList
                        {
                            Name = item.Name.Trim(item.Extension.ToCharArray()),
                            Value = objList.Count.ToString(),
                            Path = item.FullName
                        };

                        objList.Add(obj);
                    }
                }

                m_listTitleBlocksNames = objList.OrderBy(c => c.Name).ToList();
                ifTamplate = false;
            }           
        }

        private void FillListViewsTemplates(List<View> viewTemplates)
        {
            foreach (View viewItem in viewTemplates)
            {
                if (viewItem.IsTemplate)
                {
                    m_floorViewTemplates.Add(viewItem);
                    m_roofViewTemplates.Add(viewItem);
                    m_elevationViewTemplates.Add(viewItem);
                    //switch (viewItem.ViewType)
                    //{
                    //    case ViewType.FloorPlan:
                    //        {
                    //            m_floorViewTemplates.Add(viewItem);
                    //            break;
                    //        }
                    //    case ViewType.CeilingPlan:
                    //        {
                    //            m_roofViewTemplates.Add(viewItem);
                    //            break;
                    //        }
                    //    case ViewType.Elevation:
                    //        {
                    //            m_elevationViewTemplates.Add(viewItem);
                    //            break;
                    //        }
                    //    default:
                    //        break;
                    //}
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

        public ReadOnlyCollection<FamilySymbol> TitleBlocksViewTemplates
        {
            get
            {
                return new ReadOnlyCollection<FamilySymbol>(m_titleBlocksTemplates);
            }
        }

        public List<objSelectList> TitleBlocksNamesTemplates
        {
            get
            {
                return m_listTitleBlocksNames;
            }
        }

        public bool ifTamplate { get; set; } = false;

        public ExternalCommandData CommandData => m_commandData;
    }

    public class objSelectList
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
    }
}
