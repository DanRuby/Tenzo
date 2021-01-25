using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using OxyPlot;
using tEngine.PlotCreator;

namespace tEngine.Helpers {
    /// <summary>
    /// Д: копирует все поля или свойсва объекта
    /// </summary>
    public class Cloner {
        public static object Clone( object something ) {
            var serializer = new XmlSerializer( something.GetType() );
            var tempStream = new MemoryStream();
            serializer.Serialize( tempStream, something );
            tempStream.Seek( 0, SeekOrigin.Begin );
            return serializer.Deserialize( tempStream );
        }

        public static void CopyAllFields( object dest, object src ) {
            var srcFields = src.GetType().GetFields();
            var destFields = dest.GetType().GetFields();
            foreach( var fiSrc in srcFields ) {
                // если источник можно прочитать
                if( fiSrc.IsPublic ) {
                    var fiDest = destFields.FirstOrDefault( pi => pi.Name.Equals( fiSrc.Name ) );
                    if( fiDest != null ) {
                        // если цель можно записать
                        if( fiDest.IsPublic ) {
                            if( fiDest.FieldType == fiSrc.FieldType ) {
                                fiDest.SetValue( dest, fiSrc.GetValue( src ) );
                            } else if( fiDest.FieldType == typeof( Color ) && fiSrc.FieldType == typeof( OxyColor ) ) {
                                var color = ((OxyColor) fiSrc.GetValue( src )).GetColorMedia();
                                fiDest.SetValue( dest, color );
                            } else if( fiDest.FieldType == typeof( OxyColor ) && fiSrc.FieldType == typeof( Color ) ) {
                                var color = ((Color) fiSrc.GetValue( src )).GetColorOxy();
                                fiDest.SetValue( dest, color );
                            }
                        }
                    }
                }
            }
        }

        public static void CopyAllProperties( object dest, object src ) {
            var srcProp = src.GetType().GetProperties();
            var destProp = dest.GetType().GetProperties();
            foreach( var piSrc in srcProp ) {
                // если источник можно прочитать
                if( piSrc.CanRead ) {
                    var piDest = destProp.FirstOrDefault( pi => pi.Name.Equals( piSrc.Name ) );
                    if( piDest != null ) {
                        // если цель можно записать
                        if( piDest.CanWrite ) {
                            if( piDest.PropertyType == piSrc.PropertyType ) {
                                piDest.SetValue( dest, piSrc.GetValue( src, null ), null );
                            } else if( piDest.PropertyType == typeof( Color ) && piSrc.PropertyType == typeof( OxyColor ) ) {
                                var color = ((OxyColor) piSrc.GetValue( src, null )).GetColorMedia();
                                piDest.SetValue( dest, color, null );
                            } else if( piDest.PropertyType == typeof( OxyColor ) &&
                                       piSrc.PropertyType == typeof( Color ) ) {
                                var color = ((Color) piSrc.GetValue( src, null )).GetColorOxy();
                                piDest.SetValue( dest, color, null );
                            }
                        }
                    }
                }
            }
        }
    }
}