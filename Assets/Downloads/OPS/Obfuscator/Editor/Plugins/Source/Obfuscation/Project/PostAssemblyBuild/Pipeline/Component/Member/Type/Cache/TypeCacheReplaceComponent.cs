using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type
{
    internal class TypeCacheReplaceComponent : APostAssemblyBuildComponent, IAssemblyProcessingComponent
    {
        //Info
        #region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Type Cache Replacement";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Replaces the types and namespace in the assemblies cache.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Replaces the types and namespace in the assemblies cache.";
            }
        }

        #endregion

        // Analyse A
        #region Analyse A

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Analyse A
        #region Analyse B

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        //Obfuscate
        #region Obfuscate

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Replace Type Namespace and Name Cache...");

            //Replace inside the name cache the type name and namespaces.
            this.Obfuscate_ReplaceNameCache(_AssemblyInfo);

            return true;
        }

        private bool Obfuscate_ReplaceNameCache(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Not for nested. Is not needed?
                if (var_TypeDefinition.IsNested)
                {
                    continue;
                }

                // Original
                TypeKey var_OriginalTypeKey = this.Step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(var_TypeDefinition);

                // Check if obfuscated or set in cache. Else skip.
                if(var_OriginalTypeKey == null)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] While type/namespace replacement in module cache " + var_TypeDefinition.ToString() + " is not in typecache. Skip it.");
                    continue;
                }

                //Old
                String var_Old_Namespace = var_OriginalTypeKey.Namespace;
                String var_Old_Name = var_OriginalTypeKey.Name;

                //New
                String var_New_Namespace = var_Old_Namespace;
                String var_New_Name = var_Old_Name;

                if (!this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition))
                {
                    var_New_Namespace = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition);
                }

                if (!this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                {
                    var_New_Name = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition);
                }

                //Get the module.
                ModuleDefinition var_Module = var_TypeDefinition.Module;

                //Update the collection references.
                TypeDefinitionCollection var_Collection = ((TypeDefinitionCollection)var_Module.Types);
                var_Collection.OnReplace(var_Old_Namespace, var_Old_Name, var_TypeDefinition);

                //TODO: How are generic stored there? As A`2?
            }

            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion
    }
}