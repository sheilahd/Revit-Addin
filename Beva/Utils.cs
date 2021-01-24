using Autodesk.Revit.DB;
using Beva.FormData;
using System;
using System.Linq;

namespace Beva
{
    public static class Utils
    {
        const double _feet_to_mm = 25.4 * 12;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="levelBottom"></param>
        /// <param name="levelTop"></param>
        /// <returns></returns>
        public static bool GetBottomAndTopLevels(Document doc, ref Level levelBottom, ref Level levelTop)
        {
            FilteredElementCollector levels = GetElementsOfType(doc, typeof(Level), BuiltInCategory.OST_Levels);

            foreach (Element e in levels)
            {
                if (null == levelBottom)
                {
                    levelBottom = e as Level;
                }
                else if (null == levelTop)
                {
                    levelTop = e as Level;
                }
                else
                {
                    break;
                }
            }

            bool bothLevels = null != levelBottom && null != levelTop;

            if (bothLevels && levelTop.Elevation < levelBottom.Elevation)
            {
                Level tmp = levelTop;
                levelTop = levelBottom;
                levelBottom = tmp;
            }

            return bothLevels;
        }

        /// <summary>
        /// Return all elements of the requested class,
        /// i.e. System.Type, matching the given built-in
        /// category in the given document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="type"></param>
        /// <param name="bic"></param>
        /// <returns></returns>
        public static FilteredElementCollector GetElementsOfType(Document doc, Type type, BuiltInCategory bic)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //collector.OfCategory(bic);
            collector.OfClass(type);

            return collector;
        }

        /// <summary>
        /// Return the midpoint between two points.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static XYZ Midpoint(XYZ p, XYZ q)
        {
            return p + 0.5 * (q - p);
        }

        public static void FillImperialScales(string vFeets, string vInch, int scaleFactor)
        {
            ImperialScale impSc = new ImperialScale
            {
                valueFeet = vFeets,
                valueInch = vInch,
                valueInteger = scaleFactor
            };

            GlobalImperialScale.imperialScalesList.Add(impSc);
        }

        public static void FillMetricScales(string vScales, int scaleFactor)
        {
            MetricScale metSc = new MetricScale
            {
                valueScale = vScales,
                valueInteger = scaleFactor
            };

            GlobalMetricScale.metricScalesList.Add(metSc);
        }

        public static int ConvertFeetToMillimetres(double d)
        {
            //return (int) ( _feet_to_mm * d + 0.5 );
            return (int)Math.Round(_feet_to_mm * d, MidpointRounding.AwayFromZero);
        }

        public static bool SvgFlip = true;

        public static int SvgFlipY(int y)
        {
            return SvgFlip ? -y : y;
        }
    }
}
