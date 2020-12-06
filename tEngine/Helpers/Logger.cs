using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace tEngine.Helpers {
    public class Logger {
        public static void ShowException( Exception ex ) {
            MessageBox.Show( ex.ToString() );
        }
    }
}