using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Visio;
using ShapeSpecs.Core.Services;

namespace ShapeSpecs.UI.Ribbon
{
    /// <summary>
    /// Ribbon for the ShapeSpecs add-in
    /// </summary>
    [ComVisible(true)]
    public class ShapeSpecsRibbon : Office.IRibbonExtensibility
    {
        private Microsoft.Office.Tools.Ribbon.RibbonUI ribbon;
        private readonly ShapeService _shapeService;
        private readonly FileService _fileService;
        private readonly Forms.SpecsPanel _specPanel;

        /// <summary>
        /// Initializes a new instance of the ShapeSpecsRibbon class
        /// </summary>
        /// <param name="shapeService">Service for shape operations</param>
        /// <param name="fileService">Service for file operations</param>
        /// <param name="specPanel">Panel for displaying specifications</param>
        public ShapeSpecsRibbon(ShapeService shapeService, FileService fileService, Forms.SpecsPanel specPanel)
        {
            _shapeService = shapeService ?? throw new ArgumentNullException(nameof(shapeService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _specPanel = specPanel ?? throw new ArgumentNullException(nameof(specPanel));
        }

        #region IRibbonExtensibility Members

        /// <summary>
        /// Gets the XML that defines the ribbon UI
        /// </summary>
        /// <param name="ribbonID">The ID of the ribbon</param>
        /// <returns>The XML for the ribbon</returns>
        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("ShapeSpecs.UI.Ribbon.ShapeSpecsRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks

        /// <summary>
        /// Called when the ribbon is loaded
        /// </summary>
        /// <param name="ribbonUI">The ribbon UI</param>
        public void OnLoad(Microsoft.Office.Tools.Ribbon.RibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        /// <summary>
        /// Called when the "Show Panel" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnShowPanelClick(Office.IRibbonControl control)
        {
            // Show the dockable pane
            // In a real implementation, this would show the dockable pane
            // For now, we'll just show a message
            System.Windows.Forms.MessageBox.Show("The panel functionality will be implemented in Phase 1.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called when the "Add Spec" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnAddSpecClick(Office.IRibbonControl control)
        {
            // Check if there's a selected shape
            var application = Globals.ThisAddIn.Application;
            if (application.ActiveWindow.Selection.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Please select a shape first.", 
                    "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            var shape = application.ActiveWindow.Selection[1];
            var metadata = _shapeService.GetShapeMetadata(shape);

            // Show editor form
            var editor = Forms.EditorForm.ForSpecification();
            if (editor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Add the new specification
                metadata.TextSpecifications[editor.EditedName] = editor.EditedText;
                
                // Save the metadata
                _shapeService.SaveShapeMetadata(shape, metadata);
                
                // Update the panel
                _specPanel.UpdateForShape(shape);
            }
        }

        /// <summary>
        /// Called when the "Add Attachment" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnAddAttachmentClick(Office.IRibbonControl control)
        {
            // This functionality will be implemented in Phase 2
            System.Windows.Forms.MessageBox.Show("Add Attachment functionality will be implemented in Phase 2.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called when the "Add Note" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnAddNoteClick(Office.IRibbonControl control)
        {
            // This functionality will be implemented in Phase 3
            System.Windows.Forms.MessageBox.Show("Add Note functionality will be implemented in Phase 3.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called when the "Import" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnImportClick(Office.IRibbonControl control)
        {
            // This functionality will be implemented in Phase 2
            System.Windows.Forms.MessageBox.Show("Import functionality will be implemented in Phase 2.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called when the "Export" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnExportClick(Office.IRibbonControl control)
        {
            // This functionality will be implemented in Phase 2
            System.Windows.Forms.MessageBox.Show("Export functionality will be implemented in Phase 2.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called when the "Settings" button is clicked
        /// </summary>
        /// <param name="control">The ribbon control that was clicked</param>
        public void OnSettingsClick(Office.IRibbonControl control)
        {
            // This functionality will be implemented in Phase 3
            System.Windows.Forms.MessageBox.Show("Settings functionality will be implemented in Phase 3.", 
                "ShapeSpecs", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the text from an embedded resource
        /// </summary>
        /// <param name="resourceName">The name of the resource</param>
        /// <returns>The resource text</returns>
        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}