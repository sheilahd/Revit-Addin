using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Beva.FormData;
using Beva.Forms;
using Beva.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Beva.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class cmdNewSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var newSheetManager = new NewSheetManager(commandData);

                DialogResult result = DialogResult.None;
                using (frmNewSheet form = new frmNewSheet(newSheetManager))
                {
                    result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        CreateSheets(commandData);

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

        private void CreateSheets(ExternalCommandData commandData)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            UIDocument uidoc = app.ActiveUIDocument;            
        }
    }
}
