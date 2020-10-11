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
        List<RibbonItem> _buttom = new List<RibbonItem>();

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
            // Create a ribbon tab
            string tabName = "Beva";

            a.CreateRibbonTab(tabName);

            // Create a ribbon panel inside the tab
            RibbonPanel panel = a.CreateRibbonPanel(tabName, "Beva Tools");

            // Create buttons
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string imagePath = Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Images");
            string helpPath = Path.GetDirectoryName(thisAssemblyPath);

            PushButtonData button1 = new PushButtonData("btnNewProj", "New Project", thisAssemblyPath, typeof(cmdNewProj).FullName);
            PushButton pushButton1 = panel.AddItem(button1) as PushButton;
            pushButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "NewProjectIcon96x96.png")));
            pushButton1.ToolTip = "Start a new project by collecting data from the user. Once you have filled out the form, a new 3D model will automatically be created";
            pushButton1.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "Beva.chm")));

            _buttom.Add(pushButton1);

            PushButtonData button2 = new PushButtonData("btnNewSheet", "New Sheet", thisAssemblyPath, typeof(cmdNewSheet).FullName);
            PushButton pushButton2 = panel.AddItem(button2) as PushButton;
            pushButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "NewSheetIcon96x96.png")));
            pushButton2.ToolTip = "Create a new sheet by collecting data from the user. Once you have filled out the form, a new populated sheet will automatically be created";
            pushButton2.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "Beva.chm")));

            _buttom.Add(pushButton2);

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
                    RibbonItem ribbItem = _buttom[0];
                    ribbItem.Enabled = false;
                } else
                {
                    RibbonItem ribbItem = _buttom[0];
                    ribbItem.Enabled = true;
                }
            }
            else
            {
                RibbonItem ribbItem = _buttom[0];
                ribbItem.Enabled = true;
            }
        }

        private bool ExistAnyElement(Document doc, BuiltInCategory type)
        {
            // Find all Wall instances in the document by using category filter
            ElementCategoryFilter filter = new ElementCategoryFilter(type);

            // Apply the filter to the elements in the active document,
            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> elements = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            return elements.Count > 0 ? true : false;
        }
    }
}
