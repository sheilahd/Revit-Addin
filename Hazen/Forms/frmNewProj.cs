using Autodesk.Revit.UI;
using Hazen.FormData;
using Hazen.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hazen.Forms
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

            if (string.IsNullOrWhiteSpace(txtLength.Text) || !double.TryParse(txtLength.Text, out double length))
            {
                TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtWidth.Text) || !double.TryParse(txtWidth.Text, out double width))
            {
                TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHeight.Text) || !double.TryParse(txtHeight.Text, out double height))
            {
                TaskDialog.Show("Data validation", "Please, fix the dimensions. There are some invalid values.");
                return;
            }

            FormData = new NewProjData
            {
                WallType = cbWallType.SelectedValue.ToString(),
                RoofType = cbRoofType.SelectedValue.ToString(),
                X = x,
                Y = y,
                Z = z,
                Length = length,
                Width = width,
                Height = height
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
