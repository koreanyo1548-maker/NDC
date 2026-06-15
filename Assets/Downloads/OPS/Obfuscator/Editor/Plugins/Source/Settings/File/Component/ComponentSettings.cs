using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OPS.Editor.Settings.File
{
    /// <summary>
    /// Settings for a component.
    /// </summary>
    public class ComponentSettings : ASettings
    {
        public ComponentSettings()
        {
        }

        public ComponentSettings(String _Settings_Name)
        {
            this.Settings_Name = _Settings_Name;
        }

        // Name
        #region Name

        /// <summary>
        /// Name of the ComponentSettings.
        /// </summary>
        public String Settings_Name { get; set; }

        #endregion
    }
}
