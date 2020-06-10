using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;

namespace XRayBuilder.Core.Libraries.Serialization.Xml.Util
{
    public static class XmlUtil
    {
        private static readonly Dictionary<Type, XmlSerializer> Serializers = new Dictionary<Type, XmlSerializer>();

        private static XmlSerializer GetCachedOrCreate(Type type)
        {
            var serializer = Serializers.GetOrDefault(type);
            if (serializer != null)
                return serializer;

            serializer = new XmlSerializer(type);
            Serializers.Add(type, serializer);

            return serializer;
        }

        public static T Deserialize<T>(string xml)
        {
            using var reader = new StringReader(xml);
            var serializer = GetCachedOrCreate(typeof(T));

            return (T) serializer.Deserialize(reader);
        }

        public static string Serialize<T>(T anything, bool includeDeclaration = false, bool includeNamespaces = false)
        {
            using var writer = new Utf8StringWriter(CultureInfo.InvariantCulture);
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = !includeDeclaration,
                Encoding = Encoding.UTF8
            };
            var names = new XmlSerializerNamespaces();
            names.Add("", "");
            using var xmlWriter = XmlWriter.Create(writer, settings);
            var serializer = GetCachedOrCreate(typeof(T));
            if (includeNamespaces)
                serializer.Serialize(xmlWriter, anything);
            else
                serializer.Serialize(xmlWriter, anything, names);
            return writer.ToString();
        }

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

        public sealed class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter([CanBeNull] IFormatProvider formatProvider = null) : base(formatProvider) { }

            // Use UTF8 encoding but write no BOM
            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }
}