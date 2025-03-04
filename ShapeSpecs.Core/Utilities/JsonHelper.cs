using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ShapeSpecs.Core.Utilities
{
    /// <summary>
    /// Helper class for JSON serialization and deserialization
    /// </summary>
    public class JsonHelper
    {
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Creates a new instance of the JsonHelper with default settings
        /// </summary>
        public JsonHelper()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        /// <summary>
        /// Creates a new instance of the JsonHelper with custom settings
        /// </summary>
        /// <param name="serializerSettings">Custom JSON serializer settings</param>
        public JsonHelper(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
        }

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>A JSON string representation of the object</returns>
        public string Serialize<T>(T obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonConvert.SerializeObject(obj, _serializerSettings);
        }

        /// <summary>
        /// Deserializes a JSON string to an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonConvert.DeserializeObject<T>(json, _serializerSettings);
        }

        /// <summary>
        /// Serializes an object to JSON and writes it to a file
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <param name="filePath">The path to write the JSON to</param>
        public void SerializeToFile<T>(T obj, string filePath)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            // Create the directory if it doesn't exist
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize the object to JSON
            string json = Serialize(obj);

            // Write the JSON to the file
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Reads a JSON file and deserializes it to an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="filePath">The path to the JSON file</param>
        /// <returns>The deserialized object</returns>
        public T DeserializeFromFile<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("JSON file not found", filePath);

            // Read the JSON from the file
            string json = File.ReadAllText(filePath, Encoding.UTF8);

            // Deserialize the JSON to an object
            return Deserialize<T>(json);
        }
    }
}