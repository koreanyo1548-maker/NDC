using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Accessibility
{
    public enum EAccessibilityLevel
    {
        Public,
        Protected,
        Protected_And_Internal,
        Protected_Or_Private,
        Internal,
        Private,
    }
}
