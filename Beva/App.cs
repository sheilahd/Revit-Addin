using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Beva.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Beva
{
    class App : IExternalApplication
    {
        List<RibbonItem> _button = new List<RibbonItem>();

        void onViewActivated(object sender, ViewActivatedEventArgs e)
        {
            Document doc = e.Document;
            EnabledTabItem(doc);
        }

        void OnDocChanged(object sender, DocumentChangedEventArgs e)
        {
            Document doc = e.GetDocument();
            EnabledTabItem(doc);
        }

        public Result OnStartup(UIControlledApplication a)
        {
            string tabName = "Beva";
            string tooltipContentBtn1 = "Creates a new building by collecting data from the user in a form.";
            string tooltipDescriptionBtn1 = "<p>Use the type selector to specify the type of wall and roof or " +
                "use the default choices and do the changes later.</p>" +
                "<p></p>" +
                "<p>Use the slab checkbox to create a generic floor (slab) " +
                "at the specified height (Project Base Point = PBP => Z-axis)</p>" +
                "<p></p>" +
                "<p>Use the axes XYZ of PBP to locate the building in the Revit world. PBP will be placed at the lower left " +
                "corner of the building and at the top of the slab.</p>" +
                "<p></p>" +
                "<p>Building horizontal dimensions are from outer edge of wall to outer edge of wall. Building height is " +
                "from top of slab to top of wall.</p>";
            string tooltipContentBtn2 = "Creates one or more sheets with viewports to scale showing the existing geometry";
            string tooltipDescriptionBtn2 = "<p>lorem ipsum</p>" +
                "<p></p>" +
                "<p>lorem ipsum</p>" +
                "<p></p>" +
                "<p>lorem ipsum</p>";

            a.CreateRibbonTab(tabName);

            RibbonPanel panel = a.CreateRibbonPanel(tabName, "Beva Tools");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string imagePath = Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Images");
            string helpPath = Path.GetDirectoryName(thisAssemblyPath);

            PushButtonData button1 = new PushButtonData("btnNewProj", "EZ-Build", thisAssemblyPath, typeof(cmdNewProj).FullName);
            PushButton pushButton1 = panel.AddItem(button1) as PushButton;
            pushButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "Xpress-Bldg_Btn32x32.png")));
            pushButton1.ToolTip = tooltipContentBtn1;
            pushButton1.LongDescription = tooltipDescriptionBtn1;
            pushButton1.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "EZevit.chm")));

            _button.Add(pushButton1);

            PushButtonData button2 = new PushButtonData("btnNewSheet", "New Sheet", thisAssemblyPath, typeof(cmdNewSheet).FullName);
            PushButton pushButton2 = panel.AddItem(button2) as PushButton;
            pushButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "Xpress-Bldg_Btn32x32.png")));
            pushButton2.ToolTip = tooltipContentBtn2;
            pushButton2.LongDescription = tooltipDescriptionBtn2;
            pushButton2.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "EZevit.chm")));

            _button.Add(pushButton2);

            a.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocChanged);
            a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(onViewActivated);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private void EnabledTabItem(Document doc)
        {
            if (doc.IsModified)
            {
                if ((ExistAnyElement(doc, BuiltInCategory.OST_Walls)) || (ExistAnyElement(doc, BuiltInCategory.OST_Roofs)) || (ExistAnyElement(doc, BuiltInCategory.OST_Floors)))
                {
                    RibbonItem ribbItem = _button[0];
                    ribbItem.Enabled = false;
                } else
                {
                    RibbonItem ribbItem = _button[0];
                    ribbItem.Enabled = true;
                }
            }
            else
            {
                RibbonItem ribbItem = _button[0];
                ribbItem.Enabled = true;
            }
        }

        private bool ExistAnyElement(Document doc, BuiltInCategory type)
        {
            ElementCategoryFilter filter = new ElementCategoryFilter(type);

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elements = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            return elements.Count > 0 ? true : false;
        }
    }
}
