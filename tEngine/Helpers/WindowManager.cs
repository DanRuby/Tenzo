using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace tEngine.Helpers
{
    public class WindowManager {
        
        private static Dictionary<Guid, Window> mWindows = new Dictionary<Guid, Window>(); 

        public static void CloseAll() {
            var windows = mWindows.ToArray();
            foreach( var item in windows ) {
                var window = item.Value;
                if(window != null)
                Task.Factory.StartNew(
                    () => {
                        window.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
                            new Action( () => { window.Close(); } ) );
                    } );
            }
        }

        public static IEnumerable<Window> GetOpenWindows<T>() {
            return mWindows.Where( item => item.Value.GetType().Name.Equals( typeof( T ).Name ) ).Select( item => item.Value );
        }

        public static T NewWindow<T>() where T : Window, new() {
            
            return NewWindow<T>(Guid.NewGuid());
        }
        public static T NewWindow<T>(int id) where T : Window, new() {
            return NewWindow<T>(new Guid(id, 0,0, new byte[8]));
        }

        public static T NewWindow<T>(Guid id) where T : Window, new() {
            T result = new T();
            result.Closed += ( sender, args ) => mWindows.Remove( id );
            mWindows.Add( id, result );
            return result;
        }

        public static T GetWindow<T>( int id ) where T : Window {
            var guid = new Guid( id, 0, 0, new byte[8] );
            return mWindows.ContainsKey( guid ) ? (T)mWindows[guid] : null;
        }        
        public static T GetWindow<T>( Guid id ) where T : Window {
            return mWindows.ContainsKey( id ) ? (T)mWindows[id] : null;
        }


        public static void SaveWindowPos( string key, Window wnd ) {
            var wndCount = mWindows.Count( w => w.GetType().Name.Equals( key ) );
            if( wndCount > 1 )
                return; // если такое же окно еще будет закрыто
            var ws = wnd.WindowState;
            if( ws == WindowState.Normal ) {
                var rect = new Rect( (uint) wnd.Left, (uint) wnd.Top, (uint) wnd.ActualWidth, (uint) wnd.ActualHeight );
                AppSettings.SetValue( key, rect );
            }
            AppSettings.SetValue( key + "_WS", ws );
        }

        public static void UpdateWindowPos( string key, Window wnd ) {
            var rect = AppSettings.GetValue( key, new Rect( 100, 100, wnd.MinHeight, wnd.MinWidth ) );
            if( mWindows.Any( w => w.GetType().Name.Equals( key ) ) ) {
                rect.X += 10;
                rect.Y += 10;
            }

            var maxW = SystemParameters.FullPrimaryScreenWidth;
            var maxH = SystemParameters.FullPrimaryScreenHeight;
            if( rect.Width > maxW ) rect.Width = maxW;
            if( rect.Height > maxH ) rect.Height = maxH;
            if( rect.Right > maxW || rect.Bottom > maxH ) {
                rect.X = 0;
                rect.Y = 0;
            }

            var ws = AppSettings.GetValue( key + "_WS", WindowState.Normal );
            wnd.Left = rect.Left;
            wnd.Top = rect.Top;
            if( wnd.ResizeMode != ResizeMode.CanMinimize && wnd.ResizeMode != ResizeMode.NoResize ) {
                wnd.Height = rect.Height > 100 ? rect.Height : 100;
                wnd.Width = rect.Width > 100 ? rect.Width : 100;
            }
            wnd.WindowState = ws;

            AppSettings.SetValue( key, rect );
        }
    }
}