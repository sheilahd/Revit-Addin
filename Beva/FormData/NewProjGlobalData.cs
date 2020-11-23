using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beva.FormData
{
    public static class GlobalData
    {
        public static double X { get; set; }
        public static double Y { get; set; }
        public static double Z { get; set; }
        public static double Depth { get; set; }
        public static double Width { get; set; }
        public static double Height { get; set; }
        public static double HeightFloor { get; set; }
        public static double ThicknessRoof { get; set; }
        public static bool roofDrawing { get; set; } = false;
        public static bool slabDrawing { get; set; } = false;
        public static List<XYZ> corners { get; set; } = new List<XYZ>();
        public static List<Curve> geomLine { get; set; } = new List<Curve>();
        public static WallType wallType { get; set; }
    }
}
