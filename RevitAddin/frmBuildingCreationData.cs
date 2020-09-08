using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAddin
{
    public partial class frmBuildingCreationData : Form
    {
        private readonly BuildingManager buildingManager;

        public frmBuildingCreationData()
        {
            InitializeComponent();
        }

        public frmBuildingCreationData(BuildingManager buildingManager): this()
        {
            this.buildingManager = buildingManager;
        }

        private void frmBuildingCreationData_Load(object sender, EventArgs e)
        {
            this.cbRoofType.DataSource = buildingManager.RoofTypes;
            this.cbRoofType.DisplayMember = "Name";
            this.cbRoofType.DropDownStyle = ComboBoxStyle.DropDownList;

            this.cbWallType.DataSource = buildingManager.WallTypes;
            this.cbWallType.DisplayMember = "Name";
            this.cbWallType.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void chbRoofType_CheckedChanged(object sender, EventArgs e)
        {
            cbRoofType.Enabled = chbRoofType.Checked;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TaskDialog.Show("Building creation", "Hello world!");
        }
    }
}
