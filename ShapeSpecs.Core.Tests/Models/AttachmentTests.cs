using System;
using NUnit.Framework;
using ShapeSpecs.Core.Models;

namespace ShapeSpecs.Core.Tests.Models
{
    [TestFixture]
    public class AttachmentTests
    {
        [Test]
        public void Constructor_ShouldInitializeId()
        {
            // Act
            var attachment = new Attachment();

            // Assert
            Assert.IsNotNull(attachment.Id);
            Assert.AreNotEqual(Guid.Empty.ToString(), attachment.Id);
        }

        [Test]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var attachment = new Attachment();

            // Act
            attachment.Name = "Test.pdf";
            attachment.Type = AttachmentType.PDF;
            attachment.Path = "/test/path";
            attachment.Size = 1024;
            attachment.MimeType = "application/pdf";

            // Assert
            Assert.AreEqual("Test.pdf", attachment.Name);
            Assert.AreEqual(AttachmentType.PDF, attachment.Type);
            Assert.AreEqual("/test/path", attachment.Path);
            Assert.AreEqual(1024, attachment.Size);
            Assert.AreEqual("application/pdf", attachment.MimeType);
        }

        [Test]
        public void DateAdded_ShouldBeSettable()
        {
            // Arrange
            var attachment = new Attachment();
            var testDate = new DateTime(2025, 1, 1, 12, 0, 0);

            // Act
            attachment.DateAdded = testDate;

            // Assert
            Assert.AreEqual(testDate, attachment.DateAdded);
        }

        [Test]
        public void ThumbnailPath_ShouldBeOptional()
        {
            // Arrange & Act
            var attachment = new Attachment();

            // Assert
            Assert.IsNull(attachment.ThumbnailPath);
        }
    }
}
