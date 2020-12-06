using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace tEngine.Helpers {
    public class FileIO {
        private static string mLastError = "";
        private static object mLockRead = new object();
        private static object mLockWrite = new object();

        public static bool CreateDirectory( string path, bool empty = false ) {
            var dinfo = new DirectoryInfo( path );
            if( empty ) {
                dinfo.Delete();
            }
            if( !dinfo.Exists ) {
                dinfo.Create();
            }
            return true;
        }

        public static string GetLastError() {
            return mLastError;
        }

        public static bool ReadBytes( string filePath, out byte[] bytes ) {
            var finfo = new FileInfo( filePath );
            if( finfo.Exists == false ) {
                bytes = null;
                return false;
            }
            using( var infile = File.OpenRead( filePath ) ) {
                byte[] fileBytes = new byte[infile.Length];
                infile.Read( fileBytes, 0, fileBytes.Length );
                bytes = fileBytes;
            }
            return true;
        }

        public static bool ReadText( string filePath, out string text ) {
            var finfo = new FileInfo( filePath );
            if( finfo.Exists == false ) {
                text = "";
                return false;
            }
            var sb = new StringBuilder();
            using( var infile = new StreamReader( filePath, false ) ) {
                sb.Append( infile.ReadToEnd() );
            }
            text = sb.ToString();
            return true;
        }

        public static void WriteBytes( string filePath, byte[] bytes ) {
            var finfo = new FileInfo( filePath );
            if( finfo.Directory != null ) {
                if( finfo.Directory.Exists == false ) {
                    CreateDirectory( finfo.Directory.FullName );
                }
            }
            using( var outfile = File.OpenWrite( filePath ) ) {
                outfile.Write( bytes, 0, bytes.Length );
            }
        }

        public static void WriteText( string filePath, string text ) {
            var finfo = new FileInfo( filePath );
            if( finfo.Directory != null ) {
                if( finfo.Directory.Exists == false ) {
                    CreateDirectory( finfo.Directory.FullName );
                }
            }
            using( var outfile = new StreamWriter( filePath, false ) ) {
                outfile.Write( text );
            }
        }
    }
}