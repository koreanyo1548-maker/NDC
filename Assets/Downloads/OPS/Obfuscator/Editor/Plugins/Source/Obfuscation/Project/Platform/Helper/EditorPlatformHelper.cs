using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//OPS
using OPS.Obfuscator.Editor.Platform.Editor;

namespace OPS.Obfuscator.Editor.Platform.Helper
{
    internal class EditorPlatformHelper
    {
        public static EEditorPlatform RunningPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications")
                        & Directory.Exists("/System")
                        & Directory.Exists("/Users")
                        & Directory.Exists("/Volumes"))
                        return EEditorPlatform.Mac;
                    else
                        return EEditorPlatform.Linux;

                case PlatformID.MacOSX:
                    return EEditorPlatform.Mac;

                default:
                    return EEditorPlatform.Windows;
            }
        }
    }
}
