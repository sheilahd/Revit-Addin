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

        private void selectAllPlans(Boolean state)
        {
            chkBFloorPlan.Checked = state;
            chkBNorthElevation.Checked = state;
            chkBSouthElevation.Checked = state;
            chkBWestElevation.Checked = state;
            chkBEastElevation.Checked = state;

            txtRoofPlan.Enabled = state;
            cbxRoofPlanTemplate.Enabled = state;
            btnBrowseRoofPlanTemplate.Enabled = state;
            txtFloorPlan.Enabled = state;
            cbxFloorPlanTemplate.Enabled = state;
            btnBrowseFloorPlanTemplate.Enabled = state;

            txtNorthElevation.Enabled = state;
            cbxNorthElevationTemplate.Enabled = state;
            btnBrowseNorthElevationTemplate.Enabled = state;
            txtSouthElevation.Enabled = state;
            cbxSouthElevationTemplate.Enabled = state;
            btnBrowseSouthElevationTemplate.Enabled = state;
            txtWestElevation.Enabled = state;
            cbxWestElevationTemplate.Enabled = state;
            btnBrowseWestElevationTemplate.Enabled = state;
            txtEastElevation.Enabled = state;
            cbxEastElevationTemplate.Enabled = state;
            btnBrowseEastElevationTemplate.Enabled = state;
        }

        private void chkBFloorPlan_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(false);
            setStateFloorPlan(chkBFloorPlan.Checked);
        }

        private void chkBRoofPlan_Click(object sender, EventArgs e)
        {
            selectAllPlans(chkBRoofPlan.Checked);
        }

        private void chkBNorthElevation_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(false);
            setStateNorthElevation(chkBNorthElevation.Checked);            
        }

        private void chkBSouthElevation_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(false);
            setStateSouthElevation(chkBSouthElevation.Checked);
        }

        private void chkBWestElevation_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(false);
            setStateWestElevation(chkBWestElevation.Checked);
        }

        private void chkBEastElevation_Click(object sender, EventArgs e)
        {
            setStateRoofPlan(false);
            setStateEastElevation(chkBEastElevation.Checked);
        }

        private void setStateRoofPlan(Boolean state)
        {
            chkBRoofPlan.Checked = state;
            txtRoofPlan.Enabled = state;
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
    }
}
