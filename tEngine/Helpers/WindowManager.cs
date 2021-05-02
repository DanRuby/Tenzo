using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace tEngine.Helpers
{
    /// <summary>
    /// Класс для работы с окнами приложения
    /// </summary>
    public static class WindowManager
    {

        private static Dictionary<Guid, Window> mWindows = new Dictionary<Guid, Window>();

        /// <summary>
        /// Закрыть все окна программы 
        /// </summary>
        public static void CloseAll()
        {
            KeyValuePair<Guid, Window>[] windows = mWindows.ToArray();
            foreach (KeyValuePair<Guid, Window> item in windows)
            {
                Window window = item.Value;
                if (window != null)
                    Task.Factory.StartNew(
                        () =>
                        {
                            window.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new Action(() => { window.Close(); }));
                        });
            }
        }

        public static IEnumerable<Window> GetOpenWindows<T>() => mWindows.Where(item => item.Value.GetType().Name.Equals(typeof(T).Name)).Select(item => item.Value);
       
        /// <summary>
        /// Создать новое окно типа Т
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T NewWindow<T>() where T : Window, new() => NewWindow<T>(Guid.NewGuid());

        /// <summary>
        /// Создать новое окно типа Т с установленным идентификатором
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T NewWindow<T>(int id) where T : Window, new() => NewWindow<T>(new Guid(id, 0, 0, new byte[8]));

        /// <summary>
        /// Создать новое окно типа Т с установленным идентификатором
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T NewWindow<T>(Guid id) where T : Window, new()
        {
            T result = new T();
            result.Closed += (sender, args) => mWindows.Remove(id);
            mWindows.Add(id, result);
            return result;
        }

        /// <summary>
        /// Создать окно типа Т 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetWindow<T>(int id) where T : Window
        {
            Guid guid = new Guid(id, 0, 0, new byte[8]);
            return mWindows.ContainsKey(guid) ? (T)mWindows[guid] : null;
        }
        public static T GetWindow<T>(Guid id) where T : Window => mWindows.ContainsKey(id) ? (T)mWindows[id] : null;

        /// <summary>
        /// Сохранить габариты и положение окна
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wnd"></param>
        public static void SaveWindowPos(string key, Window wnd)
        {
            int wndCount = mWindows.Count(w => w.GetType().Name.Equals(key));
            if (wndCount > 1)
                return; // если такое же окно еще будет закрыто
            WindowState ws = wnd.WindowState;
            if (ws == WindowState.Normal)
            {
                Rect rect = new Rect((uint)wnd.Left, (uint)wnd.Top, (uint)wnd.ActualWidth, (uint)wnd.ActualHeight);
                AppSettings.SetValue(key, rect);
            }
            AppSettings.SetValue(key + "_WS", ws);
        }

        /// <summary>
        /// Обновить габариты и положение окна
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wnd"></param>
        public static void UpdateWindowPos(string key, Window wnd)
        {
            Rect rect = AppSettings.GetValue(key, new Rect(100, 100, wnd.MinHeight, wnd.MinWidth));
            if (mWindows.Any(w => w.GetType().Name.Equals(key)))
            {
                rect.X += 10;
                rect.Y += 10;
            }

            double maxW = SystemParameters.FullPrimaryScreenWidth;
            double maxH = SystemParameters.FullPrimaryScreenHeight;
            if (rect.Width > maxW) rect.Width = maxW;
            if (rect.Height > maxH) rect.Height = maxH;
            if (rect.Right > maxW || rect.Bottom > maxH)
            {
                rect.X = 0;
                rect.Y = 0;
            }

            WindowState ws = AppSettings.GetValue(key + "_WS", WindowState.Normal);
            wnd.Left = rect.Left;
            wnd.Top = rect.Top;
            if (wnd.ResizeMode != ResizeMode.CanMinimize && wnd.ResizeMode != ResizeMode.NoResize)
            {
                wnd.Height = rect.Height > 100 ? rect.Height : 100;
                wnd.Width = rect.Width > 100 ? rect.Width : 100;
            }
            wnd.WindowState = ws;

            AppSettings.SetValue(key, rect);
        }
    }
}