using Autodesk.Revit.UI;
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
            objSelectList objS = new objSelectList();
            var sele = cbxRoofPlanTemplate.SelectedValue;
            FormData = new NewSheetData
            {
                SelectFloorViewTemplate = chkBFloorPlan.Checked,
                SelectRoofViewTemplate = chkBRoofPlan.Checked,
                SelectNorthElevationViewTemplate = chkBNorthElevation.Checked,
                SelectSouthElevationViewTemplate = chkBSouthElevation.Checked,
                SelectWestElevationViewTemplate = chkBWestElevation.Checked,
                SelectEastElevationViewTemplate = chkBEastElevation.Checked,
                NameSheetFloorViewTemplate = txtDwgFloorPlan.Text,
                NameSheetRoofViewTemplate = txtDwgRoofPlan.Text,
                NameSheetNorthElevationViewTemplate = txtDwgNorthElevation.Text,
                NameSheetSouthElevationViewTemplate = txtDwgSouthElevation.Text,
                NameSheetWestElevationViewTemplate = txtDwgWestElevation.Text,
                NameSheetEastElevationViewTemplate = txtDwgEastElevation.Text,
                RoofViewTemplate = cbxRoofPlanTemplate.SelectedValue as Autodesk.Revit.DB.View,
                FloorViewTemplate = cbxFloorPlanTemplate.SelectedValue as Autodesk.Revit.DB.View,
                NorthElevationViewTemplate = cbxNorthElevationTemplate.SelectedValue as Autodesk.Revit.DB.View,
                SouthElevationViewTemplate = cbxSouthElevationTemplate.SelectedValue as Autodesk.Revit.DB.View,
                WestElevationViewTemplate = cbxWestElevationTemplate.SelectedValue as Autodesk.Revit.DB.View,
                EastElevationViewTemplate = cbxEastElevationTemplate.SelectedValue as Autodesk.Revit.DB.View,
                SelectTitleBlockViewTemplate = chkBTitleBlock.Checked,
                TitleBlockViewTemplate = cbxTitleBlockTemplate.SelectedValue as objSelectList,
                ProjectName = txtProjectName.Text,
                ProjectNumber = txtProjectNumber.Text,
                Discipline = txtDiscipline.Text,
                DrawnBy = txtDrawnBy.Text,
                CheckedBy = txtCheckedBy.Text,
                ApprovedBy = txtApprovedBy.Text
            };

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
            txtDwgRoofPlan.Enabled = state;
            cbxRoofPlanTemplate.Enabled = state;
        }

        private void setStateFloorPlan(Boolean state)
        {
            txtDwgFloorPlan.Enabled = state;
            cbxFloorPlanTemplate.Enabled = state;
        }

        private void setStateNorthElevation(Boolean state)
        {
            txtDwgNorthElevation.Enabled = state;
            cbxNorthElevationTemplate.Enabled = state;
        }

        private void setStateSouthElevation(Boolean state)
        {
            txtDwgSouthElevation.Enabled = state;
            cbxSouthElevationTemplate.Enabled = state;
        }

        private void setStateWestElevation(Boolean state)
        {
            txtDwgWestElevation.Enabled = state;
            cbxWestElevationTemplate.Enabled = state;
        }

        private void setStateEastElevation(Boolean state)
        {
            txtDwgEastElevation.Enabled = state;
            cbxEastElevationTemplate.Enabled = state;
        }

        private void setStateTitleBlock(Boolean state)
        {
            cbxTitleBlockTemplate.Enabled = state;
        }

        private void chkBTitleBlock_Click(object sender, EventArgs e)
        {
            setStateTitleBlock(chkBTitleBlock.Checked);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearAllPlans(false);
        }

        private void btnBrowseFloorPlanTemplate_Click(object sender, EventArgs e)
        {

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

            this.cbxTitleBlockTemplate.DataSource = newSheetManager.TitleBlocksNamesTemplates;
            this.cbxTitleBlockTemplate.DisplayMember = "Name";           
        }

        private void btnBrowseTitleBlockTemplate_Click(object sender, EventArgs e)
        {
            var app = newSheetManager.CommandData.Application.Application;
            var doc = newSheetManager.CommandData.Application.ActiveUIDocument.Document;

            if (ofdBrowseTitleBlockTemplate.ShowDialog() == DialogResult.OK)
            {
                List<objSelectList> listSelect = new List<objSelectList>();
                listSelect = newSheetManager.TitleBlocksNamesTemplates;

                if (listSelect.Any(c => c.Name == ofdBrowseTitleBlockTemplate.SafeFileName.Split('.')[0]))
                {
                    MessageBox.Show("The selected title block already exist in the list.");
                } else
                {
                    string value = (listSelect.Count + 1).ToString();
                    objSelectList objS = new objSelectList
                    {
                        Name = ofdBrowseTitleBlockTemplate.SafeFileName.Split('.')[0],
                        Value = value,
                        Path = ofdBrowseTitleBlockTemplate.FileName
                    };
                    listSelect.Add(objS);
                    
                    this.cbxTitleBlockTemplate.DataSource = null;
                    this.cbxTitleBlockTemplate.DataSource = listSelect.OrderBy(c => c.Name).ToList();
                    this.cbxTitleBlockTemplate.DisplayMember = "Name";
                }
            }
        }
    }
}
