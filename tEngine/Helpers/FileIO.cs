using System.IO;
using System.Text;

namespace tEngine.Helpers
{
    /// <summary>
    /// Предоставляет методы по работе с файловым вводом/выводом
    /// </summary>
    public static class FileIO
    {
        private static bool CreateDirectory(string path, bool empty = false)
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

        private static void CheckFilePath(string filePath)
        {
            FileInfo finfo = new FileInfo(filePath);
            if (finfo.Directory != null)
            {
                if (finfo.Directory.Exists == false)
                {
                    CreateDirectory(finfo.Directory.FullName);
                }
            }
        }

        /// <summary>
        /// Считать файл и вернуть байты
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        /// <returns>В случае неудачи возвращает false, иначе true</returns>
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

        /// <summary>
        /// Считать файл и вернуть строку
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <returns>В случае неудачи возвращает false, иначе true</returns>
        public static bool ReadString(string filePath, out string text)
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

        /// <summary>
        /// Записать байты в файл
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteBytes(string filePath, byte[] bytes)
        {
            CheckFilePath(filePath);
            using (FileStream outfile = File.OpenWrite(filePath))
            {
                outfile.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Записать строку в файл
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public static void WriteString(string filePath, string text)
        {
            CheckFilePath(filePath);
            using (StreamWriter outfile = new StreamWriter(filePath, false))
            {
                outfile.Write(text);
            }
        }

    
    }
}