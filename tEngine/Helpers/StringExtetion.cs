using System.IO;

namespace tEngine.Helpers
{
    /// <summary>
    /// Два дополнительных метода для string
    /// </summary>
    public static class StringExtetion
    {
        /// <summary>
        /// ..\path => ..\path\
        /// </summary>
        public static string CorrectSlash(this string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (!(path.EndsWith(@"\") || path.EndsWith("/")))
                path += @"\";
            return path;
        }

        public static string CutFileName(this string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            DirectoryInfo dinfo = new FileInfo(path).Directory;
            if (dinfo != null)
                return dinfo.FullName;
            return "";
        }
    }
}