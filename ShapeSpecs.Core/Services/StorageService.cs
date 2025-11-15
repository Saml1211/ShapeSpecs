using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Services
{
    /// <summary>
    /// Service for storing and retrieving shape metadata and attachments
    /// </summary>
    /// <remarks>
    /// Storage Directory Structure:
    ///
    /// [BaseStoragePath]/
    /// ├── Logs/
    /// │   └── ShapeSpecs_YYYYMMDD.log
    /// └── shapes/
    ///     └── [ShapeId]/
    ///         ├── metadata.json
    ///         ├── images/
    ///         │   ├── [AttachmentId].jpg
    ///         │   └── [AttachmentId]_thumb.jpg
    ///         ├── pdfs/
    ///         │   └── [AttachmentId].pdf
    ///         ├── documents/
    ///         │   └── [AttachmentId].docx
    ///         └── others/
    ///             └── [AttachmentId].[ext]
    ///
    /// Where:
    /// - BaseStoragePath: Configured storage location (typically in add-in directory or AppData)
    /// - ShapeId: Format "{DocumentName}_{ShapeID}"
    /// - AttachmentId: GUID generated for each attachment
    /// - Attachments are organized by type (images, pdfs, documents, others)
    /// - Thumbnails are created for image attachments with "_thumb" suffix
    /// - All paths stored in metadata are relative to BaseStoragePath for portability
    /// </remarks>
    public class StorageService : IDisposable
    {
        private readonly string _baseStoragePath;
        private readonly JsonHelper _jsonHelper;
        private readonly FileHelper _fileHelper;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of the StorageService
        /// </summary>
        /// <param name="baseStoragePath">The base directory where data will be stored</param>
        /// <param name="jsonHelper">Helper for JSON operations</param>
        /// <param name="fileHelper">Helper for file operations</param>
        public StorageService(string baseStoragePath, JsonHelper jsonHelper, FileHelper fileHelper)
        {
            _baseStoragePath = baseStoragePath ?? throw new ArgumentNullException(nameof(baseStoragePath));
            _jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            
            // Ensure the storage directory exists
            Directory.CreateDirectory(_baseStoragePath);
        }

        /// <summary>
        /// Saves shape metadata and returns a reference to it
        /// </summary>
        /// <param name="metadata">The metadata to save</param>
        /// <returns>A reference string that can be used to load the metadata later</returns>
        public string SaveShapeMetadata(ShapeMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            // Create the shape's storage directory if it doesn't exist
            string shapeDirectory = GetShapeDirectory(metadata.ShapeId);
            Directory.CreateDirectory(shapeDirectory);

            // Save the metadata to a JSON file
            string metadataPath = Path.Combine(shapeDirectory, "metadata.json");
            _jsonHelper.SerializeToFile(metadata, metadataPath);

            // Return a reference to the metadata
            // The reference format is simply the relative path from the base storage
            return GetRelativePath(metadataPath);
        }

        /// <summary>
        /// Loads shape metadata from a reference
        /// </summary>
        /// <param name="shapeId">The ID of the shape</param>
        /// <param name="metadataReference">The reference to the metadata</param>
        /// <returns>The loaded shape metadata</returns>
        public ShapeMetadata LoadShapeMetadata(string shapeId, string metadataReference)
        {
            if (string.IsNullOrEmpty(shapeId))
                throw new ArgumentException("Shape ID cannot be null or empty", nameof(shapeId));
            
            if (string.IsNullOrEmpty(metadataReference))
                throw new ArgumentException("Metadata reference cannot be null or empty", nameof(metadataReference));

            try
            {
                // Get the absolute path to the metadata file
                string metadataPath = Path.Combine(_baseStoragePath, metadataReference);

                // Check if the file exists - this is expected for new shapes
                if (!File.Exists(metadataPath))
                {
                    // File doesn't exist - this is normal for a new shape, return new metadata
                    return new ShapeMetadata
                    {
                        ShapeId = shapeId
                    };
                }

                // Deserialize the metadata from the JSON file
                var metadata = _jsonHelper.DeserializeFromFile<ShapeMetadata>(metadataPath);

                // Verify the shape ID matches
                if (metadata.ShapeId != shapeId)
                {
                    throw new InvalidOperationException($"Shape ID mismatch. Expected: {shapeId}, Found: {metadata.ShapeId}");
                }

                return metadata;
            }
            catch (FileNotFoundException)
            {
                // File not found - return new metadata (should be caught by File.Exists above, but defensive)
                return new ShapeMetadata
                {
                    ShapeId = shapeId
                };
            }
            catch (DirectoryNotFoundException)
            {
                // Directory structure doesn't exist yet - return new metadata
                return new ShapeMetadata
                {
                    ShapeId = shapeId
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                // Permission error - this is serious, don't mask it
                throw new InvalidOperationException($"Access denied when loading metadata for shape {shapeId}: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // Corrupt JSON - return new metadata with error note so user can recover
                return new ShapeMetadata
                {
                    ShapeId = shapeId,
                    Notes = new List<Note>
                    {
                        new Note
                        {
                            Text = $"Warning: Previous metadata was corrupted and could not be loaded. Error: {ex.Message}",
                            Author = "System",
                            Category = "Error",
                            Priority = NotePriority.High
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // Unexpected error - log it and return new metadata with error note
                // In a real application, this would be logged to a logging system
                System.Diagnostics.Debug.WriteLine($"Unexpected error loading metadata for shape {shapeId}: {ex}");

                return new ShapeMetadata
                {
                    ShapeId = shapeId,
                    Notes = new List<Note>
                    {
                        new Note
                        {
                            Text = $"Error loading metadata: {ex.Message}. Previous data may be lost.",
                            Author = "System",
                            Category = "Error",
                            Priority = NotePriority.High
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Adds an attachment to a shape's metadata
        /// </summary>
        /// <param name="metadata">The shape metadata</param>
        /// <param name="sourceFilePath">The path to the file to attach</param>
        /// <param name="attachmentType">The type of attachment</param>
        /// <param name="name">Optional name for the attachment (defaults to filename)</param>
        /// <returns>The updated metadata with the attachment added</returns>
        public ShapeMetadata AddAttachment(ShapeMetadata metadata, string sourceFilePath, 
            AttachmentType attachmentType, string name = null)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            
            if (string.IsNullOrEmpty(sourceFilePath))
                throw new ArgumentException("Source file path cannot be null or empty", nameof(sourceFilePath));

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source file not found", sourceFilePath);

            // Create the attachments directory for this shape
            string attachmentsDirectory = GetAttachmentsDirectory(metadata.ShapeId, attachmentType);
            Directory.CreateDirectory(attachmentsDirectory);

            // Generate a unique filename for the attachment
            string attachmentId = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(sourceFilePath);
            string destinationFilename = $"{attachmentId}{extension}";
            string destinationPath = Path.Combine(attachmentsDirectory, destinationFilename);

            // Copy the file to our storage
            _fileHelper.CopyFile(sourceFilePath, destinationPath);

            // Create and add the attachment to the metadata
            var attachment = new Attachment
            {
                Id = attachmentId,
                Type = attachmentType,
                Name = name ?? Path.GetFileName(sourceFilePath),
                Path = GetRelativePath(destinationPath),
                Size = new FileInfo(destinationPath).Length,
                DateAdded = DateTime.Now,
                MimeType = _fileHelper.GetMimeType(destinationPath)
            };

            // If it's an image, create a thumbnail
            if (attachmentType == AttachmentType.Image)
            {
                string thumbnailPath = CreateThumbnail(destinationPath, attachmentId);
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    attachment.ThumbnailPath = GetRelativePath(thumbnailPath);
                }
            }

            // Add the attachment to the metadata and save
            metadata.Attachments.Add(attachment);
            SaveShapeMetadata(metadata);

            return metadata;
        }

        /// <summary>
        /// Creates a thumbnail for an image
        /// </summary>
        /// <param name="imagePath">Path to the source image</param>
        /// <param name="attachmentId">ID of the attachment</param>
        /// <returns>Path to the created thumbnail, or null if creation failed</returns>
        private string CreateThumbnail(string imagePath, string attachmentId)
        {
            try
            {
                string directory = Path.GetDirectoryName(imagePath);
                string thumbnailPath = Path.Combine(directory, $"{attachmentId}_thumb.jpg");
                
                // Create a thumbnail using the FileHelper
                _fileHelper.CreateThumbnail(imagePath, thumbnailPath, 200, 200);
                
                return thumbnailPath;
            }
            catch
            {
                // If thumbnail creation fails, just return null
                return null;
            }
        }

        /// <summary>
        /// Gets the directory for a specific shape
        /// </summary>
        /// <param name="shapeId">The ID of the shape</param>
        /// <returns>The absolute path to the shape's directory</returns>
        private string GetShapeDirectory(string shapeId)
        {
            return Path.Combine(_baseStoragePath, "shapes", shapeId);
        }

        /// <summary>
        /// Gets the attachments directory for a specific shape and attachment type
        /// </summary>
        /// <param name="shapeId">The ID of the shape</param>
        /// <param name="attachmentType">The type of attachments in this directory</param>
        /// <returns>The absolute path to the attachments directory</returns>
        private string GetAttachmentsDirectory(string shapeId, AttachmentType attachmentType)
        {
            string typeFolderName = attachmentType.ToString().ToLowerInvariant() + "s";
            return Path.Combine(GetShapeDirectory(shapeId), typeFolderName);
        }

        /// <summary>
        /// Gets the base storage path used by this service
        /// </summary>
        /// <returns>The base storage path</returns>
        public string GetBaseStoragePath()
        {
            return _baseStoragePath;
        }

        /// <summary>
        /// Gets a path relative to the base storage path
        /// </summary>
        /// <param name="fullPath">The absolute path</param>
        /// <returns>The relative path</returns>
        private string GetRelativePath(string fullPath)
        {
            // Create a URI for the full path and the base path
            Uri fullPathUri = new Uri(fullPath);
            Uri basePathUri = new Uri(_baseStoragePath + Path.DirectorySeparatorChar);

            // Return the relative path
            return Uri.UnescapeDataString(basePathUri.MakeRelativeUri(fullPathUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Disposes resources used by the StorageService
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                // Currently no disposable resources, but this allows for future expansion
            }

            _disposed = true;
        }
    }
}