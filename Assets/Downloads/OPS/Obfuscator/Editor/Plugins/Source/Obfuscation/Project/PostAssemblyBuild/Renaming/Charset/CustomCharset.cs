using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    public class CustomCharset : ACharset
    {
        private String characters;

        public CustomCharset(String _Characters)
        {
            this.characters = _Characters;
        }

        public override String Characters
        {
            get
            {
                return this.characters;
            }
        }
    }
}
