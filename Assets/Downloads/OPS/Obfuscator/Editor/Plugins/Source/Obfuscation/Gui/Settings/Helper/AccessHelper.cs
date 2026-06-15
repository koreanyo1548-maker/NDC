using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

namespace OPS.Obfuscator.Editor.Gui
{
    public static class AccessHelper
    {
        public static bool IsProActive()
        {
#if Obfuscator_Free
            return false;
#else
            return true;
#endif
        }
    }
}
