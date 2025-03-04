using System;
using System.Collections.Generic;

namespace ShapeSpecs.Core.Models
{
    /// <summary>
    /// Represents metadata and specifications associated with a Visio shape
    /// </summary>
    public class ShapeMetadata
    {
        /// <summary>
        /// Unique identifier for the shape
        /// </summary>
        public string ShapeId { get; set; }

        /// <summary>
        /// Type of AV device this shape represents
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Model information for the device
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Dictionary of text-based specifications for the device
        /// </summary>
        public Dictionary<string, string> TextSpecifications { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// List of file attachments associated with this shape
        /// </summary>
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        /// <summary>
        /// List of notes associated with this shape
        /// </summary>
        public List<Note> Notes { get; set; } = new List<Note>();

        /// <summary>
        /// Date and time when this metadata was last modified
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Optional user who last modified the metadata
        /// </summary>
        public string LastModifiedBy { get; set; }
    }
}