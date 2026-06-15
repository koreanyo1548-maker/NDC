using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset
{
    // 0xAC00 - 0xD7A3 : Hangul Syllables
    // 0x2E80 - 0x2EF3 : CJK Radicals Supplement / CJK - Chinese Japanese Korean
    // 0x2F00 - 0x:2FD5: Kangxi Radicals
    public class KoreanCharset : ACharset
    {
        private String koreanCharacters;

        public KoreanCharset()
        {
            int var_StartPoint = 0xAC00;
            int var_Length = 64;

            var chars = new List<char>(var_Length);
            for (int i = var_StartPoint; i < var_StartPoint + var_Length; i++)
            {
                chars.Add((char)i);
            }

            this.koreanCharacters = new string(chars.ToArray());
        }

        public override string Characters
        {
            get
            {
                return this.koreanCharacters;
            }
        }
    }
}
