using System;
using System.Windows;

namespace tEngine.Helpers
{
    /// <summary>
    /// Выводит месседжбокс с ошибкой
    /// </summary>
    public class Logger
    {
        public static void ShowException(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}