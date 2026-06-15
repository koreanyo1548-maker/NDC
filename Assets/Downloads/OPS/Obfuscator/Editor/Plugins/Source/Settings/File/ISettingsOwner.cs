using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Settings.File
{
    /// <summary>
    /// Add to a class if it is a settings owner.
    /// </summary>
    public interface ISettingsOwner
    {
        /// <summary>
        /// The settings class.
        /// </summary>
        ASettings Settings { get; }
    }
}
