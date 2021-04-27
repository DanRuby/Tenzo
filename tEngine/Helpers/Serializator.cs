using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace tEngine.Helpers
{/*
    public class Serializator
    {
        public static T DeserializeClass<T>(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return default(T);
                }
                XmlSerializer reader = new XmlSerializer(typeof(T));
                T result;
                using (StreamReader file = new StreamReader(filepath))
                {
                    result = (T)reader.Deserialize(file);
                }
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T DeserializeClassBinary<T>(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return default(T);
                }
                BinaryFormatter reader = new BinaryFormatter();
                T result;
                using (StreamReader file = new StreamReader(filepath))
                {
                    result = (T)reader.Deserialize(file.BaseStream);
                }
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T DeserializeDataContract<T>(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return default(T);
                }
                DataContractSerializer reader = new DataContractSerializer(typeof(T));
                T result;
                using (XmlReader file = XmlReader.Create(filepath))
                {
                    result = (T)reader.ReadObject(file);
                }
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T DeserializeDataContractBinary<T>(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return default(T);
                }
                DataContractSerializer reader = new DataContractSerializer(typeof(T));
                T result;

                using (StreamReader file = new StreamReader(filepath))
                {
                    XmlDictionaryReader br = XmlDictionaryReader.CreateBinaryReader(file.BaseStream, XmlDictionaryReaderQuotas.Max);
                    result = (T)reader.ReadObject(br);
                }
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T DeserializeFromString<T>(string xmlText)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringReader stringReader = new StringReader(xmlText))
                {
                    return (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static bool SerializeClass<T>(T value, string filepath)
        {
            try
            {
                if (value.Equals(default(T)))
                {
                    return false;
                }
                XmlSerializer writer = new XmlSerializer(typeof(T));
                using (StreamWriter file = new StreamWriter(filepath))
                {
                    writer.Serialize(file, value);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SerializeClassBinary(Object obj, string filepath)
        {
            try
            {
                if (obj == null)
                {
                    return false;
                }
                BinaryFormatter writer = new BinaryFormatter();
                using (StreamWriter file = new StreamWriter(filepath))
                {
                    writer.Serialize(file.BaseStream, obj);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SerializeDataContrakt<T>(T value, string filepath)
        {
            try
            {
                if (value.Equals(default(T)))
                {
                    return false;
                }
                DataContractSerializer writer = new DataContractSerializer(typeof(T));
                using (XmlWriter file = XmlWriter.Create(filepath, new XmlWriterSettings() { NewLineChars = "lol" }))
                {
                    writer.WriteObject(file, value);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SerializeDataContraktBinary<T>(T value, string filepath)
        {
            try
            {
                if (value.Equals(default(T)))
                {
                    return false;
                }

                DataContractSerializer writer = new DataContractSerializer(typeof(T));
                using (StreamWriter file = new StreamWriter(filepath))
                {
                    XmlDictionaryWriter bw = XmlDictionaryWriter.CreateBinaryWriter(file.BaseStream);
                    writer.WriteObject(bw, value);
                    bw.Flush();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string SerializeToString<T>(T value)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringWriter stringwriter = new StringWriter())
                {
                    serializer.Serialize(stringwriter, value);
                    return stringwriter.ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }*/
}