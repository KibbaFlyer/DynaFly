using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitServices.Persistence;

namespace Revit
{
    /// <summary>
    /// Contains nodes for interacting with Phases
    /// </summary>
    public class Elements
    {
        private Elements() { }
        /// <summary>
        /// Gets a list of all phases in the current Revit document.
        /// Is different from the OOTB-node in that it returns Elements of type Autodesk.Revit.DB.Phase instead of Revit.Elements.Phase
        /// Used for IFCExportOptions
        /// </summary>
        /// <returns>A list of phases.</returns>
        public static List<Phase> GetAllPhases()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            // Collecting all the phases in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> phaseElements = collector.OfClass(typeof(Phase)).ToElements();

            var phases = new List<Phase>();
            foreach (Element element in phaseElements)
            {
                if (element is Phase phase)
                {
                    phases.Add(phase);
                }
            }

            return phases;
        }
    }
}
