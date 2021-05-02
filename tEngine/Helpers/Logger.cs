using System;
using System.Windows;

namespace tEngine.Helpers
{
    /// <summary>
    /// Выводит месседжбокс с ошибкой
    /// </summary>
    public static class Logger
    {
        public static void ShowException(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}