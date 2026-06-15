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
    public class NotObfuscateCauseComponent : APostAssemblyBuildComponent, IAssemblyProcessingComponent
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
                return "Not Obfuscate Cause";
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
                return "Adds a do not obfuscate cause attribute to the members in debug build.";
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
                return "Adds a do not obfuscate cause attribute to the members.";
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
            // Add the NotObfuscatedCauseAttribute to _AssemblyInfo, but only if the build is a development build.
            this.OnPostObfuscate_AddNotObfuscateCause(_AssemblyInfo);

            return true;
        }

        private bool OnPostObfuscate_AddNotObfuscateCause(AssemblyInfo _AssemblyInfo)
        {
            // If the build is not a development build, skip adding the NotObfuscatedCauseAttribute.
            if (!this.Step.BuildSettings.IsDevelopmentBuild)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip add not obfuscated cause ...");

                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Add not obfuscated cause ...");

            MethodReference var_AttributeConstructor = _AssemblyInfo.AssemblyDefinition.MainModule.ImportReference(typeof(OPS.Obfuscator.Attribute.NotObfuscatedCauseAttribute).GetConstructor(new Type[] { typeof(String) }));

            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Process Type.
                List<String> var_TypeNotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Type, var_TypeDefinition);
                if (var_TypeNotObfuscatedCause != null)
                {
                    for (int i = 0; i < var_TypeNotObfuscatedCause.Count; i++)
                    {
                        var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                        {
                            ConstructorArguments =
                            {
                                new CustomAttributeArgument(
                                    _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                    "Class: " + var_TypeNotObfuscatedCause[i])
                            }
                        };

                        var_TypeDefinition.CustomAttributes.Add(var_NewAttribute);
                    }
                }

                //Process Type Namespace.
                List<String> var_NamespaceNotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Namespace, var_TypeDefinition);
                if (var_NamespaceNotObfuscatedCause != null)
                {
                    for (int i = 0; i < var_NamespaceNotObfuscatedCause.Count; i++)
                    {
                        var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                        {
                            ConstructorArguments =
                            {
                                new CustomAttributeArgument(
                                    _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                    "Namespace: " + var_NamespaceNotObfuscatedCause[i])
                            }
                        };

                        var_TypeDefinition.CustomAttributes.Add(var_NewAttribute);
                    }
                }

                //Process Methods
                foreach (var var_Method in var_TypeDefinition.Methods)
                {
                    if (var_Method.IsConstructor || var_Method.Name == ".ctor" || var_Method.Name == ".cctor")
                    {
                        continue;
                    }

                    //Process
                    List<String> var_NotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Method, var_Method);
                    if (var_NotObfuscatedCause != null)
                    {
                        for (int i = 0; i < var_NotObfuscatedCause.Count; i++)
                        {
                            var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                            {
                                ConstructorArguments =
                                {
                                    new CustomAttributeArgument(
                                        _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                        var_NotObfuscatedCause[i])
                                }
                            };

                            var_Method.CustomAttributes.Add(var_NewAttribute);
                        }
                    }
                }

                //Process Fields
                foreach (var var_Field in var_TypeDefinition.Fields)
                {
                    //Process
                    List<String> var_NotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Field, var_Field);
                    if (var_NotObfuscatedCause != null)
                    {
                        for (int i = 0; i < var_NotObfuscatedCause.Count; i++)
                        {
                            var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                            {
                                ConstructorArguments =
                                {
                                    new CustomAttributeArgument(
                                        _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                        var_NotObfuscatedCause[i])
                                }
                            };

                            var_Field.CustomAttributes.Add(var_NewAttribute);
                        }
                    }
                }

                //Process Properties
                foreach (var var_Property in var_TypeDefinition.Properties)
                {
                    //Process
                    List<String> var_NotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Property, var_Property);
                    if (var_NotObfuscatedCause != null)
                    {
                        for (int i = 0; i < var_NotObfuscatedCause.Count; i++)
                        {
                            var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                            {
                                ConstructorArguments =
                            {
                                new CustomAttributeArgument(
                                    _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                    var_NotObfuscatedCause[i])
                            }
                            };

                            var_Property.CustomAttributes.Add(var_NewAttribute);
                        }
                    }
                }

                //Process Events
                foreach (var var_Event in var_TypeDefinition.Events)
                {
                    //Process
                    List<String> var_NotObfuscatedCause = this.DataContainer.RenameManager.GetDoNotObfuscateCause(EMemberType.Event, var_Event);
                    if (var_NotObfuscatedCause != null)
                    {
                        for (int i = 0; i < var_NotObfuscatedCause.Count; i++)
                        {
                            var var_NewAttribute = new CustomAttribute(var_AttributeConstructor)
                            {
                                ConstructorArguments =
                            {
                                new CustomAttributeArgument(
                                    _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.String,
                                    var_NotObfuscatedCause[i])
                            }
                            };

                            var_Event.CustomAttributes.Add(var_NewAttribute);
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
