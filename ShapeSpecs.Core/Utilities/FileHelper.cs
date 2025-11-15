using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShapeSpecs.Core.Utilities
{
    /// <summary>
    /// Helper class for file operations
    /// </summary>
    public class FileHelper
    {
        // Dictionary mapping file extensions to MIME types
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".tiff", "image/tiff" },
            { ".svg", "image/svg+xml" },
            
            // Documents
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".txt", "text/plain" },
            { ".rtf", "application/rtf" },
            { ".html", "text/html" },
            { ".htm", "text/html" },
            
            // Other
            { ".zip", "application/zip" },
            { ".xml", "application/xml" },
            { ".json", "application/json" }
        };

        /// <summary>
        /// Copies a file from one location to another
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to copy the file to</param>
        /// <param name="overwrite">Whether to overwrite the destination if it exists</param>
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Source path cannot be null or empty", nameof(sourcePath));
            
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination path cannot be null or empty", nameof(destinationPath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            // Create the destination directory if it doesn't exist
            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Copy the file
            File.Copy(sourcePath, destinationPath, overwrite);
        }

        /// <summary>
        /// Asynchronously copies a file from one location to another
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to copy the file to</param>
        /// <param name="overwrite">Whether to overwrite the destination if it exists</param>
        public async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Source path cannot be null or empty", nameof(sourcePath));

            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination path cannot be null or empty", nameof(destinationPath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            // Create the destination directory if it doesn't exist
            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // If destination exists and we're not overwriting, throw exception
            if (!overwrite && File.Exists(destinationPath))
            {
                throw new IOException($"Destination file already exists: {destinationPath}");
            }

            // Copy the file asynchronously using streams
            const int bufferSize = 81920; // 80KB buffer
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream, bufferSize).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a thumbnail from an image file
        /// </summary>
        /// <param name="imagePath">Path to the source image</param>
        /// <param name="thumbnailPath">Path to save the thumbnail to</param>
        /// <param name="maxWidth">Maximum width of the thumbnail</param>
        /// <param name="maxHeight">Maximum height of the thumbnail</param>
        /// <returns>True if the thumbnail was created successfully, false otherwise</returns>
        public bool CreateThumbnail(string imagePath, string thumbnailPath, int maxWidth, int maxHeight)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));
                
                if (string.IsNullOrEmpty(thumbnailPath))
                    throw new ArgumentException("Thumbnail path cannot be null or empty", nameof(thumbnailPath));

                if (!File.Exists(imagePath))
                    throw new FileNotFoundException("Image file not found", imagePath);

                // Create the thumbnail directory if it doesn't exist
                string thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);
                if (!string.IsNullOrEmpty(thumbnailDirectory) && !Directory.Exists(thumbnailDirectory))
                {
                    Directory.CreateDirectory(thumbnailDirectory);
                }

                // Load the image
                using (var image = Image.FromFile(imagePath))
                {
                    // Calculate the thumbnail dimensions while maintaining aspect ratio
                    int width = image.Width;
                    int height = image.Height;
                    
                    if (width > maxWidth || height > maxHeight)
                    {
                        float ratio = Math.Min((float)maxWidth / width, (float)maxHeight / height);
                        width = (int)(width * ratio);
                        height = (int)(height * ratio);
                    }

                    // Create the thumbnail
                    using (var thumbnail = new Bitmap(width, height))
                    {
                        using (var graphics = Graphics.FromImage(thumbnail))
                        {
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.DrawImage(image, 0, 0, width, height);
                        }

                        // Save the thumbnail
                        thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
                    }
                }

                return true;
            }
            catch
            {
                // If thumbnail creation fails for any reason, return false
                return false;
            }
        }

        /// <summary>
        /// Gets the MIME type for a file based on its extension
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>The MIME type, or "application/octet-stream" if unknown</returns>
        public string GetMimeType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "application/octet-stream";

            string extension = Path.GetExtension(filePath);
            
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            return MimeTypes.TryGetValue(extension, out string mimeType) 
                ? mimeType 
                : "application/octet-stream";
        }

        /// <summary>
        /// Validates that a file exists and is not too large
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="maxSizeBytes">Maximum allowed size in bytes</param>
        /// <returns>True if the file is valid, false otherwise</returns>
        public bool ValidateFile(string filePath, long maxSizeBytes = 10485760) // Default to 10MB
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (!File.Exists(filePath))
                return false;

            // Check file size
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length <= maxSizeBytes;
        }

        /// <summary>
        /// Gets a unique filename in a directory by appending a number if needed
        /// </summary>
        /// <param name="directory">Directory to create the file in</param>
        /// <param name="filename">Desired filename</param>
        /// <returns>A unique filename that doesn't exist in the directory</returns>
        public string GetUniqueFilename(string directory, string filename)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Directory cannot be null or empty", nameof(directory));
            
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string fullPath = Path.Combine(directory, filename);
            int counter = 1;

            while (File.Exists(fullPath))
            {
                string newFilename = $"{baseFilename}_{counter}{extension}";
                fullPath = Path.Combine(directory, newFilename);
                counter++;
            }

            return fullPath;
        }

        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        /// <param name="filePath">Path to the file to delete</param>
        /// <returns>True if the file was deleted, false if it didn't exist</returns>
        public bool DeleteFileIfExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (!File.Exists(filePath))
                return false;

            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}