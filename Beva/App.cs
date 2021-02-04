using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net; // for HttpStatusCode
using System.Windows.Media.Imaging;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Events;

// Added for REST API
// We are using C# REST library called RestShap
// See http://restsharp.org/ for detail
//  
using RestSharp;
using RestSharp.Deserializers;
using Beva;
using RestSharp.Serialization.Json;
using Microsoft.Exchange.WebServices.Data;

/// Revit 2016 has added two methods to help exchange store app publishers
/// to check a store app entitlement, i.e., to check if the user has purchase or not.
/// This is a minimum sample to show the usage.
///
namespace EntitlementAPIRevit
{
    [Transaction(TransactionMode.Manual)]
    public class EntitlementAPI : IExternalCommand
    {
        // Set values specific to the environment
        public const string _baseApiUrl = @"https://apps.exchange.autodesk.com/";
        // This is the id of your app.
        // e.g.,
        //public const string _appId = @"appstore.exchange.autodesk.com:TransTips-for-Revit:en";
        public const string _appId = @"<the id of your app comes here>";

        // Command to check an entitlement
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get hold of the top elements
            UIApplication uiApp = commandData.Application;
            Application rvtApp = uiApp.Application;

            // Check to see if the user is logged in.
            if (!Application.IsLoggedIn)
            {
                TaskDialog.Show("Entitlement API", "Please login to Autodesk 360 first\n");
                return Result.Failed;
            }

            // Get the user id, and check entitlement
            string userId = rvtApp.LoginUserId;
            private readonly bool isValid = CheckEntitlement(_appId, userId);

            if (isValid)
            { 
                    //The user has a valid entitlement
                    //<YOUR HANDLER CODE HERE>
                    class App : IExternalApplication
                    {
                        readonly List<RibbonItem> _button = new List<RibbonItem>();

                        void OnViewActivated(object sender, ViewActivatedEventArgs e)
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
                            // CREATES THE RIBBON TAB AND RIBBON PANEL
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

                            // DEFINES VARIABLES WITH ALL REQUIRED PATHS
                            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
                            string imagePath = Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Images");
                            string helpPath = Path.GetDirectoryName(thisAssemblyPath);

                            // CREATES PUSH BUTTON 1 ( EZ-BUILD )
                            PushButtonData button1 = new PushButtonData("btnNewProj", "EZ-Build", thisAssemblyPath, typeof(CmdNewProj).FullName);
                            PushButton pushButton1 = panel.AddItem(button1) as PushButton;
                            pushButton1.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "btn1B_EZBuild_32x32.png")));
                            pushButton1.ToolTip = tooltipContentBtn1;
                            pushButton1.LongDescription = tooltipDescriptionBtn1;
                            pushButton1.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "EZevit.chm")));

                            _button.Add(pushButton1);

                            // CREATES PUSH BUTTON 2 ( EZ-SHEET )
                            PushButtonData button2 = new PushButtonData("btnNewSheet", "EZ-Sheet", thisAssemblyPath, typeof(CmdNewSheet).FullName);
                            PushButton pushButton2 = panel.AddItem(button2) as PushButton;
                            pushButton2.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "btn2B_EZSheets_32x32.png")));
                            pushButton2.ToolTip = tooltipContentBtn2;
                            pushButton2.LongDescription = tooltipDescriptionBtn2;
                            pushButton2.SetContextualHelp(new ContextualHelp(ContextualHelpType.ChmFile, Path.Combine(helpPath, "EZevit.chm")));

                            _button.Add(pushButton2);

                            // CREATES PUSH BUTTON 3 ( SUPPORT )
                            PushButtonData button3 = new PushButtonData("btnSupport", "Support", thisAssemblyPath, typeof(CmdSupport).FullName);
                            PushButton pushButton3 = panel.AddItem(button3) as PushButton;
                            pushButton3.LargeImage = new BitmapImage(new Uri(Path.Combine(imagePath, "btn3B_EZSupport_32x32.png")));

                            _button.Add(pushButton3);

                            a.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocChanged);
                            a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);

                            FillUtilsScalesImperial();
                            FillUtilsScalesMetric();

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

                                    RibbonItem ribbItemSheets = _button[1];
                                    ribbItemSheets.Enabled = true;
                                }
                                else
                                {
                                    RibbonItem ribbItem = _button[0];
                                    ribbItem.Enabled = true;

                                    if (ExistAnyElement(doc, BuiltInCategory.OST_Sheets))
                                    {
                                        RibbonItem ribbItemSheets = _button[1];
                                        ribbItemSheets.Enabled = false;
                                    }
                                }
                            }
                            else
                            {
                                RibbonItem ribbItem = _button[0];
                                ribbItem.Enabled = true;

                                RibbonItem ribbItemSheets = _button[1];
                                ribbItemSheets.Enabled = false;
                            }
                        }

                        private bool ExistAnyElement(Document doc, BuiltInCategory type)
                        {
                            ElementCategoryFilter filter = new ElementCategoryFilter(type);
                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            IList<Element> elements = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

                            return elements.Count > 0 ? true : false;
                        }

                        private void FillUtilsScalesImperial()
                        {
                            Utils.FillImperialScales("1\'-0\"", "12\"", 1);
                            Utils.FillImperialScales("1\'-0\"", "6\"", 2);
                            Utils.FillImperialScales("1\'-0\"", "3\"", 4);
                            Utils.FillImperialScales("1\'-0\"", "1 1/2\"", 8);
                            Utils.FillImperialScales("1\'-0\"", "1\"", 12);
                            Utils.FillImperialScales("1\'-0\"", "3/4\"", 16);
                            Utils.FillImperialScales("1\'-0\"", "1/2\"", 24);
                            Utils.FillImperialScales("1\'-0\"", "3/8\"", 32);
                            Utils.FillImperialScales("1\'-0\"", "1/4\"", 48);
                            Utils.FillImperialScales("1\'-0\"", "3/16\"", 64);
                            Utils.FillImperialScales("1\'-0\"", "1/8\"", 96);
                            Utils.FillImperialScales("10\'-0\"", "1\"", 120);
                            Utils.FillImperialScales("1\'-0\"", "3/32\"", 128);
                            Utils.FillImperialScales("1\'-0\"", "1/16\"", 192);
                            Utils.FillImperialScales("20\'-0\"", "1\"", 240);
                            Utils.FillImperialScales("1\'-0\"", "3/64\"", 256);
                            Utils.FillImperialScales("30\'-0\"", "1\"", 360);
                            Utils.FillImperialScales("1\'-0\"", "1/32\"", 384);
                            Utils.FillImperialScales("40\'-0\"", "1\"", 480);
                            Utils.FillImperialScales("50\'-0\"", "1\"", 600);
                            Utils.FillImperialScales("60\'-0\"", "1\"", 720);
                            Utils.FillImperialScales("1\'-0\"", "1/64\"", 768);
                            Utils.FillImperialScales("80\'-0\"", "1\"", 960);
                            Utils.FillImperialScales("100\'-0\"", "1\"", 1200);
                            Utils.FillImperialScales("160\'-0\"", "1\"", 1920);
                            Utils.FillImperialScales("200\'-0\"", "1\"", 2400);
                            Utils.FillImperialScales("300\'-0\"", "1\"", 3600);
                            Utils.FillImperialScales("400\'-0\"", "1\"", 4800);
                        }

                        private void FillUtilsScalesMetric()
                        {
                            Utils.FillMetricScales("1:1", 1);
                            Utils.FillMetricScales("1:2", 2);
                            Utils.FillMetricScales("1:5", 5);
                            Utils.FillMetricScales("1:10", 10);
                            Utils.FillMetricScales("1:20", 20);
                            Utils.FillMetricScales("1:25", 25);
                            Utils.FillMetricScales("1:50", 50);
                            Utils.FillMetricScales("1:100", 100);
                            Utils.FillMetricScales("1:200", 200);
                            Utils.FillMetricScales("1:500", 500);
                            Utils.FillMetricScales("1:1000", 1000);
                            Utils.FillMetricScales("1:2000", 2000);
                            Utils.FillMetricScales("1:5000", 5000);
                        }
                    } 
            }

            // For now, display the result
            private string msg = "userId = " + UserId + "\nappId = " + _appId + "\nisValid = " + isValid.ToString();
            TaskDialog.Show("Entitlement API", msg);

            return Result.Succeeded;
           
        }

        private bool CheckEntitlement(string appId, string userId)
        {
            // REST API call for the entitlement API.
            // We are using RestSharp for simplicity.
            // You may choose to use other library.

            // (1) Build request
            var client = new RestClient();
            client.BaseUrl = new System.Uri(_baseApiUrl);

            // Set resource/end point
            var request = new RestRequest();
            request.Resource = "webservices/checkentitlement";
            request.Method = Method.GET;

            // Add parameters
            request.AddParameter("userid", userId);
            request.AddParameter("appid", appId);

            // (2) Execute request and get response
            IRestResponse response = client.Execute(request);

            // (3) Parse the response and get the value of IsValid.
            bool isValid = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonDeserializer deserial = new JsonDeserializer();
                EntitlementResponse entitlementResponse = deserial.Deserialize<EntitlementResponse>(response);
                isValid = entitlementResponse.IsValid;
            }

            return isValid;
        }

        [Serializable]
        public class EntitlementResponse
        {
            public string UserId { get; set; }
            public string AppId { get; set; }
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }

    }
}
