using System.IO;
using System.Text;

namespace tEngine.Helpers
{
    public class FileIO
    {
        private static string mLastError = "";
        private static object mLockRead = new object();
        private static object mLockWrite = new object();

        public static bool CreateDirectory(string path, bool empty = false)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (empty)
            {
                dinfo.Delete();
            }
            if (!dinfo.Exists)
            {
                dinfo.Create();
            }
            return true;
        }

        public static string GetLastError() => mLastError;

        public static bool ReadBytes(string filePath, out byte[] bytes)
        {
            FileInfo finfo = new FileInfo(filePath);
            if (finfo.Exists == false)
            {
                bytes = null;
                return false;
            }
            using (FileStream infile = File.OpenRead(filePath))
            {
                byte[] fileBytes = new byte[infile.Length];
                infile.Read(fileBytes, 0, fileBytes.Length);
                bytes = fileBytes;
            }
            return true;
        }

        public static bool ReadText(string filePath, out string text)
        {
            FileInfo finfo = new FileInfo(filePath);
            if (finfo.Exists == false)
            {
                text = "";
                return false;
            }
            StringBuilder sb = new StringBuilder();
            using (StreamReader infile = new StreamReader(filePath, false))
            {
                sb.Append(infile.ReadToEnd());
            }
            text = sb.ToString();
            return true;
        }

        public static void WriteBytes(string filePath, byte[] bytes)
        {
            FileInfo finfo = new FileInfo(filePath);
            if (finfo.Directory != null)
            {
                if (finfo.Directory.Exists == false)
                {
                    CreateDirectory(finfo.Directory.FullName);
                }
            }
            using (FileStream outfile = File.OpenWrite(filePath))
            {
                outfile.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteText(string filePath, string text)
        {
            FileInfo finfo = new FileInfo(filePath);
            if (finfo.Directory != null)
            {
                if (finfo.Directory.Exists == false)
                {
                    CreateDirectory(finfo.Directory.FullName);
                }
            }
            using (StreamWriter outfile = new StreamWriter(filePath, false))
            {
                outfile.Write(text);
            }
        }
    }
}