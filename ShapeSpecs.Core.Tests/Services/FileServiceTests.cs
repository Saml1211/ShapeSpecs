using System;
using System.IO;
using NUnit.Framework;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Services;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Tests.Services
{
    [TestFixture]
    public class FileServiceTests
    {
        private FileService _fileService;
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
            _fileService = new FileService(_fileHelper, _storageService);
        }

        [TearDown]
        public void TearDown()
        {
            _fileService?.Dispose();
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
        public void ImportFile_ShouldAddAttachmentToMetadata()
        {
            // Arrange
            var metadata = new ShapeMetadata { ShapeId = "Test_1" };
            var tempFile = Path.Combine(Path.GetTempPath(), "test.pdf");
            File.WriteAllText(tempFile, "Test PDF content");

            try
            {
                // Act
                var updated = _fileService.ImportFile(metadata, tempFile, "TestDocument.pdf");

                // Assert
                Assert.AreEqual(1, updated.Attachments.Count);
                Assert.AreEqual("TestDocument.pdf", updated.Attachments[0].Name);
                Assert.AreEqual(AttachmentType.PDF, updated.Attachments[0].Type);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Test]
        public void ImportFile_ShouldDetermineAttachmentTypeFromExtension()
        {
            // Arrange
            var metadata = new ShapeMetadata { ShapeId = "Test_1" };
            var tempFile = Path.Combine(Path.GetTempPath(), "test.jpg");
            File.WriteAllText(tempFile, "Test image content");

            try
            {
                // Act
                var updated = _fileService.ImportFile(metadata, tempFile);

                // Assert
                Assert.AreEqual(AttachmentType.Image, updated.Attachments[0].Type);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Test]
        public void DeleteAttachment_ShouldRemoveAttachmentFromMetadata()
        {
            // Arrange
            var metadata = new ShapeMetadata { ShapeId = "Test_1" };
            var tempFile = Path.Combine(Path.GetTempPath(), "test.txt");
            File.WriteAllText(tempFile, "Test content");

            try
            {
                var updated = _fileService.ImportFile(metadata, tempFile, "Test.txt");
                var attachmentId = updated.Attachments[0].Id;

                // Act
                var final = _fileService.DeleteAttachment(updated, attachmentId);

                // Assert
                Assert.AreEqual(0, final.Attachments.Count);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
