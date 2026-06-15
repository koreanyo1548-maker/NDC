using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.IO
{
    /// <summary>
    /// Internal path helper for the obfuscator.
    /// </summary>
    internal static class PathHelper
    {
        // Directory Path
        #region Directory Path

        /// <summary>
        /// Returns the Unity ObfuscationProject Path C:\XYZ\MyUnityProject\
        /// </summary>
        /// <returns></returns>
        public static String Get_Project_Directory()
        {
#if Obfuscator_Standalone
            return "";
#else
            return System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
#endif
        }

        /// <summary>
        /// Returns the full path to the OPS directory.
        /// </summary>
        /// <returns></returns>
        public static String Get_OPS_Directory()
        {
#if Obfuscator_Standalone
            return "";
#else
            return System.IO.Path.Combine(UnityEngine.Application.dataPath, "Downloads", "OPS");
#endif
        }

        /// <summary>
        /// Returns the full path to the OPS Obfuscator directory. 
        /// </summary>
        /// <returns></returns>
        public static String Get_Obfuscator_Directory()
        {
            return System.IO.Path.Combine(Get_OPS_Directory(), "Obfuscator");
        }

        /// <summary>
        /// Returns the full path to the OPS Obfuscator Editor directory. 
        /// </summary>
        /// <returns></returns>
        public static String Get_Obfuscator_Editor_Directory()
        {
            return System.IO.Path.Combine(Get_Obfuscator_Directory(), "Editor");
        }

        /// <summary>
        /// Returns the full path to the OPS Obfuscator Editor Temp directory. 
        /// </summary>
        /// <returns></returns>
        public static String Get_Obfuscator_Editor_Temp_Directory()
        {
            return System.IO.Path.Combine(Get_Obfuscator_Directory(), "Temp");
        }

        /// <summary>
        /// Returns the full path to the OPS Obfuscator Log directory. 
        /// </summary>
        /// <returns></returns>
        public static String Get_Obfuscator_Log_Directory()
        {
            return System.IO.Path.Combine(Get_Obfuscator_Directory(), "Log");
        }

        /// <summary>
        /// Returns the full path to the OPS Obfuscator Settings directory. 
        /// </summary>
        /// <returns></returns>
        public static String Get_Obfuscator_Settings_Directory()
        {
            return System.IO.Path.Combine(Get_Obfuscator_Directory(), "Settings");
        }

#endregion
    }
}
