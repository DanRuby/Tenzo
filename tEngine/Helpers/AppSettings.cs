using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using tEngine.TMeter;

namespace tEngine.Helpers
{
    /// <summary>
    /// Д: Класс который сохраняет и загружает настройки проектов
    /// </summary>
    public class AppSettings
    {
       /* public enum Project
        {
            Empty,
            Meter,
            Actual
        }*/

        //private static readonly Dictionary<Project, Type> ConstantsClass = new Dictionary<Project, Type>();
        private static Constants mConstants = null;
        private static Dictionary<string, string> Settings = new Dictionary<string, string>();
        private static bool WasOpen = false;

        public static Constants Constants => mConstants;

        public static T GetValue<T>(string key, T defValue = default(T))
        {
            try
            {
                if (mConstants == null)
                    return defValue;
                if (Settings.ContainsKey(key))
                {
                    return JsonConvert.DeserializeObject<T>(Settings[key]);
                }
                return defValue;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                return defValue;
            }
        }

        public static void Init()
        {
            //Type tp = ConstantsClass.ContainsKey(project) ? ConstantsClass[project] : typeof(CommonConstants);
            mConstants = new Constants();//Activator.CreateInstance(tp) as BConstants;
            Open();
        }

        public static void Open()
        {
            if (WasOpen == false)
            {
                try
                {
                    string jsonText;
                    bool result = FileIO.ReadText(mConstants.AppSettings, out jsonText);
                    if (result)
                    {
                        Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
                    }
                    else
                        Settings = new Dictionary<string, string>();
                }
                catch (Exception)
                {
                    Settings = new Dictionary<string, string>();
                }
            }
            WasOpen = true;
        }

        public static void RemoveSet(string key)
        {
            if (Settings.ContainsKey(key))
                Settings.Remove(key);
        }

        public static void Save()
        {
            if (mConstants == null)
                return;
            JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new JSONContractResolver() };
            string jsonString = JsonConvert.SerializeObject(Settings, settings);
            FileIO.WriteText(mConstants.AppSettings, jsonString);
            WasOpen = false;
        }

        public static void SetValue<T>(string key, T value)
        {
            string jsonString = JsonConvert.SerializeObject(value);
            if (!Settings.ContainsKey(key))
                Settings.Add(key, "");
            Settings[key] = jsonString;
        }

        static AppSettings()
        {
            //ConstantsClass.Add(Project.Meter, typeof(TMeter.Constants));
            //ConstantsClass.Add(Project.Actual, typeof(TActual.Constants));
        }
    }
}