using System;
using System.IO;
using NUnit.Framework;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Services;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Tests.Services
{
    [TestFixture]
    public class ImportExportTests
    {
        private JsonHelper _jsonHelper;
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonHelper();
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
        public void Export_ShouldCreateJsonFile()
        {
            // Arrange
            var metadata = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };
            metadata.TextSpecifications["Power"] = "100W";
            metadata.TextSpecifications["Impedance"] = "8 Ohms";

            var exportPath = Path.Combine(_testDirectory, "export.json");

            // Act
            _jsonHelper.SerializeToFile(metadata, exportPath);

            // Assert
            Assert.IsTrue(File.Exists(exportPath));
            var content = File.ReadAllText(exportPath);
            Assert.IsTrue(content.Contains("Test_1"));
            Assert.IsTrue(content.Contains("Speaker"));
            Assert.IsTrue(content.Contains("100W"));
        }

        [Test]
        public void Import_ShouldLoadJsonFile()
        {
            // Arrange
            var original = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };
            original.TextSpecifications["Power"] = "100W";

            var filePath = Path.Combine(_testDirectory, "import.json");
            _jsonHelper.SerializeToFile(original, filePath);

            // Act
            var imported = _jsonHelper.DeserializeFromFile<ShapeMetadata>(filePath);

            // Assert
            Assert.IsNotNull(imported);
            Assert.AreEqual("Test_1", imported.ShapeId);
            Assert.AreEqual("Speaker", imported.DeviceType);
            Assert.AreEqual("100W", imported.TextSpecifications["Power"]);
        }

        [Test]
        public void ImportExport_RoundTrip_ShouldPreserveAllData()
        {
            // Arrange
            var original = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };
            original.TextSpecifications["Power"] = "100W";
            original.TextSpecifications["Impedance"] = "8 Ohms";
            original.Notes.Add(new Note
            {
                Text = "Test note",
                Author = "Test User",
                Priority = NotePriority.High
            });

            var filePath = Path.Combine(_testDirectory, "roundtrip.json");

            // Act
            _jsonHelper.SerializeToFile(original, filePath);
            var imported = _jsonHelper.DeserializeFromFile<ShapeMetadata>(filePath);

            // Assert
            Assert.AreEqual(original.ShapeId, imported.ShapeId);
            Assert.AreEqual(original.DeviceType, imported.DeviceType);
            Assert.AreEqual(original.Model, imported.Model);
            Assert.AreEqual(2, imported.TextSpecifications.Count);
            Assert.AreEqual("100W", imported.TextSpecifications["Power"]);
            Assert.AreEqual("8 Ohms", imported.TextSpecifications["Impedance"]);
            Assert.AreEqual(1, imported.Notes.Count);
            Assert.AreEqual("Test note", imported.Notes[0].Text);
        }
    }
}
