using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beva.Managers
{
    public class NewSheetManager
    {
        // To store a reference to the commandData.
        private readonly ExternalCommandData m_commandData;

        public NewSheetManager(ExternalCommandData commandData)
        {
            this.m_commandData = commandData;

            Initialize();
        }

        private void Initialize()
        {
            Document doc = m_commandData.Application.ActiveUIDocument.Document;
        }
    }
}
