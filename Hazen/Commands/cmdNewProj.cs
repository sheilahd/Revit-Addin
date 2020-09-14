using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Hazen.FormData;
using Hazen.Forms;
using Hazen.Managers;
using System;
using System.Windows.Forms;

namespace Hazen.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class cmdNewProj : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var newProjManager = new NewProjManager(commandData);

                DialogResult result = DialogResult.None;
                using (frmNewProj form = new frmNewProj(newProjManager))
                {
                    result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        NewProjData data = form.FormData;

                        // pintar la casa

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
    }
}
