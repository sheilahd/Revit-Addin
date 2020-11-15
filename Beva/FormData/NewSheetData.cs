using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beva.FormData
{
    public class NewSheetData
    {
        public View FloorViewTemplate { get; set; }

        public View RoofViewTemplate { get; set; }

        public View NorthElevationViewTemplate { get; set; }

        public View SouthElevationViewTemplate { get; set; }

        public View WestElevationViewTemplate { get; set; }

        public View EastElevationViewTemplate { get; set; }

        public bool SelectRoofViewTemplate { get; set; }

        public bool SelectFloorViewTemplate { get; set; }

        public bool SelectNorthElevationViewTemplate { get; set; }

        public bool SelectSouthElevationViewTemplate { get; set; }

        public bool SelectWestElevationViewTemplate { get; set; }

        public bool SelectEastElevationViewTemplate { get; set; }

        public View TitleBlockViewTemplate { get; set; }

        public bool SelectTitleBlockViewTemplate { get; set; }

        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }

        public string Discipline { get; set; }

        public string DrawnBy { get; set; }

        public string CheckedBy { get; set; }

        public string ApprovedBy { get; set; }
    }
}
