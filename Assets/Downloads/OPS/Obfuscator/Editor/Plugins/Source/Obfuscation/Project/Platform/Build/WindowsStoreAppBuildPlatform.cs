using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Platform.Build
{
    public class WindowsStoreAppBuildPlatform : DefaultBuildPlatform
    {
        public override List<string> AdditionalAssemblyResolvingDirectories()
        {
            List<string> var_List = new List<string>();
            String var_DataPath = EditorApplication.applicationContentsPath + System.IO.Path.DirectorySeparatorChar;

            String var_FacadePath = var_DataPath + "MonoBleedingEdge" + Path.DirectorySeparatorChar + "lib" + Path.DirectorySeparatorChar + "mono" + Path.DirectorySeparatorChar + "4.5" + Path.DirectorySeparatorChar + "Facades";
            if (System.IO.Directory.Exists(var_FacadePath))
            {
                var_List.Add(var_FacadePath);
            }
            else
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not find path to dependency assemblies at: " + var_FacadePath, true);
            }

            String var_MetroSupportPath_Managed = var_DataPath + "PlaybackEngines" + Path.DirectorySeparatorChar + "MetroSupport" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "UAP";
            if (System.IO.Directory.Exists(var_MetroSupportPath_Managed))
            {
                var_List.Add(var_MetroSupportPath_Managed);
            }
            else
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not find path to dependency assemblies at: " + var_MetroSupportPath_Managed, true);
            }

            String var_MetroSupportPath_IL2CPP = var_DataPath + "PlaybackEngines" + Path.DirectorySeparatorChar + "MetroSupport" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "il2cpp";
            if (System.IO.Directory.Exists(var_MetroSupportPath_IL2CPP))
            {
                var_List.Add(var_MetroSupportPath_IL2CPP);
            }
            else
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not find path to dependency assemblies at: " + var_MetroSupportPath_IL2CPP, true);
            }

            var_List.AddRange(new List<string> { @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Runtime\4.0.20\lib\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Collections\4.0.0\ref\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Collections\4.0.10\lib\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Threading\4.0.0\ref\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Diagnostics.Debug\4.0.0\ref\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Runtime.Extensions\4.0.0\ref\netcore50",
            @"C:\Program Files (x86)\Microsoft SDKs\NETCoreSDK\System.Runtime.Extensions\4.0.0\ref\dotnet" });

            return var_List;
        }

        public override string ToString()
        {
            return "WindowsStoreAppBuildPlatform";
        }
    }
}
