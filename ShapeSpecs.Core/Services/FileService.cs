using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Services
{
    /// <summary>
    /// Service for managing file operations related to attachments
    /// </summary>
    public class FileService : IDisposable
    {
        private readonly FileHelper _fileHelper;
        private readonly StorageService _storageService;
        private static readonly HttpClient _httpClient = new HttpClient();
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of the FileService
        /// </summary>
        /// <param name="fileHelper">Helper for file operations</param>
        /// <param name="storageService">Service for storage operations</param>
        public FileService(FileHelper fileHelper, StorageService storageService)
        {
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        /// <summary>
        /// Imports a file as an attachment for a shape
        /// </summary>
        /// <param name="metadata">The shape metadata</param>
        /// <param name="filePath">Path to the file to import</param>
        /// <param name="attachmentName">Optional custom name for the attachment</param>
        /// <returns>The updated metadata with the attachment added</returns>
        public ShapeMetadata ImportFile(ShapeMetadata metadata, string filePath, string attachmentName = null)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            // Determine the attachment type based on the file extension
            AttachmentType attachmentType = DetermineAttachmentType(filePath);
            
            // Use the StorageService to add the attachment
            return _storageService.AddAttachment(metadata, filePath, attachmentType, attachmentName);
        }

        /// <summary>
        /// Imports a file from a URL as an attachment for a shape
        /// </summary>
        /// <param name="metadata">The shape metadata</param>
        /// <param name="url">URL of the file to import</param>
        /// <param name="attachmentName">Optional custom name for the attachment</param>
        /// <returns>The updated metadata with the attachment added</returns>
        public async Task<ShapeMetadata> ImportFileFromUrlAsync(ShapeMetadata metadata, string url, string attachmentName = null)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            try
            {
                // Create a temporary file to download to
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                // Download the file using HttpClient (modern replacement for WebClient)
                using (var response = await _httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }

                try
                {
                    // Determine a filename from the URL if no name was provided
                    if (string.IsNullOrEmpty(attachmentName))
                    {
                        attachmentName = Path.GetFileName(new Uri(url).LocalPath);
                    }

                    // Import the downloaded file
                    return ImportFile(metadata, tempFile, attachmentName);
                }
                finally
                {
                    // Clean up the temporary file
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download file from URL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to import file from URL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the absolute path to an attachment file
        /// </summary>
        /// <param name="metadata">The shape metadata</param>
        /// <param name="attachmentId">The ID of the attachment</param>
        /// <returns>The absolute path to the attachment file</returns>
        public string GetAttachmentPath(ShapeMetadata metadata, string attachmentId)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentException("Attachment ID cannot be null or empty", nameof(attachmentId));

            // Find the attachment in the metadata
            var attachment = metadata.Attachments.Find(a => a.Id == attachmentId);
            if (attachment == null)
                throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");

            // Get the base storage path from the storage service
            string basePath = _storageService.GetBaseStoragePath();
            
            // Combine with the relative path to get the absolute path
            return Path.Combine(basePath, attachment.Path);
        }

        /// <summary>
        /// Deletes an attachment from a shape's metadata
        /// </summary>
        /// <param name="metadata">The shape metadata</param>
        /// <param name="attachmentId">The ID of the attachment to delete</param>
        /// <returns>The updated metadata with the attachment removed</returns>
        public ShapeMetadata DeleteAttachment(ShapeMetadata metadata, string attachmentId)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentException("Attachment ID cannot be null or empty", nameof(attachmentId));

            // Find the attachment in the metadata
            var attachment = metadata.Attachments.Find(a => a.Id == attachmentId);
            if (attachment == null)
                throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");

            try
            {
                // Get the base storage path from the storage service
                string basePath = _storageService.GetBaseStoragePath();
                
                // Delete the attachment file
                string attachmentPath = Path.Combine(basePath, attachment.Path);
                if (File.Exists(attachmentPath))
                {
                    File.Delete(attachmentPath);
                }
                
                // Delete the thumbnail if it exists
                if (!string.IsNullOrEmpty(attachment.ThumbnailPath))
                {
                    string thumbnailPath = Path.Combine(basePath, attachment.ThumbnailPath);
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                    }
                }
                
                // Remove the attachment from the metadata
                metadata.Attachments.Remove(attachment);
                
                // Save the updated metadata
                _storageService.SaveShapeMetadata(metadata);
                
                return metadata;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete attachment: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determines the attachment type based on the file extension
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>The determined attachment type</returns>
        private AttachmentType DetermineAttachmentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // Check for image extensions
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || 
                extension == ".gif" || extension == ".bmp" || extension == ".tiff" || 
                extension == ".svg")
            {
                return AttachmentType.Image;
            }
            
            // Check for PDF
            if (extension == ".pdf")
            {
                return AttachmentType.PDF;
            }
            
            // Check for document extensions
            if (extension == ".doc" || extension == ".docx" || extension == ".txt" || 
                extension == ".rtf" || extension == ".xlsx" || extension == ".pptx" ||
                extension == ".html" || extension == ".htm")
            {
                return AttachmentType.Document;
            }
            
            // Default to Other
            return AttachmentType.Other;
        }

        /// <summary>
        /// Disposes resources used by the FileService
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
                // Note: HttpClient is static and shared, so we don't dispose it here
                // Future disposable resources can be added here
            }

            _disposed = true;
        }
    }
}