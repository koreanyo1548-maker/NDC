using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    public enum ECharset : byte
    {
        Default = 0,

        Unicode = 10,

        Common_Chinese = 20,

        Common_Korean = 30,

        Custom = 90,
    }
}
