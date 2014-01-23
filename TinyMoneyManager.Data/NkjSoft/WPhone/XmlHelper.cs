namespace NkjSoft.WPhone
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlHelper
    {
        public static TResult DeserializeFromXmlString<TResult>(System.IO.FileStream xmlSourceFileStream)
        {
            TResult local = default(TResult);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TResult));
                local = (TResult) serializer.Deserialize(xmlSourceFileStream);
                xmlSourceFileStream.Close();
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            return local;
        }

        public static TResult DeserializeFromXmlString<TResult>(string xmlSource)
        {
            TResult local = default(TResult);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TResult));
                System.IO.StringReader textReader = new System.IO.StringReader(xmlSource);
                local = (TResult) serializer.Deserialize(textReader);
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            return local;
        }

        public static string SerializeToXmlString<T>(T source)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true
            };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (System.IO.StringWriter writer = new System.IO.StringWriter(sb))
            {
                try
                {
                    new XmlSerializer(typeof(T)).Serialize(writer, source);
                }
                catch (System.Exception)
                {
                    throw;
                }
                finally
                {
                    writer.Close();
                }
            }
            return sb.ToString();
        }

        public static void SerializeToXmlString<T>(T source, System.IO.Stream fileWriteTo)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true
            };
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileWriteTo))
            {
                try
                {
                    new XmlSerializer(typeof(T)).Serialize(writer, source);
                }
                catch (System.Exception)
                {
                    throw;
                }
                finally
                {
                    writer.Close();
                    fileWriteTo.Close();
                }
            }
        }
    }
}

