using System;
using System.IO;
using NUnit.Framework;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Services;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Tests.Services
{
    [TestFixture]
    public class StorageServiceTests
    {
        private StorageService _storageService;
        private JsonHelper _jsonHelper;
        private FileHelper _fileHelper;
        private string _testStoragePath;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonHelper();
            _fileHelper = new FileHelper();
            _testStoragePath = Path.Combine(Path.GetTempPath(), "ShapeSpecsTests_" + Guid.NewGuid().ToString());
            _storageService = new StorageService(_testStoragePath, _jsonHelper, _fileHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _storageService?.Dispose();

            if (Directory.Exists(_testStoragePath))
            {
                try
                {
                    Directory.Delete(_testStoragePath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Test]
        public void Constructor_ShouldCreateStorageDirectory()
        {
            // Assert
            Assert.IsTrue(Directory.Exists(_testStoragePath));
        }

        [Test]
        public void SaveShapeMetadata_ShouldCreateMetadataFile()
        {
            // Arrange
            var metadata = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker"
            };

            // Act
            var reference = _storageService.SaveShapeMetadata(metadata);

            // Assert
            Assert.IsNotNull(reference);
            Assert.IsFalse(string.IsNullOrEmpty(reference));
        }

        [Test]
        public void LoadShapeMetadata_ShouldReturnNewMetadataForNonExistentFile()
        {
            // Act
            var metadata = _storageService.LoadShapeMetadata("Test_1", "nonexistent/metadata.json");

            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Test_1", metadata.ShapeId);
        }

        [Test]
        public void SaveAndLoad_ShouldPreserveMetadata()
        {
            // Arrange
            var original = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };
            original.TextSpecifications["Power"] = "100W";

            // Act
            var reference = _storageService.SaveShapeMetadata(original);
            var loaded = _storageService.LoadShapeMetadata("Test_1", reference);

            // Assert
            Assert.AreEqual(original.ShapeId, loaded.ShapeId);
            Assert.AreEqual(original.DeviceType, loaded.DeviceType);
            Assert.AreEqual(original.Model, loaded.Model);
            Assert.AreEqual("100W", loaded.TextSpecifications["Power"]);
        }

        [Test]
        public void GetBaseStoragePath_ShouldReturnCorrectPath()
        {
            // Act
            var path = _storageService.GetBaseStoragePath();

            // Assert
            Assert.AreEqual(_testStoragePath, path);
        }

        [Test]
        public void AddAttachment_ShouldCreateAttachmentFile()
        {
            // Arrange
            var metadata = new ShapeMetadata { ShapeId = "Test_1" };
            var tempFile = Path.Combine(Path.GetTempPath(), "test.txt");
            File.WriteAllText(tempFile, "Test content");

            try
            {
                // Act
                var updated = _storageService.AddAttachment(metadata, tempFile, AttachmentType.Document, "Test.txt");

                // Assert
                Assert.AreEqual(1, updated.Attachments.Count);
                Assert.AreEqual("Test.txt", updated.Attachments[0].Name);
                Assert.AreEqual(AttachmentType.Document, updated.Attachments[0].Type);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
