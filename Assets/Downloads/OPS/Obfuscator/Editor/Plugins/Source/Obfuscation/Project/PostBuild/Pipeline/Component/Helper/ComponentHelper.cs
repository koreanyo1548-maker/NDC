using System;
using System.Collections.Generic;
using System.Linq;

namespace OPS.Obfuscator.Editor.Project.PostBuild.Pipeline.Component.Helper
{
    /// <summary>
    /// Component helper for the post build step.
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Returns all post build components.
        /// </summary>
        /// <returns></returns>
        public static List<IPostBuildComponent> GetComponents()
        {
            List<IPostBuildComponent> var_Components = new List<IPostBuildComponent>();

            // Buildin - Assets
            var_Components.Add(new MonoBehaviourAssetsComponent());

            // User Plugins
            List<IPostBuildPluginComponent> var_UserPluginComponents = GetPreBuildPluginComponents();
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
        private static List<IPostBuildPluginComponent> GetPreBuildPluginComponents()
        {
            // Result
            List<IPostBuildPluginComponent> var_UserPluginComponentList = new List<IPostBuildPluginComponent>();

            // Type of the user plugin component
            Type var_Type = typeof(IPostBuildPluginComponent);

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
                    IPostBuildPluginComponent var_Instance = (IPostBuildPluginComponent)Activator.CreateInstance(var_InheritedType);
                    var_UserPluginComponentList.Add(var_Instance);

                }
                catch(Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not instantiate " + var_InheritedType.ToString() + " as IPostBuildPluginComponent! Error: " + e.ToString());
                }
            }
            

            return var_UserPluginComponentList;
        }
    }
}
