using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Beva.FormData;
using Beva.Managers;
using System;
using System.Windows.Forms;

namespace Beva.Forms
{
    public partial class frmNewProj : System.Windows.Forms.Form
    {
        private readonly NewProjManager newProjManager;

        public frmNewProj()
        {
            InitializeComponent();
        }

        public NewProjData FormData { get; set; }

        public frmNewProj(NewProjManager newProjManager): this()
        {
            this.newProjManager = newProjManager;
        }

        private void frmNewProj_Load(object sender, EventArgs e)
        {
            this.cbRoofType.DataSource = newProjManager.RoofTypes;
            this.cbRoofType.DisplayMember = "Name";

            this.cbWallType.DataSource = newProjManager.WallTypes;
            this.cbWallType.DisplayMember = "Name";
        }

        private void chbRoofType_CheckedChanged(object sender, EventArgs e)
        {
            cbRoofType.Enabled = chbRoofType.Checked;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cbWallType.SelectedIndex == -1)
            {
                TaskDialog.Show("Data validation", "Please, select the construction type.");
                return;
            }

            if (chbRoofType.Checked && cbRoofType.SelectedIndex == -1)
            {
                TaskDialog.Show("Data validation", "Please, select the roof type.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtX.Text) || !double.TryParse(txtX.Text, out double x))
            {
                TaskDialog.Show("Data validation", "Please, fix the insertion point. There are some invalid values.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtY.Text) || !double.TryParse(txtY.Text, out double y))
            {
                TaskDialog.Show("Data validation", "Please, fix the insertion point. There are some invalid values.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtZ.Text) || !double.TryParse(txtZ.Text, out double z))
            {
                TaskDialog.Show("Data validation", "Please, fix the insertion point. There are some invalid values.");
                return;
            }

            var docUnits = newProjManager.CommandData.Application.ActiveUIDocument.Document.GetUnits();
            var units = newProjManager.CommandData.Application.ActiveUIDocument.Document.DisplayUnitSystem;

            //if (!TryParse(docUnits, txtLength.Text, out double length))
            //{
            //    TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values. The project is in " + units.ToString() + " units.");
            //    return;
            //}

            //if (!TryParse(docUnits, txtWidth.Text, out double width))
            //{
            //    TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values. The project is in " + units.ToString() + " units.");
            //    return;
            //}

            //if (!TryParse(docUnits, txtHeight.Text, out double height))
            //{
            //    TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values. The project is in " + units.ToString() + " units.");
            //    return;
            //}

            FormData = new NewProjData
            {
                WallType = cbWallType.SelectedValue as WallType,
                RoofType = cbRoofType.SelectedValue as RoofType,
                X = x,
                Y = y,
                Z = z,
                //Length = length,
                //Width = width,
                //Height = height,
                DrawingRoof = chbRoofType.Checked,
                DrawingSlab = chbSlab.Checked
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbWallType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //private bool TryParse(Units units, string stringToParse, out double value)
        //{
        //    value = 0;

        //    if (string.IsNullOrWhiteSpace(stringToParse))
        //    {
        //        return false;
        //    }

        //    var valueParsingOptions = new ValueParsingOptions()
        //    {
        //        AllowedValues = AllowedValues.Positive
        //    };

        //    return UnitFormatUtils.TryParse(units, UnitType.UT_Length, stringToParse, valueParsingOptions, out value);
        //}
    }
}
