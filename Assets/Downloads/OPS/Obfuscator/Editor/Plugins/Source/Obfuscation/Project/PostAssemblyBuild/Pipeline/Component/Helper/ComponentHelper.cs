using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Helper
{
    /// <summary>
    /// Component helper for the post assembly build step.
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Returns all post assembly build components.
        /// </summary>
        /// <returns></returns>
        public static List<IPostAssemblyBuildComponent> GetObfuscatorPipelineComponentList()
        {
            List<IPostAssemblyBuildComponent> var_ComponentList = new List<IPostAssemblyBuildComponent>();

            // Buildin - Assembly
            var_ComponentList.Add(new Assembly.AssemblyObfuscationComponent());

            // Buildin - Compatibility 1
            var_ComponentList.Add(new Compatibility.AnimationComponent());
            var_ComponentList.Add(new Compatibility.UnityEventComponent());
            var_ComponentList.Add(new Compatibility.StringReflectionComponent());

            // Buildin - Debug 1
            var_ComponentList.Add(new Debug.LoggingComponent());

            // Buildin - Member
            var_ComponentList.Add(new Member.Type.NamespaceObfuscationComponent());
            var_ComponentList.Add(new Member.Type.TypeObfuscationComponent());
            var_ComponentList.Add(new Member.Type.TypeCacheReplaceComponent());

            var_ComponentList.Add(new Member.Type.MonoBehaviourRenameObfuscationComponent());
            var_ComponentList.Add(new Member.Type.ScriptableObjectObfuscationComponent());
            var_ComponentList.Add(new Member.Type.PlayableObfuscationComponent());

            var_ComponentList.Add(new Member.Method.MethodObfuscationComponent());
            var_ComponentList.Add(new Member.Method.ParameterObfuscationComponent());

            var_ComponentList.Add(new Member.Field.FieldObfuscationComponent());

            var_ComponentList.Add(new Member.Property.PropertyObfuscationComponent());

            var_ComponentList.Add(new Member.Event.EventObfuscationComponent());

            // Buildin - Optional
            var_ComponentList.Add(new Optional.CustomAttributeComponent());
            var_ComponentList.Add(new Optional.RenamingComponent());

            // Buildin - Security

#if Obfuscator_Use_Beta
            var_ComponentList.Add(new Security.JunkMethodComponent());
#endif

            var_ComponentList.Add(new Security.CloneMethodComponent());
            var_ComponentList.Add(new Security.Method.ControlFlow.ControlFlowComponent());
            var_ComponentList.Add(new Security.StringObfuscationComponent());
            var_ComponentList.Add(new Security.SuppressIldasmAttributeComponent());
            var_ComponentList.Add(new Security.AntiTamperingObfuscationComponent());

            // Buildin - Debug 2
            var_ComponentList.Add(new Debug.NotObfuscateCauseComponent());
            var_ComponentList.Add(new Debug.RemoveObfuscatorAttributesComponent());

            // Buildin - Compatibility 2
            var_ComponentList.Add(new Compatibility.AddressableComponent());

            // User Plugins
            List<IPostAssemblyBuildPluginComponent> var_UserPluginComponents = GetPostAssemblyBuildPluginComponents();
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
        private static List<IPostAssemblyBuildPluginComponent> GetPostAssemblyBuildPluginComponents()
        {
            // Result
            List<IPostAssemblyBuildPluginComponent> var_UserPluginComponentList = new List<IPostAssemblyBuildPluginComponent>();

            // Type of the user plugin component
            Type var_Type = typeof(IPostAssemblyBuildPluginComponent);

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
                    IPostAssemblyBuildPluginComponent var_Instance = (IPostAssemblyBuildPluginComponent)Activator.CreateInstance(var_InheritedType);
                    var_UserPluginComponentList.Add(var_Instance);

                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not instantiate " + var_InheritedType.ToString() + " as IPostAssemblyBuildPluginComponent! Error: " + e.ToString());
                }
            }

            return var_UserPluginComponentList;
        }
    }
}
