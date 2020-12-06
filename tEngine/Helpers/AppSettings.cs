using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.TMeter;
using tEngine.TActual;


namespace tEngine.Helpers {
    public class AppSettings {
        public enum Project {
            Empty,
            Meter,
            Actual
        }

        private static readonly Dictionary<Project, Type> ConstantsClass = new Dictionary<Project, Type>();
        private static BConstants mConstants = null;
        private static Dictionary<string, string> Settings = new Dictionary<string, string>();
        private static bool WasOpen = false;

        public static BConstants Constants {
            get { return mConstants; }
        }

        public static T GetValue<T>( string key, T defValue = default(T) ) {
            try {
                if( mConstants == null ) return defValue;
                if( Settings.ContainsKey( key ) ) {
                    return JsonConvert.DeserializeObject<T>( Settings[key] );
                }
                return defValue;
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
                return defValue;
            }
        }

        public static void Init( Project project = Project.Empty ) {
            var tp = ConstantsClass.ContainsKey( project ) ? ConstantsClass[project] : typeof( CommonConstants );
            mConstants = Activator.CreateInstance( tp ) as BConstants;
            Open();
        }

        public static void Open() {
            if( WasOpen == false ) {
                try {
                    string json;
                    var result = FileIO.ReadText( mConstants.AppSettings, out json );
                    if( result ) {
                        Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
                    } else
                        Settings = new Dictionary<string, string>();
                } catch( Exception ex ) {
                    Settings = new Dictionary<string, string>();
                }
            }
            WasOpen = true;
        }

        public static void RemoveSet( string key ) {
            if( Settings.ContainsKey( key ) )
                Settings.Remove( key );
        }

        public static void Save() {
            if( mConstants == null ) return;
            var settings = new JsonSerializerSettings() {ContractResolver = new JSONContractResolver()};
            var json = JsonConvert.SerializeObject( Settings, settings );
            FileIO.WriteText( mConstants.AppSettings, json );
            WasOpen = false;
        }

        public static void SetValue<T>( string key, T value ) {
            var json = JsonConvert.SerializeObject( value );
            if( !Settings.ContainsKey( key ) )
                Settings.Add( key, "" );
            Settings[key] = json;
        }

        static AppSettings() {
            ConstantsClass.Add( Project.Meter, typeof( TMeter.Constants ) );
            ConstantsClass.Add( Project.Actual, typeof( TActual.Constants ) );
        }
    }
}