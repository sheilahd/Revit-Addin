using Autodesk.Revit.DB;
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
    public partial class frmBuildingCreationData : System.Windows.Forms.Form
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
            buildingManager.m_dimX = Convert.ToDouble(tbX.Text);
            buildingManager.m_dimY = Convert.ToDouble(tbY.Text);
            buildingManager.m_dimZ = Convert.ToDouble(tbZ.Text);

            buildingManager.m_lengtn = Convert.ToDouble(tbLength.Text);
            buildingManager.m_width = Convert.ToDouble(tbWidth.Text);
            buildingManager.m_height = Convert.ToDouble(tbHeight.Text);

            buildingManager.m_wallTypeSelect = cbWallType.SelectedValue;
            buildingManager.m_roofTypeSelect = cbRoofType.SelectedValue;

            ExternalCommandData commandData = null;
            string message = string.Empty;
            ElementSet elements = null;

            Command nw = new Command();
            var resultado = nw.Execute(commandData, ref message, elements);
            //buildingManager.m_floorTypeSelect = Convert.ToDouble(TextBox.Text);
            
            //TaskDialog.Show("Building creation", "Hello world!");
        }
    }
}
