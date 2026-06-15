using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEditor;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key.Helper;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug
{
    public class RemoveObfuscatorAttributesComponent : APostAssemblyBuildComponent, IAssemblyProcessingComponent
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Remove Obfuscator References";
            }
        }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Removes Obfuscator references in the build.";
            }
        }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Removes OPS.Obfuscator.Attributes in the build.";
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

        // Analyse B
        #region Analyse B

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Remove Obfuscator Attributes from _AssemblyInfo on release build.
            this.OnPostObfuscate_RemoveObfuscatorAttributes(_AssemblyInfo);

            // Remove Obfuscator Reference from _AssemblyInfo on release build.
            this.OnPostObfuscate_RemoveObfuscatorReference(_AssemblyInfo);

            // Remove Obfuscator Editor Reference from _AssemblyInfo on development and release build.
            this.OnPostObfuscate_RemoveObfuscatorEditorReference(_AssemblyInfo);

            return true;
        }

        private bool OnPostObfuscate_RemoveObfuscatorAttributes(AssemblyInfo _AssemblyInfo)
        {
            // If development build, skip the removal of the obfuscator attributes.
            if (this.Step.BuildSettings.IsDevelopmentBuild)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip remove obfuscator attributes ...");

                return true;
            }

            // On release build, remove the obfuscator attributes.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Remove obfuscator attributes ...");

            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                this.OnPostObfuscate_RemoveObfuscatorAttributes(var_TypeDefinition);

                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    this.OnPostObfuscate_RemoveObfuscatorAttributes(var_MethodDefinition);
                }

                foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                {
                    this.OnPostObfuscate_RemoveObfuscatorAttributes(var_FieldDefinition);
                }

                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    this.OnPostObfuscate_RemoveObfuscatorAttributes(var_PropertyDefinition);
                }

                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    this.OnPostObfuscate_RemoveObfuscatorAttributes(var_EventDefinition);
                }
            }

            return true;
        }

        private void OnPostObfuscate_RemoveObfuscatorAttributes(ICustomAttributeProvider _AttributeProvider)
        {
            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.DoNotObfuscateClassAttribute).FullName);

            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.DoNotUseClassForFakeCodeAttribute).FullName);

            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.DoNotObfuscateMethodBodyAttribute).FullName);

            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).FullName);

            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.NotObfuscatedCauseAttribute).FullName);

            AttributeHelper.RemoveCustomAttributes(_AttributeProvider, typeof(OPS.Obfuscator.Attribute.ObfuscateAnywayAttribute).FullName);
        }

        private bool OnPostObfuscate_RemoveObfuscatorReference(AssemblyInfo _AssemblyInfo)
        {
            // If development build, skip the removal of the obfuscator reference.
            if (this.Step.BuildSettings.IsDevelopmentBuild)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip remove obfuscator reference ...");

                return true;
            }

            // On release build, remove the obfuscator reference.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Remove obfuscator reference ...");

            // Iterate all assembly references and remove where name equals OPS.Obfuscator.
            for (int i = 0; i < _AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences.Count; i++)
            {
                if (_AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences[i].Name.Equals("OPS.Obfuscator"))
                {
                    _AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences.RemoveAt(i);
                    i -= 1;
                }
            }
            return true;
        }

        private bool OnPostObfuscate_RemoveObfuscatorEditorReference(AssemblyInfo _AssemblyInfo)
        {
            // Always remove the obfuscator editor reference.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Remove obfuscator editor reference ...");

            // Iterate all assembly references and remove where name equals OPS.Obfuscator.Editor.
            for (int i = 0; i < _AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences.Count; i++)
            {
                if (_AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences[i].Name.Equals("OPS.Obfuscator.Editor"))
                {
                    _AssemblyInfo.AssemblyDefinition.MainModule.AssemblyReferences.RemoveAt(i);
                    i -= 1;
                }
            }
            return true;
        }

        #endregion

        // PostProcess
        #region PostProcess

        public override bool PostProcess_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            if(!base.PostProcess_Assemblies(_AssemblyLoadInfo))
            {
                return false;
            }

            // Note: Does not work for IL2CPP...
            // Check if not debug. Else return.
            /*if (UnityEngine.Debug.isDebugBuild)
            {
                return true;
            }

            // Remove it. Returns only once true. Because can only be deleted once.
            if (AssemblyHelper.RemoveAssemblyFromBuild("OPS.Obfuscator.dll"))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Remove obfuscator assembly ...");
            }*/

            return true;
        }

        #endregion
    }
}
