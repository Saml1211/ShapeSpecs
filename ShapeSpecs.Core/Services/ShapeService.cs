using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Visio;
using ShapeSpecs.Core.Models;

namespace ShapeSpecs.Core.Services
{
    /// <summary>
    /// Service for interacting with Visio shapes and managing their associated metadata
    /// </summary>
    public class ShapeService
    {
        private const string MetadataPropertyName = "ShapeSpecs_Metadata";
        private readonly StorageService _storageService;

        public ShapeService(StorageService storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        /// <summary>
        /// Gets the metadata for a specific shape
        /// </summary>
        /// <param name="shape">The Visio shape to retrieve metadata for</param>
        /// <returns>The shape's metadata or a new metadata object if none exists</returns>
        public ShapeMetadata GetShapeMetadata(Shape shape)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));

            // Generate a consistent ID for the shape
            string shapeId = GetShapeId(shape);

            // Check if the shape has existing metadata
            if (ShapeHasMetadata(shape))
            {
                // Retrieve and deserialize the metadata reference from the shape's custom properties
                string metadataReference = GetCustomProperty(shape, MetadataPropertyName);
                return _storageService.LoadShapeMetadata(shapeId, metadataReference);
            }

            // If no metadata exists, create a new metadata object
            var metadata = new ShapeMetadata
            {
                ShapeId = shapeId,
                DeviceType = TryGetShapeType(shape),
                Model = shape.Name
            };

            // Save the new metadata
            SaveShapeMetadata(shape, metadata);

            return metadata;
        }

        /// <summary>
        /// Saves metadata for a specific shape
        /// </summary>
        /// <param name="shape">The Visio shape to save metadata for</param>
        /// <param name="metadata">The metadata to save</param>
        public void SaveShapeMetadata(Shape shape, ShapeMetadata metadata)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            // Ensure the metadata has the correct ShapeId
            metadata.ShapeId = GetShapeId(shape);
            metadata.LastModified = DateTime.Now;
            
            // Save the metadata using the storage service
            string metadataReference = _storageService.SaveShapeMetadata(metadata);
            
            // Store the reference in the shape's custom properties
            SetCustomProperty(shape, MetadataPropertyName, metadataReference);
        }

        /// <summary>
        /// Generates a consistent ID for a shape
        /// </summary>
        /// <param name="shape">The Visio shape</param>
        /// <returns>A unique identifier for the shape</returns>
        /// <remarks>
        /// LIMITATION: The shape ID is based on the document name and shape ID.
        /// If the document is renamed, the shape ID will change and the association
        /// with existing metadata will be lost. This is a known limitation of Phase 1.
        /// Future enhancement: Consider persisting a GUID in the shape's custom properties
        /// to maintain the association across document renames.
        /// </remarks>
        private string GetShapeId(Shape shape)
        {
            // Use the shape's unique ID (or another suitable property) to generate a consistent ID
            // NOTE: This will change if the document is renamed
            return $"{shape.Document.Name}_{shape.ID}";
        }

        /// <summary>
        /// Attempts to determine the type of device this shape represents
        /// </summary>
        /// <param name="shape">The Visio shape</param>
        /// <returns>A string representing the device type, or an empty string if unknown</returns>
        private string TryGetShapeType(Shape shape)
        {
            // This is a placeholder - in a real implementation, we would
            // analyze the shape's master, text, or other properties to
            // determine what type of AV device it represents
            
            if (shape.Master != null)
                return shape.Master.Name;

            return string.Empty;
        }

        /// <summary>
        /// Checks if a shape has associated metadata
        /// </summary>
        /// <param name="shape">The Visio shape to check</param>
        /// <returns>True if the shape has metadata, false otherwise</returns>
        private bool ShapeHasMetadata(Shape shape)
        {
            return !string.IsNullOrEmpty(GetCustomProperty(shape, MetadataPropertyName));
        }

        /// <summary>
        /// Gets a custom property value from a shape
        /// </summary>
        /// <param name="shape">The Visio shape</param>
        /// <param name="propertyName">The name of the custom property</param>
        /// <returns>The property value or an empty string if the property doesn't exist</returns>
        private string GetCustomProperty(Shape shape, string propertyName)
        {
            try
            {
                short propIndex = shape.CellExists[$"Prop.{propertyName}", 0];
                if (propIndex != 0)
                    return shape.Cells[$"Prop.{propertyName}.Value"].ResultStr[""];
            }
            catch (Exception)
            {
                // If any error occurs, return an empty string
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Sets a custom property value on a shape
        /// </summary>
        /// <param name="shape">The Visio shape</param>
        /// <param name="propertyName">The name of the custom property</param>
        /// <param name="value">The value to set</param>
        private void SetCustomProperty(Shape shape, string propertyName, string value)
        {
            try
            {
                shape.AddCustomProperty(propertyName, value);
            }
            catch
            {
                // If the property already exists, update it instead
                shape.Cells[$"Prop.{propertyName}.Value"].Formula = $"\"{value}\"";
            }
        }
    }
}