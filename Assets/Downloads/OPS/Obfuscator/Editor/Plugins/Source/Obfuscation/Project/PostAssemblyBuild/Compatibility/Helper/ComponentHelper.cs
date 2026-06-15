using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Helper
{
    /// <summary>
    /// Helper class for compatibility components.
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Returns a list of all obfuscator compatibility components.
        /// </summary>
        /// <returns></returns>
        public static List<IObfuscationCompatibilityComponent> GetObfuscationCompatibilityComponentList()
        {
            List<IObfuscationCompatibilityComponent> var_ComponentList = new List<IObfuscationCompatibilityComponent>();

            // Buildin - DotNet
            var_ComponentList.Add(new DotNetCompatibilityComponent());

            // Buildin - Unity
            var_ComponentList.Add(new UnityCompatibilityComponent());

            // Buildin - Obfuscator
            var_ComponentList.Add(new ObfuscatorCompatibilityComponent());

            // Buildin - Extern
            var_ComponentList.Add(new ChartboostCompatibilityComponent());
            var_ComponentList.Add(new MetaCompatibilityComponent());
            var_ComponentList.Add(new GoogleSdkCompatibilityComponent());
            var_ComponentList.Add(new JsonSdkCompatibilityComponent());
            var_ComponentList.Add(new PhotonSdkCompatibilityComponent());
            var_ComponentList.Add(new PlayMakerCompatibilityComponent());
            var_ComponentList.Add(new MicrosoftPlayFabSdkCompatibilityComponent());
            var_ComponentList.Add(new RealmsCompatibilityComponent());

            // User Plugins
            List<IObfuscationCompatibilityPluginComponent> var_UserPluginComponents = GetObfuscationCompatibilityPluginComponents();
            for (int c = 0; c < var_UserPluginComponents.Count; c++)
            {
                var_ComponentList.Add(var_UserPluginComponents[c]);
            }

            return var_ComponentList;
        }

        /// <summary>
        /// Returns a list of all user plugins.
        /// </summary>
        /// <returns></returns>
        private static List<IObfuscationCompatibilityPluginComponent> GetObfuscationCompatibilityPluginComponents()
        {
            // Result
            List<IObfuscationCompatibilityPluginComponent> var_UserPluginComponentList = new List<IObfuscationCompatibilityPluginComponent>();

            // Type of the user plugin component
            Type var_Type = typeof(IObfuscationCompatibilityPluginComponent);

            // Get all types that inherite from var_Type and are neither interfaces nor abstract classes.
            var var_AllTypes = OPS.Obfuscator.Editor.Assembly.DotNet.Helper.AssemblyHelper.GetLoadableTypes().Where(t => var_Type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            // Instantiate each and add to result list.
            foreach (Type var_InheritedType in var_AllTypes)
            {
                if (var_InheritedType == null)
                {
                    continue;
                }

                try
                {
                    IObfuscationCompatibilityPluginComponent var_Instance = (IObfuscationCompatibilityPluginComponent)Activator.CreateInstance(var_InheritedType);
                    var_UserPluginComponentList.Add(var_Instance);

                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not instantiate " + var_InheritedType.ToString() + " as IObfuscationCompatibilityPluginComponent! Error: " + e.ToString());
                }
            }

            return var_UserPluginComponentList;
        }
    }
}
