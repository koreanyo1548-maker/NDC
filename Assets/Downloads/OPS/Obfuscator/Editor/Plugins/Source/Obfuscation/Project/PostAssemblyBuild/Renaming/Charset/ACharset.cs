using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    public abstract class ACharset
    {
        public ACharset()
        {

        }

        public abstract String Characters { get; }
    }
}
