using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitServices.Persistence;
using Revit.Elements;
using Revit.Elements.Views;
using System.Collections.ObjectModel;

namespace Revit
{
    /// <summary>
    /// Contains nodes for interacting with Revit Elements
    /// </summary>
    public class ElementData
    {
        private ElementData() { }
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
            ICollection<Autodesk.Revit.DB.Element> phaseElements = collector.OfClass(typeof(Phase)).ToElements();

            var phases = new List<Phase>();
            foreach (Autodesk.Revit.DB.Element element in phaseElements)
            {
                if (element is Phase phase)
                {
                    phases.Add(phase);
                }
            }

            return phases;
        }
        /// <summary>
        /// Looks for all view templates in the project.
        /// DOES NOT INCLUDE 3D VIEW TEMPLATES
        /// There is a missing functionality in Dynamo to cast Revit API View3D to Dynamo views
        /// </summary>
        /// <returns>A list of view templates</returns>
        public static List<Revit.Elements.Element> GetAllViewTemplates ()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Autodesk.Revit.DB.View));

            var viewTemplates = new List<Revit.Elements.Element>();
            foreach (Autodesk.Revit.DB.View view in collector)
            {
                // Check if the view's OwnerViewId is InvalidElementId, which means it's a template
                if (view.IsTemplate)
                {
                    var dynamoView = ElementWrapper.ToDSType(view, true) as Revit.Elements.Element;
                    if(dynamoView != null)
                    {
                        viewTemplates.Add(dynamoView);
                    }
                }
            }
            return viewTemplates;
        }
        /// <summary>
        /// Looks for all USED view templates in the project
        /// </summary>
        /// <returns>A list of view templates</returns>
        public static List<Revit.Elements.Views.View> GetAllUsedViewTemplates()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

                // Fetch all views in the project
                FilteredElementCollector viewCollector = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View));

                // Use LINQ to filter and transform the collection
                List<Revit.Elements.Views.View> usedViewTemplates = viewCollector.Cast<Autodesk.Revit.DB.View>()
                    .Where(view => !view.IsTemplate && view.ViewTemplateId != ElementId.InvalidElementId && view.ViewTemplateId.IntegerValue != -1)
                    .Select(view => ElementWrapper.ToDSType(view, true) as Revit.Elements.Views.View)
                    .ToList();

            return usedViewTemplates;
        }
    }
}
