using System;
using System.Collections.Generic;
using System.Linq;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component.Helper
{
    /// <summary>
    /// Component helper for the pre build step.
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Returns all pre build components.
        /// </summary>
        /// <returns></returns>
        public static List<IPreBuildComponent> GetComponents()
        {
            List<IPreBuildComponent> var_Components = new List<IPreBuildComponent>();

            // Buildin - Assets
            var_Components.Add(new AnalyseAnimationComponent());

            // Buildin - Components
            var_Components.Add(new AnalyseUnityEventComponent());
            // var_Components.Add(new AnalyseMonoScriptComponent()); // Note: Not required at the moment. Requirement for this component was solved with Obfuscator version 5.2.0.

            // User Plugins
            List<IPreBuildPluginComponent> var_UserPluginComponents = GetPreBuildPluginComponents();
            for(int c = 0; c < var_UserPluginComponents.Count; c++)
            {
                var_Components.Add(var_UserPluginComponents[c]);
            }

            return var_Components;
        }

        /// <summary>
        /// Returns a list of all user plugins.
        /// </summary>
        /// <returns></returns>
        private static List<IPreBuildPluginComponent> GetPreBuildPluginComponents()
        {
            // Result
            List<IPreBuildPluginComponent> var_UserPluginComponentList = new List<IPreBuildPluginComponent>();

            // Type of the user plugin component
            Type var_Type = typeof(IPreBuildPluginComponent);

            // Get all types that inherite from var_Type and are neither interfaces nor abstract classes.
            var var_AllTypes = OPS.Obfuscator.Editor.Assembly.DotNet.Helper.AssemblyHelper.GetLoadableTypes().Where(t => var_Type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            // Instantiate each and add to result list.
            foreach(Type var_InheritedType in var_AllTypes)
            {
                if(var_InheritedType == null)
                {
                    continue;
                }

                try
                {
                    IPreBuildPluginComponent var_Instance = (IPreBuildPluginComponent)Activator.CreateInstance(var_InheritedType);
                    var_UserPluginComponentList.Add(var_Instance);

                }
                catch(Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not instantiate " + var_InheritedType.ToString() + " as IPreBuildPluginComponent! Error: " + e.ToString());
                }
            }
            

            return var_UserPluginComponentList;
        }
    }
}
