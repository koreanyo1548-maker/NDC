using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Platform.Editor
{
    public class WindowsEditorPlatform : DefaultEditorPlatform
    {
        public override List<string> AdditionalAssemblyResolvingDirectories()
        {
            List<String> var_PathList = new List<string>();

            String var_UnityEditorPath = Path.GetDirectoryName(EditorApplication.applicationContentsPath);
            String var_ManagedPath = Path.Combine(var_UnityEditorPath, "Managed");

            var_PathList.Add(var_ManagedPath);

            return var_PathList;
        }

        public override string ToString()
        {
            return "WindowsEditorPlatform";
        }
    }
}
