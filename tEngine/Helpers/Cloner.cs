using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using OxyPlot;
using tEngine.PlotCreator;

namespace tEngine.Helpers
{
    /// <summary>
    /// Класс с методами копирования объектов
    /// </summary>
    public class Cloner
    {
        public static object Clone(object something)
        {
            XmlSerializer serializer = new XmlSerializer(something.GetType());
            MemoryStream tempStream = new MemoryStream();
            serializer.Serialize(tempStream, something);
            tempStream.Seek(0, SeekOrigin.Begin);
            return serializer.Deserialize(tempStream);
        }

        /// <summary>
        /// Скопировать значения всех полей
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void CopyAllFields(object dest, object src)
        {
            System.Reflection.FieldInfo[] srcFields = src.GetType().GetFields();
            System.Reflection.FieldInfo[] destFields = dest.GetType().GetFields();
            foreach (System.Reflection.FieldInfo fiSrc in srcFields)
            {
                // если источник можно прочитать
                if (fiSrc.IsPublic)
                {
                    System.Reflection.FieldInfo fiDest = destFields.FirstOrDefault(pi => pi.Name.Equals(fiSrc.Name));
                    if (fiDest != null)
                    {
                        // если цель можно записать
                        if (fiDest.IsPublic)
                        {
                            if (fiDest.FieldType == fiSrc.FieldType)
                            {
                                fiDest.SetValue(dest, fiSrc.GetValue(src));
                            }
                            else if (fiDest.FieldType == typeof(Color) && fiSrc.FieldType == typeof(OxyColor))
                            {
                                Color color = ((OxyColor)fiSrc.GetValue(src)).GetColorMedia();
                                fiDest.SetValue(dest, color);
                            }
                            else if (fiDest.FieldType == typeof(OxyColor) && fiSrc.FieldType == typeof(Color))
                            {
                                OxyColor color = ((Color)fiSrc.GetValue(src)).GetColorOxy();
                                fiDest.SetValue(dest, color);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Скопировать значения всех свойств
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void CopyAllProperties(object dest, object src)
        {
            System.Reflection.PropertyInfo[] srcProp = src.GetType().GetProperties();
            System.Reflection.PropertyInfo[] destProp = dest.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo piSrc in srcProp)
            {
                // если источник можно прочитать
                if (piSrc.CanRead)
                {
                    System.Reflection.PropertyInfo piDest = destProp.FirstOrDefault(pi => pi.Name.Equals(piSrc.Name));
                    if (piDest != null)
                    {
                        // если цель можно записать
                        if (piDest.CanWrite)
                        {
                            if (piDest.PropertyType == piSrc.PropertyType)
                            {
                                piDest.SetValue(dest, piSrc.GetValue(src, null), null);
                            }
                            else if (piDest.PropertyType == typeof(Color) && piSrc.PropertyType == typeof(OxyColor))
                            {
                                Color color = ((OxyColor)piSrc.GetValue(src, null)).GetColorMedia();
                                piDest.SetValue(dest, color, null);
                            }
                            else if (piDest.PropertyType == typeof(OxyColor) &&
                                     piSrc.PropertyType == typeof(Color))
                            {
                                OxyColor color = ((Color)piSrc.GetValue(src, null)).GetColorOxy();
                                piDest.SetValue(dest, color, null);
                            }
                        }
                    }
                }
            }
        }
    }
}