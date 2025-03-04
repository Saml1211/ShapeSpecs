using System;

namespace ShapeSpecs.Core.Models
{
    /// <summary>
    /// Represents a file or link attachment associated with a Visio shape
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Unique identifier for the attachment
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Type of attachment (image, pdf, document, link)
        /// </summary>
        public AttachmentType Type { get; set; }

        /// <summary>
        /// Name or description of the attachment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Relative path or URL to the attachment
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Size of the attachment in bytes (0 for links)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Date and time when the attachment was added
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// Optional MIME type for the attachment
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Optional thumbnail path for image attachments
        /// </summary>
        public string ThumbnailPath { get; set; }
    }

    /// <summary>
    /// Defines the types of attachments supported by ShapeSpecs
    /// </summary>
    public enum AttachmentType
    {
        Image,
        PDF,
        Document,
        Link,
        Other
    }
}