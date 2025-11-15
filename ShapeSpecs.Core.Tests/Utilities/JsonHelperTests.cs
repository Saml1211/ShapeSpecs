using System;
using System.IO;
using NUnit.Framework;
using ShapeSpecs.Core.Models;
using ShapeSpecs.Core.Utilities;

namespace ShapeSpecs.Core.Tests.Utilities
{
    [TestFixture]
    public class JsonHelperTests
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
        public void Serialize_ShouldConvertObjectToJson()
        {
            // Arrange
            var metadata = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };

            // Act
            var json = _jsonHelper.Serialize(metadata);

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("Test_1"));
            Assert.IsTrue(json.Contains("Speaker"));
        }

        [Test]
        public void Deserialize_ShouldConvertJsonToObject()
        {
            // Arrange
            var json = "{\"ShapeId\":\"Test_1\",\"DeviceType\":\"Speaker\",\"Model\":\"Test Model\"}";

            // Act
            var metadata = _jsonHelper.Deserialize<ShapeMetadata>(json);

            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Test_1", metadata.ShapeId);
            Assert.AreEqual("Speaker", metadata.DeviceType);
        }

        [Test]
        public void SerializeToFile_ShouldWriteJsonToFile()
        {
            // Arrange
            var metadata = new ShapeMetadata
            {
                ShapeId = "Test_1",
                DeviceType = "Speaker",
                Model = "Test Model"
            };
            var filePath = Path.Combine(_testDirectory, "test.json");

            // Act
            _jsonHelper.SerializeToFile(metadata, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.IsTrue(content.Contains("Test_1"));
        }

        [Test]
        public void DeserializeFromFile_ShouldReadJsonFromFile()
        {
            // Arrange
            var json = "{\"ShapeId\":\"Test_1\",\"DeviceType\":\"Speaker\",\"Model\":\"Test Model\"}";
            var filePath = Path.Combine(_testDirectory, "test.json");
            File.WriteAllText(filePath, json);

            // Act
            var metadata = _jsonHelper.DeserializeFromFile<ShapeMetadata>(filePath);

            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Test_1", metadata.ShapeId);
            Assert.AreEqual("Speaker", metadata.DeviceType);
        }

        [Test]
        public void RoundTrip_ShouldPreserveData()
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
            var json = _jsonHelper.Serialize(original);
            var deserialized = _jsonHelper.Deserialize<ShapeMetadata>(json);

            // Assert
            Assert.AreEqual(original.ShapeId, deserialized.ShapeId);
            Assert.AreEqual(original.DeviceType, deserialized.DeviceType);
            Assert.AreEqual(original.Model, deserialized.Model);
            Assert.AreEqual("100W", deserialized.TextSpecifications["Power"]);
        }
    }
}
