using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Component.Gui.Helper
{
    public static class GuiComponentHelper
    {
        public static List<IGuiComponent> GetGuiComponents()
        {
            List<IGuiComponent> var_Components = new List<IGuiComponent>();

            // Pre Build
            var_Components.AddRange(OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component.Helper.ComponentHelper.GetComponents().Where(c => c is IGuiComponent).Cast<IGuiComponent>());

            // Post Assembly Build
            var_Components.AddRange(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Helper.ComponentHelper.GetObfuscationCompatibilityComponentList().Where(c => c is IGuiComponent).Cast<IGuiComponent>());
            var_Components.AddRange(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Helper.ComponentHelper.GetObfuscatorPipelineComponentList().Where(c => c is IGuiComponent).Cast<IGuiComponent>());

            return var_Components;
        }
    }
}
