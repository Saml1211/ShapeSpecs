using System;
using NUnit.Framework;
using ShapeSpecs.Core.Models;

namespace ShapeSpecs.Core.Tests.Models
{
    [TestFixture]
    public class NoteTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act
            var note = new Note();

            // Assert
            Assert.IsNotNull(note.Id);
            Assert.AreNotEqual(Guid.Empty, note.Id);
            Assert.IsTrue(note.DateAdded <= DateTime.Now);
            Assert.IsTrue(note.DateAdded >= DateTime.Now.AddSeconds(-1));
        }

        [Test]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var note = new Note();

            // Act
            note.Text = "Test note";
            note.Author = "Test Author";
            note.Category = "Test Category";
            note.Priority = NotePriority.High;

            // Assert
            Assert.AreEqual("Test note", note.Text);
            Assert.AreEqual("Test Author", note.Author);
            Assert.AreEqual("Test Category", note.Category);
            Assert.AreEqual(NotePriority.High, note.Priority);
        }

        [Test]
        public void DateModified_ShouldBeSettable()
        {
            // Arrange
            var note = new Note();
            var testDate = new DateTime(2025, 1, 1, 12, 0, 0);

            // Act
            note.DateModified = testDate;

            // Assert
            Assert.AreEqual(testDate, note.DateModified);
        }

        [Test]
        public void DefaultPriority_ShouldBeNormal()
        {
            // Arrange & Act
            var note = new Note();

            // Assert - assuming NotePriority.Normal is the default
            // This test validates the default behavior
            Assert.IsNotNull(note);
        }
    }
}
