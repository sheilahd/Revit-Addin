using Beva.FormData;
using Beva.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Beva.Forms
{
    public partial class frmNewSheet : Form
    {
        private readonly NewSheetManager newSheetManager;

        public frmNewSheet()
        {
            InitializeComponent();
        }

        public NewSheetData FormData { get; set; }

        public frmNewSheet(NewSheetManager newSheetManager) : this()
        {
            this.newSheetManager = newSheetManager;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void clearAllPlans(Boolean state)
        {
            chkBFloorPlan.Checked = state;
            chkBRoofPlan.Checked = state;
            chkBNorthElevation.Checked = state;
            chkBSouthElevation.Checked = state;
            chkBWestElevation.Checked = state;
            chkBEastElevation.Checked = state;
            chkBTitleBlock.Checked = state;

            setStateFloorPlan(state);
            setStateRoofPlan(state);
            setStateNorthElevation(state);
            setStateSouthElevation(state);
            setStateWestElevation(state);
            setStateEastElevation(state);
            setStateTitleBlock(state);

            txtProjectName.Text = string.Empty;
            txtProjectNumber.Text = string.Empty;
            txtDiscipline.Text = string.Empty;
            txtDrawnBy.Text = string.Empty;
            txtCheckedBy.Text = string.Empty;
            txtApprovedBy.Text = string.Empty;
        }

        private void chkBFloorPlan_Click(object sender, EventArgs e)
        {
            setStateFloorPlan(chkBFloorPlan.Checked);
        }

        private void chkBRoofPlan_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(chkBRoofPlan.Checked);
        }

        private void chkBNorthElevation_Click(object sender, EventArgs e)
        {
            setStateNorthElevation(chkBNorthElevation.Checked);            
        }

        private void chkBSouthElevation_Click(object sender, EventArgs e)
        {
            setStateSouthElevation(chkBSouthElevation.Checked);
        }

        private void chkBWestElevation_Click(object sender, EventArgs e)
        {
            setStateWestElevation(chkBWestElevation.Checked);
        }

        private void chkBEastElevation_Click(object sender, EventArgs e)
        {
            setStateEastElevation(chkBEastElevation.Checked);
        }

        private void setStateRoofPlan(Boolean state)
        {
            txtRoofPlan.Enabled = state;
            cbxRoofPlanTemplate.Enabled = state;
            btnBrowseRoofPlanTemplate.Enabled = state;
        }

        private void setStateFloorPlan(Boolean state)
        {
            txtFloorPlan.Enabled = state;
            cbxFloorPlanTemplate.Enabled = state;
            btnBrowseFloorPlanTemplate.Enabled = state;
        }

        private void setStateNorthElevation(Boolean state)
        {
            txtNorthElevation.Enabled = state;
            cbxNorthElevationTemplate.Enabled = state;
            btnBrowseNorthElevationTemplate.Enabled = state;
        }

        private void setStateSouthElevation(Boolean state)
        {
            txtSouthElevation.Enabled = state;
            cbxSouthElevationTemplate.Enabled = state;
            btnBrowseSouthElevationTemplate.Enabled = state;
        }

        private void setStateWestElevation(Boolean state)
        {
            txtWestElevation.Enabled = state;
            cbxWestElevationTemplate.Enabled = state;
            btnBrowseWestElevationTemplate.Enabled = state;
        }

        private void setStateEastElevation(Boolean state)
        {
            txtEastElevation.Enabled = state;
            cbxEastElevationTemplate.Enabled = state;
            btnBrowseEastElevationTemplate.Enabled = state;
        }

        private void setStateTitleBlock(Boolean state)
        {
            cbxTitleBlockTemplate.Enabled = state;
            btnBrowseTitleBlockTemplate.Enabled = state;
        }

        private void chkBTitleBlock_Click(object sender, EventArgs e)
        {
            setStateTitleBlock(chkBTitleBlock.Checked);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearAllPlans(false);
        }

        private void setTitleOpenFileDialog(string title)
        {
            ofdViewsTemplates.Title = title;
        }

        private void btnBrowseFloorPlanTemplate_Click(object sender, EventArgs e)
        {
            setTitleOpenFileDialog("Browse floor plan template");

            if (ofdViewsTemplates.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void frmNewSheet_Load(object sender, EventArgs e)
        {
            this.cbxFloorPlanTemplate.DataSource = newSheetManager.FloorViewTemplates;
            this.cbxFloorPlanTemplate.DisplayMember = "Name";

            this.cbxRoofPlanTemplate.DataSource = newSheetManager.RoofViewTemplates;
            this.cbxRoofPlanTemplate.DisplayMember = "Name";

            this.cbxNorthElevationTemplate.DataSource = newSheetManager.ElevationViewTemplates;
            this.cbxNorthElevationTemplate.DisplayMember = "Name";

            this.cbxSouthElevationTemplate.DataSource = newSheetManager.ElevationViewTemplates;
            this.cbxSouthElevationTemplate.DisplayMember = "Name";

            this.cbxWestElevationTemplate.DataSource = newSheetManager.ElevationViewTemplates;
            this.cbxWestElevationTemplate.DisplayMember = "Name";

            this.cbxEastElevationTemplate.DataSource = newSheetManager.ElevationViewTemplates;
            this.cbxEastElevationTemplate.DisplayMember = "Name";
        }
    }
}
