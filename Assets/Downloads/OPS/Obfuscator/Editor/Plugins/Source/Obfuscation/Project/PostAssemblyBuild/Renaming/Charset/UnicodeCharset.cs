using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    public class UnicodeCharset : ACharset
    {
        private const string CUnicodeCharacters = "\u00A0\u1680" +
                        "\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u200B\u2010\u2011\u2012\u2013\u2014\u2015" +
                        "\u2022\u2024\u2025\u2027\u2028\u2029\u202A\u202B\u202C\u202D\u202E\u202F" +
                        "\u2032\u2035\u2033\u2036\u203E" +
                        "\u2047\u2048\u2049\u204A\u204B\u204C\u204D\u204E\u204F\u2050\u2051\u2052\u2053\u2054\u2055\u2056\u2057\u2058\u2059" +
                        "\u205A\u205B\u205C\u205D\u205E\u205F\u2060" +
                        "\u2061\u2062\u2063\u2064\u206A\u206B\u206C\u206D\u206E\u206F" +
                        "\u3000";

        private String unicodeCharacters;

        public UnicodeCharset()
        {
            int var_StartPoint = 0;
            int var_Length = 64;

            var chars = new List<char>(var_Length);
            for (int i = var_StartPoint; i < var_StartPoint + var_Length; i++)
            {
                chars.Add((char)CUnicodeCharacters[i]);
            }

            this.unicodeCharacters = new string(chars.ToArray());
        }

        public override string Characters
        {
            get
            {
                return this.unicodeCharacters;
            }
        }
    }
}
