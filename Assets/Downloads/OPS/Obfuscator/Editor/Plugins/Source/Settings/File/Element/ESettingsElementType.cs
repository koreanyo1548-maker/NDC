using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Settings.File
{
    /// <summary>
    /// Type of the settings element.
    /// </summary>
    public enum ESettingsElementType
    {
        String = 0,
        Bool = 1,
        Number = 2,
        Enum = 3,
        Array_String = 10,
    }
}
