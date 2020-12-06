﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tEngine.Helpers {
    public static class StringExtetion {
        /// <summary>
        /// ..\path => ..\path\
        /// </summary>
        public static string CorrectSlash( this string path ) {
            if( string.IsNullOrEmpty( path ) ) return "";
            if( !(path.EndsWith( @"\" ) || path.EndsWith( "/" )) )
                path += @"\";
            return path;
        }

        public static string CutFileName( this string path ) {
            if( string.IsNullOrEmpty( path ) ) return "";
            var dinfo = new FileInfo( path ).Directory;
            if( dinfo != null )
                return dinfo.FullName;
            return "";
        }
    }
}