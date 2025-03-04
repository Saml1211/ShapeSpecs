using System;

namespace ShapeSpecs.Core.Models
{
    /// <summary>
    /// Represents a note or comment associated with a Visio shape
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Unique identifier for the note
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Text content of the note
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Author of the note
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Date and time when the note was created
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// Date and time when the note was last modified
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Optional category or tag for the note
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Priority of the note (if applicable)
        /// </summary>
        public NotePriority Priority { get; set; } = NotePriority.Normal;
    }

    /// <summary>
    /// Defines priority levels for notes
    /// </summary>
    public enum NotePriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}