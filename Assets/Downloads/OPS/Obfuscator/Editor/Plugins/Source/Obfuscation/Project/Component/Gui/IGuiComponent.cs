using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Editor - Project
using OPS.Editor.Project.Component;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

namespace OPS.Obfuscator.Editor.Component.Gui
{
    /// <summary>
    /// Is a component with a gui.
    /// </summary>
    public interface IGuiComponent : IComponent
    {
        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        String SettingsKey { get; }

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// The category in the obfuscator settings window this component should be displayed.
        /// </summary>
        EObfuscatorCategory ObfuscatorCategory { get; }

        /// <summary>
        /// Shown gui in the obfuscator settings window.
        /// </summary>
        /// <param name="_ComponentSettings"></param>
        /// <returns></returns>
        ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings);

        #endregion
    }
}
