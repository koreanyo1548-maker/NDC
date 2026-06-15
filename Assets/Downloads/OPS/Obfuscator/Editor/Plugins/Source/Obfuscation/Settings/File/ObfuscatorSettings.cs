// System
using System;
using System.Collections.Generic;
using System.IO;

// OPS - Json
using OPS.Serialization.Json;
using OPS.Serialization.Json.Attributes;

// OPS - Version
using OPS.Editor.IO.Version;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Obfuscator.Editor.Settings
{
    [JsonObject]
    public class ObfuscatorSettings : ASettings, IVersionAble
    {
        // Global
        #region Global

        // Constants
        public const String Global_Enable_Obfuscation = "Global_Enable_Obfuscation";

        public const String Global_Obfuscator_Version_Value = "5.4.0";

        #endregion

        // Version
        #region Version

        /// <summary>
        /// Obfuscator Settings Version.
        /// </summary>
        [JsonProperty(Name = "Version")]
        public string Version { get; private set; }

        public bool LoadFromVersion<TVersionAble>(TVersionAble _VersionAble) 
            where TVersionAble : IVersionAble
        {
            throw new NotImplementedException();
        }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// All Component Settings.
        /// </summary>
        [JsonProperty(Name = "ComponentSettings_Array")]
        private ComponentSettings[] componentSettings_Array = new ComponentSettings[0];

        /// <summary>
        /// Get or Create and Get the ComponentSettings with _Settings_Name.
        /// </summary>
        /// <param name="_ComponentSettings_Name"></param>
        /// <returns></returns>
        public ComponentSettings Get_Or_Create_ComponentSettings(String _ComponentSettings_Name)
        {
            // Check if exists!
            for(int i = 0; i < componentSettings_Array.Length; i++)
            {
                if(componentSettings_Array[i].Settings_Name == _ComponentSettings_Name)
                {
                    return componentSettings_Array[i];
                }
            }

            // Add new settings!
            List<ComponentSettings> var_ComponentSettings_List = new List<ComponentSettings>(this.componentSettings_Array);
            ComponentSettings var_New_ComponentSettings = new ComponentSettings(_ComponentSettings_Name);
            var_ComponentSettings_List.Add(var_New_ComponentSettings);
            this.componentSettings_Array = var_ComponentSettings_List.ToArray();

            return var_New_ComponentSettings;
        }

        /// <summary>
        /// Return _Setting as boolean from ComponentSettings with name _ComponentSettings_Name.
        /// </summary>
        /// <param name="_ComponentSettings_Name"></param>
        /// <param name="_Setting"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public bool Get_ComponentSettings_As_Bool(String _ComponentSettings_Name, String _Setting, bool _Default = false)
        {
            ComponentSettings var_ComponentSettings = this.Get_Or_Create_ComponentSettings(_ComponentSettings_Name);
            return var_ComponentSettings.Get_Setting_AsBool(_Setting, _Default);
        }

        /// <summary>
        /// Return _Setting as string from ComponentSettings with name _ComponentSettings_Name.
        /// </summary>
        /// <param name="_ComponentSettings_Name"></param>
        /// <param name="_Setting"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public String Get_ComponentSettings_As_String(String _ComponentSettings_Name, String _Setting, String _Default = "")
        {
            ComponentSettings var_ComponentSettings = this.Get_Or_Create_ComponentSettings(_ComponentSettings_Name);
            return var_ComponentSettings.Get_Setting_AsString(_Setting, _Default);
        }

        /// <summary>
        /// Return _Setting as array from ComponentSettings with name _ComponentSettings_Name.
        /// </summary>
        /// <param name="_ComponentSettings_Name"></param>
        /// <param name="_Setting"></param>
        /// <returns></returns>
        public String[] Get_ComponentSettings_As_Array(String _ComponentSettings_Name, String _Setting)
        {
            ComponentSettings var_ComponentSettings = this.Get_Or_Create_ComponentSettings(_ComponentSettings_Name);
            return var_ComponentSettings.Get_Setting_AsArray(_Setting);
        }

        #endregion

        // Assembly Specific
        #region Assembly Specific

        #endregion

        //Save and Load
        #region Save and Load

        /// <summary>
        /// Returns the file path of the setting file.
        /// </summary>
        /// <returns></returns>
        private static String GetSettingsFilePath()
        {
            if(!Directory.Exists(OPS.Obfuscator.Editor.IO.PathHelper.Get_Obfuscator_Settings_Directory()))
            {
                Directory.CreateDirectory(OPS.Obfuscator.Editor.IO.PathHelper.Get_Obfuscator_Settings_Directory());
            }

            return System.IO.Path.Combine(OPS.Obfuscator.Editor.IO.PathHelper.Get_Obfuscator_Settings_Directory(), "Obfuscator_Settings.json");
        }

        /// <summary>
        /// Stores the settings to a file.
        /// </summary>
        public void Save()
        {
            // Set current version
            this.Version = Global_Obfuscator_Version_Value;

            // Save as json.
            String var_Json = OPS.Serialization.Json.JsonSerializer.ParseObjectToJson(this, OPS.Serialization.Json.EJsonTextMode.Indent);

            using (FileStream var_Stream = new FileStream(GetSettingsFilePath(), FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter var_Writer = new StreamWriter(var_Stream))
                {
                    var_Writer.Write(var_Json);
                }
            }
        }

        /// <summary>
        /// Loads the settings from a file.
        /// If the files does not exists, returns an empty default setting.
        /// </summary>
        public static ObfuscatorSettings Load()
        {
            // If not file, return empty / default.
            if (!File.Exists(GetSettingsFilePath()))
            {
                return new ObfuscatorSettings();
            }

            // Try to load, else return empty / default.
            try
            {
                Dictionary<String, String> var_LoadDictionary = new Dictionary<string, string>();

                using (FileStream var_Stream = new FileStream(GetSettingsFilePath(), FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader var_Reader = new StreamReader(var_Stream))
                    {
                        String var_Json = var_Reader.ReadToEnd();

                        return OPS.Serialization.Json.JsonSerializer.ParseStringToObject<ObfuscatorSettings>(var_Json);
                    }
                }
            }
            catch(Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                return new ObfuscatorSettings();
            }
        }

        #endregion
    }
}
