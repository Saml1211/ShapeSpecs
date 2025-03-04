using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Office.Interop.Visio;
using Microsoft.Office.Tools;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Services;
using ShapeSpecs.Core.Utilities;
using ShapeSpecs.UI.Forms;
using ShapeSpecs.UI.Ribbon;

namespace ShapeSpecs.Addin
{
    public partial class ThisAddIn
    {
        // Services
        private ShapeService _shapeService;
        private StorageService _storageService;
        private FileService _fileService;
        private JsonHelper _jsonHelper;
        private FileHelper _fileHelper;

        // UI Components
        private SpecsPanel _specsPanel;
        private CustomTaskPane _specsPaneHost;
        private ShapeSpecsRibbon _ribbon;

        // Event handlers
        private Microsoft.Office.Interop.Visio.Application _visioApplication;

        /// <summary>
        /// Initialization code. Called when the add-in is loaded.
        /// </summary>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try
            {
                // Store reference to Visio application
                _visioApplication = this.Application;

                // Initialize services and utilities
                InitializeServices();

                // Initialize UI components
                InitializeUI();

                // Hook up Visio events
                HookEvents();

                // Log startup success
                LogInfo("ShapeSpecs add-in started successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing ShapeSpecs add-in: {ex.Message}\n\n{ex.StackTrace}",
                    "ShapeSpecs Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Startup error: {ex}");
            }
        }

        /// <summary>
        /// Clean up code. Called when the add-in is unloaded.
        /// </summary>
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            try
            {
                // Unhook Visio events
                UnhookEvents();

                // Dispose any resources
                if (_specsPaneHost != null)
                {
                    _specsPaneHost.Dispose();
                    _specsPaneHost = null;
                }

                LogInfo("ShapeSpecs add-in shut down successfully");
            }
            catch (Exception ex)
            {
                LogError($"Shutdown error: {ex}");
            }
        }

        /// <summary>
        /// Initialize the core services and utilities
        /// </summary>
        private void InitializeServices()
        {
            // Get the storage path
            string storagePath = GetStoragePath();
            Directory.CreateDirectory(storagePath);

            // Create utilities
            _jsonHelper = new JsonHelper();
            _fileHelper = new FileHelper();

            // Create services
            _storageService = new StorageService(storagePath, _jsonHelper, _fileHelper);
            _shapeService = new ShapeService(_storageService);
            _fileService = new FileService(_fileHelper, _storageService);
        }

        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeUI()
        {
            // Create specs panel
            _specsPanel = new SpecsPanel();
            _specsPanel.Initialize(_shapeService, _fileService);

            // Create custom task pane
            _specsPaneHost = this.CustomTaskPanes.Add(_specsPanel, "ShapeSpecs");
            _specsPaneHost.DockPosition = Microsoft.Office.Core.MsoCTPDockPosition.msoCTPDockPositionRight;
            _specsPaneHost.Width = 400;
            _specsPaneHost.Visible = true;

            // Create ribbon controller
            _ribbon = new ShapeSpecsRibbon(_shapeService, _fileService, _specsPanel);
        }

        /// <summary>
        /// Hook up Visio events
        /// </summary>
        private void HookEvents()
        {
            try
            {
                if (_visioApplication != null)
                {
                    // Hook up selection event
                    _visioApplication.SelectionChanged += VisioApplication_SelectionChanged;

                    // Hook up document events
                    _visioApplication.DocumentCreated += VisioApplication_DocumentChanged;
                    _visioApplication.DocumentOpened += VisioApplication_DocumentChanged;
                    _visioApplication.DocumentSaved += VisioApplication_DocumentChanged;

                    // Initial update for current selection (if any)
                    UpdateSelectionInfo();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error hooking events: {ex}");
            }
        }

        /// <summary>
        /// Unhook Visio events
        /// </summary>
        private void UnhookEvents()
        {
            try
            {
                if (_visioApplication != null)
                {
                    // Unhook events
                    _visioApplication.SelectionChanged -= VisioApplication_SelectionChanged;
                    _visioApplication.DocumentCreated -= VisioApplication_DocumentChanged;
                    _visioApplication.DocumentOpened -= VisioApplication_DocumentChanged;
                    _visioApplication.DocumentSaved -= VisioApplication_DocumentChanged;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error unhooking events: {ex}");
            }
        }

        /// <summary>
        /// Handle selection changed event
        /// </summary>
        private void VisioApplication_SelectionChanged(Window window)
        {
            UpdateSelectionInfo();
        }

        /// <summary>
        /// Handle document events
        /// </summary>
        private void VisioApplication_DocumentChanged(Document document)
        {
            // Reset and update for the current document
            UpdateSelectionInfo();
        }

        /// <summary>
        /// Update the specs panel with the current selection
        /// </summary>
        private void UpdateSelectionInfo()
        {
            try
            {
                // Make sure we have a valid selection
                if (_visioApplication?.ActiveWindow?.Selection == null || _visioApplication.ActiveWindow.Selection.Count == 0)
                {
                    // No selection
                    _specsPanel.UpdateForShape(null);
                    return;
                }

                // Get the first selected shape
                Shape shape = _visioApplication.ActiveWindow.Selection[1];
                
                // Update the panel
                _specsPanel.UpdateForShape(shape);
            }
            catch (Exception ex)
            {
                LogError($"Error updating selection info: {ex}");
            }
        }

        /// <summary>
        /// Get the base storage path for shape metadata and attachments
        /// </summary>
        private string GetStoragePath()
        {
            try
            {
                // Get the add-in's location
                string addInPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Use a subdirectory for storage
                return Path.Combine(addInPath, "ShapeSpecsData");
            }
            catch
            {
                // Fallback to user's AppData directory
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ShapeSpecs");
            }
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        private void LogInfo(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        private void LogError(string message)
        {
            Log("ERROR", message);
        }

        /// <summary>
        /// Write a log entry
        /// </summary>
        private void Log(string level, string message)
        {
            try
            {
                string logFolder = Path.Combine(GetStoragePath(), "Logs");
                Directory.CreateDirectory(logFolder);
                
                string logFile = Path.Combine(logFolder, $"ShapeSpecs_{DateTime.Now:yyyyMMdd}.log");
                
                using (StreamWriter writer = File.AppendText(logFile))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}");
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        /// <summary>
        /// Create the ribbon extension
        /// </summary>
        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return _ribbon ?? (_ribbon = new ShapeSpecsRibbon(_shapeService, _fileService, _specsPanel));
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}