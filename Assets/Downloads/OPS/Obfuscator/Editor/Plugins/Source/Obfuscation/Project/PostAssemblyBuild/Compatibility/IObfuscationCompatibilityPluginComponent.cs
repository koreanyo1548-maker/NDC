using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project.Component;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Component.Gui;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility
{
    /// <summary>
    /// A component independent from the pipeline process. Allows custom compatibility to plugins / assets.
    /// All compatibility component are called in the belonging member obfuscation component.
    /// </summary>
    public interface IObfuscationCompatibilityPluginComponent : IObfuscationCompatibilityComponent
    {
    }
}
