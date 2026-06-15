using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    public class DefaultCharset : ACharset
    {
        public override string Characters
        {
            get
            {
                //Because Editor Platform has a problem with files using the same name. Uppercase / Lowercase is the same!
                return "abcdefghijklmnopqrstuvwxyz";//"AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
            }
        }
    }
}
