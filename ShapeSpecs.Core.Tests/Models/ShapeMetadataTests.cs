using System;
using NUnit.Framework;
using ShapeSpecs.Core.Models;

namespace ShapeSpecs.Core.Tests.Models
{
    [TestFixture]
    public class ShapeMetadataTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act
            var metadata = new ShapeMetadata();

            // Assert
            Assert.IsNotNull(metadata.Id);
            Assert.IsNotNull(metadata.ShapeId);
            Assert.IsNotNull(metadata.TextSpecifications);
            Assert.IsNotNull(metadata.Attachments);
            Assert.IsNotNull(metadata.Notes);
            Assert.AreNotEqual(Guid.Empty, metadata.Id);
        }

        [Test]
        public void TextSpecifications_ShouldBeModifiable()
        {
            // Arrange
            var metadata = new ShapeMetadata();

            // Act
            metadata.TextSpecifications["TestKey"] = "TestValue";

            // Assert
            Assert.AreEqual("TestValue", metadata.TextSpecifications["TestKey"]);
        }

        [Test]
        public void Attachments_ShouldAllowAddingItems()
        {
            // Arrange
            var metadata = new ShapeMetadata();
            var attachment = new Attachment
            {
                Name = "Test.pdf",
                Type = AttachmentType.PDF
            };

            // Act
            metadata.Attachments.Add(attachment);

            // Assert
            Assert.AreEqual(1, metadata.Attachments.Count);
            Assert.AreEqual("Test.pdf", metadata.Attachments[0].Name);
        }

        [Test]
        public void Notes_ShouldAllowAddingItems()
        {
            // Arrange
            var metadata = new ShapeMetadata();
            var note = new Note
            {
                Text = "Test note",
                Author = "Test Author"
            };

            // Act
            metadata.Notes.Add(note);

            // Assert
            Assert.AreEqual(1, metadata.Notes.Count);
            Assert.AreEqual("Test note", metadata.Notes[0].Text);
        }

        [Test]
        public void LastModified_ShouldBeSettable()
        {
            // Arrange
            var metadata = new ShapeMetadata();
            var testDate = new DateTime(2025, 1, 1, 12, 0, 0);

            // Act
            metadata.LastModified = testDate;

            // Assert
            Assert.AreEqual(testDate, metadata.LastModified);
        }
    }
}
