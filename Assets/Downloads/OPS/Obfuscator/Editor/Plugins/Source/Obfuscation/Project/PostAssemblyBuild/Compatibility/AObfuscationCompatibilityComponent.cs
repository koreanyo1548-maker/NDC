using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Project
using OPS.Editor.Project.Component;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Project;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility
{
    public abstract class AObfuscationCompatibilityComponent: IObfuscationCompatibilityComponent
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the component.
        /// </summary>
        public abstract String Name { get; }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public abstract String Description { get; }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public abstract String ShortDescription { get; }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public abstract String SettingsKey { get; }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// The ObfuscatorCategory of a CompatibilityComponent is always Compatibility.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Compatibility;
            }
        }        

        /// <summary>
        /// Shown gui in the obfuscator settings window.
        /// </summary>
        public abstract ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings);

        #endregion

        // ToString
        #region ToString

        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}
