using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Services;

namespace ShapeSpecs.UI.Forms
{
    /// <summary>
    /// The main panel for displaying and editing shape specifications
    /// </summary>
    public partial class SpecsPanel : UserControl
    {
        private ShapeService _shapeService;
        private FileService _fileService;
        private ShapeMetadata _currentMetadata;

        /// <summary>
        /// Initializes a new instance of the SpecsPanel class
        /// </summary>
        public SpecsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the panel with the required services
        /// </summary>
        /// <param name="shapeService">Service for shape operations</param>
        /// <param name="fileService">Service for file operations</param>
        public void Initialize(ShapeService shapeService, FileService fileService)
        {
            _shapeService = shapeService ?? throw new ArgumentNullException(nameof(shapeService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <summary>
        /// Updates the panel with the specifications for a shape
        /// </summary>
        /// <param name="shape">The Visio shape</param>
        public void UpdateForShape(Microsoft.Office.Interop.Visio.Shape shape)
        {
            if (shape == null)
            {
                ClearPanel();
                return;
            }

            try
            {
                // Get the metadata for the shape
                _currentMetadata = _shapeService.GetShapeMetadata(shape);
                
                // Update the UI with the metadata
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shape specifications: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearPanel();
            }
        }

        /// <summary>
        /// Clears the panel when no shape is selected
        /// </summary>
        private void ClearPanel()
        {
            _currentMetadata = null;
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI with the current metadata
        /// </summary>
        private void UpdateUI()
        {
            // This is a placeholder implementation
            // The actual implementation will update all UI elements with the metadata
            
            if (_currentMetadata == null)
            {
                // No shape selected, disable UI elements
                lblNoShape.Visible = true;
                tabControl.Visible = false;
                return;
            }

            // Shape selected, update and enable UI elements
            lblNoShape.Visible = false;
            tabControl.Visible = true;

            // Update shape info
            txtShapeName.Text = _currentMetadata.Model;
            txtDeviceType.Text = _currentMetadata.DeviceType;

            // Update text specifications
            UpdateTextSpecifications();

            // Update attachments
            UpdateAttachments();

            // Update notes
            UpdateNotes();
        }

        /// <summary>
        /// Updates the text specifications tab with the current metadata
        /// </summary>
        private void UpdateTextSpecifications()
        {
            // Clear the existing specifications
            flowLayoutSpecs.Controls.Clear();

            // Add each specification
            foreach (var spec in _currentMetadata.TextSpecifications)
            {
                // Create a panel for this specification
                var panel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 100,
                    Padding = new Padding(5),
                    Margin = new Padding(0, 0, 0, 10),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Add a label for the specification name
                var lblName = new Label
                {
                    Text = spec.Key,
                    Dock = DockStyle.Top,
                    Font = new Font(Font, FontStyle.Bold),
                    Height = 20
                };
                panel.Controls.Add(lblName);

                // Add a textbox for the specification value
                var txtValue = new TextBox
                {
                    Text = spec.Value,
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical
                };
                panel.Controls.Add(txtValue);

                // Add the panel to the flow layout
                flowLayoutSpecs.Controls.Add(panel);
            }

            // Add an "Add Specification" button
            var btnAddSpec = new Button
            {
                Text = "Add Specification",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnAddSpec.Click += BtnAddSpec_Click;
            flowLayoutSpecs.Controls.Add(btnAddSpec);
        }

        /// <summary>
        /// Updates the attachments tab with the current metadata
        /// </summary>
        private void UpdateAttachments()
        {
            // Clear the existing attachments
            flowLayoutAttachments.Controls.Clear();

            // Add each attachment
            foreach (var attachment in _currentMetadata.Attachments)
            {
                // Create a panel for this attachment
                var panel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 80,
                    Padding = new Padding(5),
                    Margin = new Padding(0, 0, 0, 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = attachment
                };

                // Add a label for the attachment name
                var lblName = new Label
                {
                    Text = attachment.Name,
                    Dock = DockStyle.Top,
                    Font = new Font(Font, FontStyle.Bold),
                    Height = 20
                };
                panel.Controls.Add(lblName);

                // Add a label for the attachment info
                var lblInfo = new Label
                {
                    Text = $"Type: {attachment.Type}, Size: {FormatFileSize(attachment.Size)}",
                    Dock = DockStyle.Top,
                    Height = 20
                };
                panel.Controls.Add(lblInfo);

                // Add a panel for the attachment buttons
                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Height = 30
                };

                // Add buttons for viewing and deleting the attachment
                var btnView = new Button
                {
                    Text = "View",
                    Width = 80,
                    Height = 25,
                    Location = new Point(0, 2),
                    Tag = attachment
                };
                btnView.Click += BtnViewAttachment_Click;
                buttonPanel.Controls.Add(btnView);

                var btnDelete = new Button
                {
                    Text = "Delete",
                    Width = 80,
                    Height = 25,
                    Location = new Point(90, 2),
                    Tag = attachment
                };
                btnDelete.Click += BtnDeleteAttachment_Click;
                buttonPanel.Controls.Add(btnDelete);

                panel.Controls.Add(buttonPanel);

                // Add the panel to the flow layout
                flowLayoutAttachments.Controls.Add(panel);
            }

            // Add an "Add Attachment" button
            var btnAddAttachment = new Button
            {
                Text = "Add Attachment",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnAddAttachment.Click += BtnAddAttachment_Click;
            flowLayoutAttachments.Controls.Add(btnAddAttachment);
        }

        /// <summary>
        /// Updates the notes tab with the current metadata
        /// </summary>
        private void UpdateNotes()
        {
            // Clear the existing notes
            flowLayoutNotes.Controls.Clear();

            // Add each note
            foreach (var note in _currentMetadata.Notes)
            {
                // Create a panel for this note
                var panel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 120,
                    Padding = new Padding(5),
                    Margin = new Padding(0, 0, 0, 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = note
                };

                // Add a label for the note info
                var lblInfo = new Label
                {
                    Text = $"By {note.Author} on {note.DateAdded.ToShortDateString()} at {note.DateAdded.ToShortTimeString()}",
                    Dock = DockStyle.Top,
                    Height = 20
                };
                panel.Controls.Add(lblInfo);

                // Add a textbox for the note text
                var txtNote = new TextBox
                {
                    Text = note.Text,
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical
                };
                panel.Controls.Add(txtNote);

                // Add a panel for the note buttons
                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 30
                };

                // Add buttons for editing and deleting the note
                var btnEdit = new Button
                {
                    Text = "Edit",
                    Width = 80,
                    Height = 25,
                    Location = new Point(0, 2),
                    Tag = note
                };
                btnEdit.Click += BtnEditNote_Click;
                buttonPanel.Controls.Add(btnEdit);

                var btnDelete = new Button
                {
                    Text = "Delete",
                    Width = 80,
                    Height = 25,
                    Location = new Point(90, 2),
                    Tag = note
                };
                btnDelete.Click += BtnDeleteNote_Click;
                buttonPanel.Controls.Add(btnDelete);

                panel.Controls.Add(buttonPanel);

                // Add the panel to the flow layout
                flowLayoutNotes.Controls.Add(panel);
            }

            // Add an "Add Note" button
            var btnAddNote = new Button
            {
                Text = "Add Note",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnAddNote.Click += BtnAddNote_Click;
            flowLayoutNotes.Controls.Add(btnAddNote);
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable string
        /// </summary>
        /// <param name="bytes">The size in bytes</param>
        /// <returns>A formatted string</returns>
        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {suffixes[suffixIndex]}";
        }

        #region Event Handlers

        private void BtnAddSpec_Click(object sender, EventArgs e)
        {
            // Placeholder for adding a specification
            MessageBox.Show("Add Specification functionality will be implemented in Phase 1.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddAttachment_Click(object sender, EventArgs e)
        {
            // Placeholder for adding an attachment
            MessageBox.Show("Add Attachment functionality will be implemented in Phase 2.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnViewAttachment_Click(object sender, EventArgs e)
        {
            // Placeholder for viewing an attachment
            MessageBox.Show("View Attachment functionality will be implemented in Phase 2.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteAttachment_Click(object sender, EventArgs e)
        {
            // Placeholder for deleting an attachment
            MessageBox.Show("Delete Attachment functionality will be implemented in Phase 2.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddNote_Click(object sender, EventArgs e)
        {
            // Placeholder for adding a note
            MessageBox.Show("Add Note functionality will be implemented in Phase 3.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEditNote_Click(object sender, EventArgs e)
        {
            // Placeholder for editing a note
            MessageBox.Show("Edit Note functionality will be implemented in Phase 3.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteNote_Click(object sender, EventArgs e)
        {
            // Placeholder for deleting a note
            MessageBox.Show("Delete Note functionality will be implemented in Phase 3.", "Not Implemented",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Designer Generated Code

        // TODO: Add the InitializeComponent method and fields
        // This would normally be generated by the Windows Forms Designer
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabInfo;
        private System.Windows.Forms.TabPage tabSpecs;
        private System.Windows.Forms.TabPage tabAttachments;
        private System.Windows.Forms.TabPage tabNotes;
        private System.Windows.Forms.Label lblNoShape;
        private System.Windows.Forms.TextBox txtShapeName;
        private System.Windows.Forms.TextBox txtDeviceType;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutSpecs;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutAttachments;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutNotes;

        /// <summary>
        /// Initialize the UI components - simplified for this example
        /// In a real project, this would be generated by the Windows Forms Designer
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabInfo = new System.Windows.Forms.TabPage();
            this.tabSpecs = new System.Windows.Forms.TabPage();
            this.tabAttachments = new System.Windows.Forms.TabPage();
            this.tabNotes = new System.Windows.Forms.TabPage();
            this.lblNoShape = new System.Windows.Forms.Label();
            this.txtShapeName = new System.Windows.Forms.TextBox();
            this.txtDeviceType = new System.Windows.Forms.TextBox();
            this.flowLayoutSpecs = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutAttachments = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutNotes = new System.Windows.Forms.FlowLayoutPanel();
            
            // TabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(400, 600);
            
            // TabPages
            this.tabControl.Controls.Add(this.tabInfo);
            this.tabControl.Controls.Add(this.tabSpecs);
            this.tabControl.Controls.Add(this.tabAttachments);
            this.tabControl.Controls.Add(this.tabNotes);
            
            this.tabInfo.Text = "Info";
            this.tabSpecs.Text = "Specifications";
            this.tabAttachments.Text = "Attachments";
            this.tabNotes.Text = "Notes";
            
            // Label for no shape selected
            this.lblNoShape.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoShape.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoShape.Location = new System.Drawing.Point(0, 0);
            this.lblNoShape.Name = "lblNoShape";
            this.lblNoShape.Size = new System.Drawing.Size(400, 600);
            this.lblNoShape.TabIndex = 0;
            this.lblNoShape.Text = "No shape selected";
            this.lblNoShape.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // Setup Info tab
            this.tabInfo.Controls.Add(this.txtDeviceType);
            this.tabInfo.Controls.Add(this.txtShapeName);
            
            this.txtShapeName.Location = new System.Drawing.Point(10, 30);
            this.txtShapeName.Name = "txtShapeName";
            this.txtShapeName.Size = new System.Drawing.Size(350, 20);
            this.txtShapeName.ReadOnly = true;
            
            this.txtDeviceType.Location = new System.Drawing.Point(10, 80);
            this.txtDeviceType.Name = "txtDeviceType";
            this.txtDeviceType.Size = new System.Drawing.Size(350, 20);
            this.txtDeviceType.ReadOnly = true;
            
            // Setup Specifications tab
            this.tabSpecs.Controls.Add(this.flowLayoutSpecs);
            this.flowLayoutSpecs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutSpecs.AutoScroll = true;
            this.flowLayoutSpecs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutSpecs.WrapContents = false;
            
            // Setup Attachments tab
            this.tabAttachments.Controls.Add(this.flowLayoutAttachments);
            this.flowLayoutAttachments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutAttachments.AutoScroll = true;
            this.flowLayoutAttachments.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutAttachments.WrapContents = false;
            
            // Setup Notes tab
            this.tabNotes.Controls.Add(this.flowLayoutNotes);
            this.flowLayoutNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutNotes.AutoScroll = true;
            this.flowLayoutNotes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutNotes.WrapContents = false;
            
            // Add controls to the user control
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.lblNoShape);
            
            this.Name = "SpecsPanel";
            this.Size = new System.Drawing.Size(400, 600);
        }
        
        #endregion
    }
}