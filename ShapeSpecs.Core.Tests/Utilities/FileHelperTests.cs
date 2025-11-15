using System;
using System.IO;
using NUnit.Framework;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Tests.Utilities
{
    [TestFixture]
    public class FileHelperTests
    {
        private FileHelper _fileHelper;
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _fileHelper = new FileHelper();
            _testDirectory = Path.Combine(Path.GetTempPath(), "ShapeSpecsTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Test]
        public void GetMimeType_ShouldReturnCorrectTypeForPdf()
        {
            // Act
            var mimeType = _fileHelper.GetMimeType("test.pdf");

            // Assert
            Assert.AreEqual("application/pdf", mimeType);
        }

        [Test]
        public void GetMimeType_ShouldReturnCorrectTypeForJpeg()
        {
            // Act
            var mimeType = _fileHelper.GetMimeType("test.jpg");

            // Assert
            Assert.AreEqual("image/jpeg", mimeType);
        }

        [Test]
        public void GetMimeType_ShouldReturnDefaultForUnknownExtension()
        {
            // Act
            var mimeType = _fileHelper.GetMimeType("test.unknown");

            // Assert
            Assert.AreEqual("application/octet-stream", mimeType);
        }

        [Test]
        public void ValidateFile_ShouldReturnFalseForNonExistentFile()
        {
            // Act
            var isValid = _fileHelper.ValidateFile("nonexistent.txt");

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateFile_ShouldReturnTrueForValidFile()
        {
            // Arrange
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Test content");

            // Act
            var isValid = _fileHelper.ValidateFile(testFile, 1024 * 1024); // 1MB limit

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void DeleteFileIfExists_ShouldReturnFalseForNonExistentFile()
        {
            // Act
            var result = _fileHelper.DeleteFileIfExists("nonexistent.txt");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteFileIfExists_ShouldDeleteFileAndReturnTrue()
        {
            // Arrange
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Test content");

            // Act
            var result = _fileHelper.DeleteFileIfExists(testFile);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(testFile));
        }

        [Test]
        public void GetUniqueFilename_ShouldReturnOriginalIfNotExists()
        {
            // Arrange
            var filename = "test.txt";

            // Act
            var uniquePath = _fileHelper.GetUniqueFilename(_testDirectory, filename);

            // Assert
            Assert.AreEqual(Path.Combine(_testDirectory, filename), uniquePath);
        }

        [Test]
        public void GetUniqueFilename_ShouldAppendNumberIfExists()
        {
            // Arrange
            var filename = "test.txt";
            var existingFile = Path.Combine(_testDirectory, filename);
            File.WriteAllText(existingFile, "Test");

            // Act
            var uniquePath = _fileHelper.GetUniqueFilename(_testDirectory, filename);

            // Assert
            Assert.AreEqual(Path.Combine(_testDirectory, "test_1.txt"), uniquePath);
        }

        [Test]
        public void CopyFile_ShouldCopyFileSuccessfully()
        {
            // Arrange
            var sourceFile = Path.Combine(_testDirectory, "source.txt");
            var destFile = Path.Combine(_testDirectory, "dest.txt");
            File.WriteAllText(sourceFile, "Test content");

            // Act
            _fileHelper.CopyFile(sourceFile, destFile);

            // Assert
            Assert.IsTrue(File.Exists(destFile));
            Assert.AreEqual("Test content", File.ReadAllText(destFile));
        }

        [Test]
        public void CopyFile_ShouldThrowExceptionForNonExistentSource()
        {
            // Arrange
            var sourceFile = "nonexistent.txt";
            var destFile = Path.Combine(_testDirectory, "dest.txt");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _fileHelper.CopyFile(sourceFile, destFile));
        }
    }
}
