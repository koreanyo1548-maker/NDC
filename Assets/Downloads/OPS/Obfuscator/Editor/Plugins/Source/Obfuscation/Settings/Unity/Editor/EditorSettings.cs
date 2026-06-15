using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Settings 
using OPS.Editor.Settings.Unity.Editor;

// OPS - Obfuscator - Build
using OPS.Obfuscator.Editor.Platform.Editor;

namespace OPS.Obfuscator.Editor.Settings.Unity.Editor
{
    /// <summary>
    /// Obfuscator Editor Settings.
    /// </summary>
    public class EditorSettings : AEditorSettings
    {
        /// <summary>
        /// Active editor platform.
        /// </summary>
        internal DefaultEditorPlatform EditorPlatform { get; set; } = new DefaultEditorPlatform();
    }
}
