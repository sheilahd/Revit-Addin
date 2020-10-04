using Autodesk.Revit.UI;
using Hazen.Commands;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Hazen
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            // Create a ribbon tab
            string tabName = "Hazen";
            a.CreateRibbonTab(tabName);

            // Create a ribbon panel inside the tab
            RibbonPanel hazenPanel = a.CreateRibbonPanel(tabName, "Hazen Tools");

            // Create buttons
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string imagePath = Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Images");
            string helpPath = Path.GetDirectoryName(thisAssemblyPath);

            PushButtonData button1 = new PushButtonData("btnNewProj", "New Project", thisAssemblyPath, typeof(cmdNewProj).FullName);
            PushButton pushButton1 = hazenPanel.AddItem(button1) as PushButton;
            pushButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "HazenIcon96x96.png")));
            pushButton1.ToolTip = "Start a new project by collecting data from the user. Once you have filled out the form, a new 3D model will automatically be created";
            pushButton1.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "Hazen.chm")));

            PushButtonData button2 = new PushButtonData("btnNewSheet", "New Sheet", thisAssemblyPath, typeof(cmdNewSheet).FullName);
            PushButton pushButton2 = hazenPanel.AddItem(button2) as PushButton;
            pushButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "NewSheetIcon96x96.png")));
            pushButton2.ToolTip = "Create a new sheet by collecting data from the user. Once you have filled out the form, a new populated sheet will automatically be created";
            pushButton2.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "Hazen.chm")));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
