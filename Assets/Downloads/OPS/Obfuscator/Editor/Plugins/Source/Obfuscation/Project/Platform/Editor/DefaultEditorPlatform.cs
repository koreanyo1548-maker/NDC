using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Platform.Editor
{
    public class DefaultEditorPlatform
    {
        public virtual List<String> AdditionalAssemblyResolvingDirectories()
        {
            return new List<string>();
        }

        public override string ToString()
        {
            return "DefaultEditorPlatform";
        }
    }
}
