using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data
{
    /// <summary>
    /// Exchanges data between Revit and other applications.
    /// </summary>
    public class IFC
    {
        private IFC() { }
        /// <summary>
        /// Exports chosen views with chosen settings to IFC.
        /// </summary>
        /// <param name="names">The collection of view names to be exported (as a list of strings).</param>
        /// <param name="viewIds">The collection of ElementIds for the views to be exported (as a list of int).</param>
        /// <param name="path">The file path to save the IFC file.</param>
        /// <param name="ifcOptions">Settings for IFC export. Note: Use the node "IFC.CreateIFCExportOptions" to create the settings.</param>
        /// <returns>true if the export is successful; otherwise, false.</returns>
        public static Tuple<List<View>, List<string>, List<string>, List<bool>> ExportViews(List<string> names, List<int> viewIds, string path, IFCExportOptions ifcOptions)
        {
            List<View> viewsToReturn = new List<View>();
            List<string> filepathsToReturn = new List<string>();
            List<string> timestampsToReturn = new List<string>();
            List<bool> exportStatus = new List<bool>();

            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            UIApplication uIApplication = DocumentManager.Instance.CurrentUIApplication;
            // Check if the document, viewIds, or path is null
            if (doc == null || viewIds == null || string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("One or more arguments are null or invalid.");
            }
            if (viewIds.Count() != names.Count())
            {
                throw new ArgumentNullException("The lists of names and views must be the same length");
            }

            // If the user does not provide specific IFC export options, create default options
            if (ifcOptions == null)
            {
                ifcOptions = new IFCExportOptions();
            }
            try
            {
                using (var enumName = names.GetEnumerator())
                using (var enumViews = viewIds.GetEnumerator())
                    while (enumName.MoveNext() && enumViews.MoveNext())
                    {
                        var currentName = enumName.Current;
                        var currentView = enumViews.Current;
                        ElementId elementId = new ElementId(currentView);
                        View view = doc.GetElement(elementId) as View;

                        // Populate the return lists
                        viewsToReturn.Add(view);
                        filepathsToReturn.Add(System.IO.Path.Combine(path, currentName + ".ifc"));
                        timestampsToReturn.Add(DateTime.Now.ToString());

                        // Time to get the viewId
                        ifcOptions.AddOption("ActiveViewId", elementId.ToString());
                        ifcOptions.FilterViewId = elementId;

                        // Execute the export command
                        doc.Export(path, currentName, ifcOptions);

                        // Add true if the export succeeded
                        exportStatus.Add(true);
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during IFC export: " + ex.Message);
                // Add false if the export didn't succeed
                exportStatus.Add(false);
                // Populate the return lists
                viewsToReturn.Add(null);
                filepathsToReturn.Add(null);
                timestampsToReturn.Add(null);
            }
            return new Tuple<List<View>, List<string>, List<string>, List<bool>>(viewsToReturn, filepathsToReturn, timestampsToReturn, exportStatus);

        }

        /// <summary>
        /// Configure and return IFC export options based on requirements.
        /// </summary>
        /// <param name="Phase">
        /// Selects the phase of the document to export, defaulting to the last phase of the document. If "Export only elements visible in view" is selected on the Additional Content tab, the phase of the view is used and this option is disabled.<br/>
        /// Note: Use the node "Elements.GetAllPhases" to get all phases in the document in the object type.
        /// </param>
        /// <param name="ExchangeRequirement">
        /// Specifies the exchange requirement.
        /// </param>
        /// <param name="Export2DElements">
        /// Select this option to include 2D elements supported by IFC export (notes and filled regions). Clear the option to exclude these elements.
        /// </param>
        /// <param name="ExportBaseQuantities">
        /// Select this option to include base quantities for model elements in the export data. Base quantities are generated from model geometry to reflect actual physical quantity values, independent of measurement rules or methods.
        /// </param>
        /// <param name="ExportBoundingBox">
        /// Select this option to export bounding box representations. Clear the option to exclude them. This option is automatically selected for GSA export.
        /// </param>
        /// <param name="ExportIFCCommonPropertySets">
        /// Select this option to include the IFC common property sets. Clear the option to exclude them.
        /// </param>
        /// <param name="ExportInternalRevitPropertySets">
        /// Select this option to include the Revit-specific property sets based on parameter groups. Clear the option to exclude them.
        /// </param>
        /// <param name="ExportLinkedFiles">
        /// Select this option to export Revit links in the model as separate IFCs. Clear this option to exclude Revit links.<br/>
        /// Each linked instance in the file is exported as a separate IFC file with the correct positioning relative to the host file.<br/>
        /// Multiple instances of the same linked document are exported as separate files, identical except for their position and orientation.<br/>
        /// Note: Linked IFC files are re-exported, so they may suffer data loss on roundtrip.
        /// </param>
        /// <param name="ExportPartsAsBuildingElements">
        /// Select this option to export parts as standard IFC elements. Clear the option to export them as IfcBuildingElementPart.
        /// </param>
        /// <param name="ExportRoomsInView">
        /// If the "Export only elements visible in view" option is selected, selecting this option exports all rooms contained inside the section box of the selected 3D view. If there is no active section box, all rooms are exported.
        /// </param>
        /// <param name="ExportSchedulesAsPsets">
        /// Select this option to export schedules as custom property sets. The name of the schedule is the property set name; the column names are the IFC parameter names.
        /// </param>
        /// <param name="ExportSolidModelRep">
        /// Select this option to allow for mixing BRep and extrusion geometries for an entity. This can result in smaller IFC files, but the files are not strictly within the standard IFC MVDs.
        /// </param>
        /// <param name="ExportSpecificSchedules">
        /// When selected, restricts the schedules exported to only those that contain "IFC," "PSet," or "Common," in the title.
        /// </param>
        /// <param name="ExportUserDefinedParameterMapping">
        /// Select this option to export a custom parameter-mapping table. If selected, you can specify the name of a text file that contains parameter mapping.
        /// </param>
        /// <param name="ExportUserDefinedParameterMappingPath">
        /// The file path of the user-defined parameter mapping to be used in the export.
        /// </param>
        /// <param name="ExportUserDefinedPsets">
        /// Select this option to export user-defined property sets.
        /// </param>
        /// <param name="ExportUserDefinedPsetsFilePath">
        /// The file path of the user-defined parameter mapping to be used in the export.
        /// </param>
        /// <param name="IFCFileType">Specifies the type of IFC file to export 
        /// 0 = IFC
        /// 1 = IFC XML
        /// 2 = Zipped IFC
        /// 3 = Zipped IFC XML
        /// </param>
        /// <param name="IFCVersion">Specifies the IFC version format to use for export: 
        /// 0 = Default
        /// 8 = IFC 2x2 Singapore BCA e-Plan Check, IFC 2x2 used for submitting files to the Singapore BCA e-Plan Check Server.When exporting to this file type, ensure that all room-bounding elements are selected.<br/>
        /// 9 = IFC 2x2 Coordination View, The older IFC 2x2 schema, using the Coordination View model definition.<br/>
        /// 10 = IFC 2x3 Coordination View, The older certified version of export, used by older product versions, based on the IFC 2x3 schema and the Coordination View model view definition.<br/>
        /// 17 = IFC 2x3 GSA Concept Design BIM 2010, Standard IFC 2x3 used for submitting files to the US Government Services Administration. Additional property sets will be included.<br/>
        /// 21 = IFC 2x3 Coordination View 2.0, The default certified version of export, and the latest version generally supported by other systems, based on the IFC 2x3 schema and the Coordination View 2.0 model view definition.<br/>
        /// 24 = IFC2x3 Extended FM Handover View<br/>
        /// 25 = IFC4 Reference View, The newest version of IFC defined by buildingSMART.Intended as a reference model which is not modified.See https://technical.buildingsmart.org/.<br/>
        /// 26 = IFC4 Design Transfer View, The newest version of IFC defined by buildingSMART. With this option, Revit will attempt to create ifcAdvancedBReps in some cases. This can reduce file size and time to export.See https://technical.buildingsmart.org/.<br/>
        /// 27 = IFC2x3 Basic FM Handover View, IFC 2x3 that follows the general requirements to enable the handover of facility management information, used primarily in Bavaria.
        /// </param>
        /// <param name="IncludeSiteElevation">
        /// Select this option to include the elevation from the Z offset of the IFCSITE local placement. Clear the option to exclude it.
        /// </param>
        /// <param name="IncludeSteelElements">Indicates whether to include steel elements in the export.
        /// </param>
        /// <param name="SitePlacement">Specifies the site placement strategy (e.g., origin, site, etc.).
        /// </param>
        /// <param name="SpaceBoundaries">Specifies the level of space boundaries to export (0, 1, or 2).
        /// </param>
        /// <param name="SplitWallsAndColumns">
        /// Allows you to divide multi-level walls, columns, and ducts by level. When you use this option, Revit cuts the walls, columns, and ducts by each level that is defined as a building story (see Level Instance Properties).<br/>
        /// Revit exports elements whose base level is a non-building story level using the next lower building story as the base level, with an appropriate offset.<br/>
        /// Revit only exports levels for which the Building Story parameter is enabled, unless no levels are defined as building stories. In that case, Revit exports all levels that are used as base levels for walls, columns, and ducts.
        /// </param>
        /// <param name="StoreIFCGUID">
        /// Select this option to store the generated IFC GUIDs in the project file after export. This will add "IFC GUID" parameters to elements and their types, and Project Information for Project, Site, and Building GUIDs.
        /// </param>
        /// <param name="TessellationLevelOfDetail">Specifies the tessellation level of detail for geometry export 0-1.
        /// </param>
        /// <param name="Use2DRoomBoundaryForVolume">Indicates whether to use 2D room boundaries for volume calculations.
        /// </param>
        /// <param name="UseActiveViewGeometry">
        /// Select this option to use the active view to generate the exported geometry. Note that this can have unexpected results if used on a non-3D view.
        /// </param>
        /// <param name="UseFamilyAndTypeNameForReference">
        /// Select this option to use the family and type name for references. Clear the option to use the type name only.
        /// </param>
        /// <param name="UseOnlyTriangulation">Indicates whether to use only triangulation for export.
        /// </param>
        /// <param name="UseTypeNameOnlyForIfcType">Indicates whether to use type name only for IFC type object.
        /// </param>
        /// <param name="UseVisibleRevitNameAsEntityName">Indicates whether to use the visible Revit name as the IFC entity name.
        /// </param>
        /// <param name="VisibleElementsOfCurrentView">
        /// Select this option to export only the visible elements of the current view. Visible elements include those that are hidden by hidden line or shaded mode, any underlays in the view, and elements that are cropped from view by the crop region. Elements temporarily hidden using temporary hide/isolate are not exported.Categories marked as Not Exported in the IFC Export Classes dialog are not exported. See Load and Modify an IFC Mapping File. Clear the option to export the entire model.
        /// </param>
        /// <returns>Returns configured IFC export options.</returns>
        public static IFCExportOptions CreateIFCExportOptions(
            int ExchangeRequirement = 3,
            bool Export2DElements = false,
            bool ExportBaseQuantities = false,
            bool ExportBoundingBox = false,
            bool ExportIFCCommonPropertySets = false,
            bool ExportInternalRevitPropertySets = false,
            bool ExportLinkedFiles = false,
            bool ExportPartsAsBuildingElements = false,
            bool ExportRoomsInView = false,
            bool ExportSchedulesAsPsets = false,
            bool ExportSolidModelRep = false,
            bool ExportSpecificSchedules = false,
            bool ExportUserDefinedParameterMapping = false,
            string ExportUserDefinedParameterMappingPath = "",
            bool ExportUserDefinedPsets = false,
            string ExportUserDefinedPsetsFilePath = "",
            bool IncludeSiteElevation = false,
            bool IncludeSteelElements = false,
            int IFCFileType = 0,
            int IFCVersion = 21,
            Phase Phase = null,
            int SitePlacement = 0,
            int SpaceBoundaries = 0,
            bool SplitWallsAndColumns = false,
            bool StoreIFCGUID = false,
            double TessellationLevelOfDetail = 0.5,
            bool Use2DRoomBoundaryForVolume = false,
            bool UseActiveViewGeometry = false,
            bool UseFamilyAndTypeNameForReference = false,
            bool UseOnlyTriangulation = false,
            bool UseTypeNameOnlyForIfcType = false,
            bool UseVisibleRevitNameAsEntityName = false,
            bool VisibleElementsOfCurrentView = false
            )
        {
            IFCExportOptions options = new IFCExportOptions();
            try
            {
                Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
                // Basic file settings
                options.FileVersion = (IFCVersion)IFCVersion; // or IFCVersion.Default, IFC2x3, IFC4DTV, etc.
                options.SpaceBoundaryLevel = SpaceBoundaries; // 0, 1, or 2, where 2 is the most detailed
                options.WallAndColumnSplitting = SplitWallsAndColumns;
                options.ExportBaseQuantities = ExportBaseQuantities;
                // Booleans
                options.AddOption("SplitWallsAndColumns", SplitWallsAndColumns.ToString());
                options.AddOption("IncludeSteelElements", IncludeSteelElements.ToString());
                options.AddOption("Export2DElements", Export2DElements.ToString());
                options.AddOption("ExportLinkedFiles", ExportLinkedFiles.ToString());
                options.AddOption("VisibleElementsOfCurrentView", VisibleElementsOfCurrentView.ToString());

                options.AddOption("ExportRoomsInView", ExportRoomsInView.ToString());
                options.AddOption("ExportInternalRevitPropertySets", ExportInternalRevitPropertySets.ToString());
                options.AddOption("ExportIFCCommonPropertySets", ExportIFCCommonPropertySets.ToString());
                options.AddOption("ExportBaseQuantities", ExportBaseQuantities.ToString());
                options.AddOption("ExportSchedulesAsPsets", ExportSchedulesAsPsets.ToString());

                options.AddOption("ExportSpecificSchedules", ExportSpecificSchedules.ToString());
                options.AddOption("ExportUserDefinedPsets", ExportUserDefinedPsets.ToString());
                options.AddOption("ExportUserDefinedParameterMapping", ExportUserDefinedParameterMapping.ToString());
                options.AddOption("ExportPartsAsBuildingElements", ExportPartsAsBuildingElements.ToString());
                options.AddOption("ExportSolidModelRep", ExportSolidModelRep.ToString());

                options.AddOption("UseActiveViewGeometry", UseActiveViewGeometry.ToString());
                options.AddOption("UseFamilyAndTypeNameForReference", UseFamilyAndTypeNameForReference.ToString());
                options.AddOption("Use2DRoomBoundaryForVolume", Use2DRoomBoundaryForVolume.ToString());
                options.AddOption("IncludeSiteElevation", IncludeSiteElevation.ToString());
                options.AddOption("StoreIFCGUID", StoreIFCGUID.ToString());

                options.AddOption("ExportBoundingBox", ExportBoundingBox.ToString());
                options.AddOption("UseOnlyTriangulation", UseOnlyTriangulation.ToString());
                options.AddOption("UseTypeNameOnlyForIfcType", UseTypeNameOnlyForIfcType.ToString());
                options.AddOption("UseVisibleRevitNameAsEntityName", UseVisibleRevitNameAsEntityName.ToString());
                options.AddOption("IsBuiltIn", "false");
                options.AddOption("IsInSession", "false");

                // Strings
                options.AddOption("ExportUserDefinedPsetsFileName", ExportUserDefinedPsetsFilePath);
                options.AddOption("ExportUserDefinedParameterMappingFileName", ExportUserDefinedParameterMappingPath);

                // Integers
                options.AddOption("ExchangeRequirement", ExchangeRequirement.ToString());
                options.AddOption("IFCFileType", IFCFileType.ToString());
                options.AddOption("ActivePhaseId", Phase.Id.ToString());
                options.AddOption("SpaceBoundaries", SpaceBoundaries.ToString());
                options.AddOption("SitePlacement", SitePlacement.ToString());
                options.AddOption("TessellationLevelOfDetail", TessellationLevelOfDetail.ToString());

            }
            catch (Exception ex)
            {
                // Handle the exception (logging, etc.). You might want to communicate this to the user.
                Console.WriteLine($"An error occurred while setting IFC export options: {ex.Message}");
            }

            return options;
        }
    }
}
