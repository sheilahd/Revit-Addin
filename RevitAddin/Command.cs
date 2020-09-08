#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    return Result.Succeeded;
                }

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;

                return Result.Failed;
            }
        }
    }
}
