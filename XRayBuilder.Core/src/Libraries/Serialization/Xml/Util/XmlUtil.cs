using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace XRayBuilder.Core.Libraries.Serialization.Xml.Util
{
    public static class XmlUtil
    {
        // TODO split into string methods vs straight to file
        public static void SerializeToFile<T>(T output, string fileName) where T : class
        {
            using var writer = new StreamWriter(fileName, false, Encoding.UTF8);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, output);
            writer.Flush();
        }

        //http://stackoverflow.com/questions/14562415/xml-deserialization-generic-method
        public static T DeserializeFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"File not found: {filePath}");

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StreamReader(filePath, Encoding.UTF8);

            try
            {
                return (T) serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error processing XML file: {ex.Message}\r\nIf the error contains a (#, #), the first number is the line the error occurred on.", ex);
            }
        }
    }
}