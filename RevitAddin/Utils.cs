using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddin
{
    public static class Utils
    {
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

            if (levelTop.Elevation < levelBottom.Elevation)
            {
                Level tmp = levelTop;
                levelTop = levelBottom;
                levelBottom = tmp;
            }
            return null != levelBottom && null != levelTop;
        }

        #region GetElementsOfType:
        /// <summary>
        /// Return all elements of the requested class,
        /// i.e. System.Type, matching the given built-in
        /// category in the given document.
        /// </summary>
        public static FilteredElementCollector GetElementsOfType( Document doc, Type type, BuiltInCategory bic)
        {
            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.OfCategory(bic);
            collector.OfClass(type);

            return collector;
        }
        #endregion // GetElementsOfType

        #region Geometry utilities
        /// <summary>
        /// Return the midpoint between two points.
        /// </summary>
        public static XYZ Midpoint(XYZ p, XYZ q)
        {
            return p + 0.5 * (q - p);
        }
        #endregion // Geometry utilities
    }
}
