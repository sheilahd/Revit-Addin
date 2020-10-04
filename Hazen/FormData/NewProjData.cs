using Autodesk.Revit.DB;

namespace Hazen.FormData
{
    public class NewProjData
    {
        public WallType WallType { get; set; }

        public RoofType RoofType { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Length { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool DrawingRoof { get; set; }

        public bool DrawingSlab { get; set; }
    }
}
