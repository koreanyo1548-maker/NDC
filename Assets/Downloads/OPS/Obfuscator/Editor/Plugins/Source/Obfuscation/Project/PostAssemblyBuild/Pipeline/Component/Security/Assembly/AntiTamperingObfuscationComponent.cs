using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Project
using OPS.Editor.Project.Step;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Clone;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security
{
    /// <summary>
    /// Obfuscation of MonoBehaviours by renaming. 
    /// </summary>
    public class AntiTamperingObfuscationComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Tampering - Protection";
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
                return "Activating this setting enables protection against assembly manipulation. The application will abored with an BadImageFormatException, if tampering was notified. (Mono build only)";
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
                return "Protection against assembly manipulation.  (Beta)";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_AntiTampering";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CEnable_AntiTampering_Protection = "Enable_AntiTampering_Protection";

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Category this Component belongs too.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Security;
            }
        }

        /// <summary>
        /// Returns the gui container.
        /// </summary>
        /// <param name="_ComponentSettings"></param>
        /// <returns></returns>
        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_AntiTampering_Protection);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = this.Description;
            var_Content.AddRow(var_ExplanationRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, true);

            return var_ObfuscatorContainer;
        }

        #endregion


        // Check Assembly
        #region Check Assembly

        /// <summary>
        /// True: The _AssemblyLoadInfo should be processed by this component.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public override bool CanBeProcessedByComponent(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            if (!base.CanBeProcessedByComponent(_AssemblyLoadInfo))
            {
                return false;
            }

            // Only Mono Support.
            if (this.Step.BuildSettings.IsIL2CPPBuild)
            {
                return false;
            }

            return true;
        }

        #endregion

        // OnAnalyse
        #region OnAnalyse

        /// <summary>
        /// Returns just true.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // On Post Analyse
        #region On Post Analyse

        /// <summary>
        /// Returns just true.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        /// <summary>
        /// Returns just true.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        /// <summary>
        /// Map the unobfuscated MonoBehaviours to the obfuscated MonoBehaviours.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {

#if Obfuscator_Free
#else
            // Check if obfuscation got activated.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_AntiTampering_Protection))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The tampering protection got skipped by your settings!");

                return true;
            }

            // Adds the tampering recognition type and method to _AssemblyInfo. And returns it.
            MethodDefinition var_TamperingRecognitionMethod = this.OnPostObfuscate_AddAntiTamperingRuntime(_AssemblyInfo);

            if(var_TamperingRecognitionMethod == null)
            {
                return true;
            }

            // Add method to all static constructors.
            this.OnPostObfuscate_AddAntiTamperingRuntime_InMethods(_AssemblyInfo, var_TamperingRecognitionMethod);
#endif

            return true;
        }

#if Obfuscator_Free
#else
        /// <summary>
        /// Adds the tampering recognition type and method to _AssemblyInfo.
        /// Returns the Method also.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        private MethodDefinition OnPostObfuscate_AddAntiTamperingRuntime(AssemblyInfo _AssemblyInfo)
        {
            TypeReference var_SystemObjectTypeReference = _AssemblyInfo.AssemblyDefinition.MainModule.TypeSystem.Object;

            // New static class with a method for the runtime tampering recognition.
            TypeDefinition StaticStringObfuscationType = new TypeDefinition(
                "<PrivateImplementationDetails>{" + Guid.NewGuid().ToString().ToUpper() + "}",
                /*Guid.NewGuid().ToString().ToUpper()*/"b",
                TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass,
                var_SystemObjectTypeReference);

            // Find the method reference.
            MethodReference var_InitializeMethodReference = _AssemblyInfo.AssemblyDefinition.MainModule.ImportReference(typeof(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security.AntiTamperingRuntime).GetMethod("Initialize"));

            // Try to resolve.
            if (!var_InitializeMethodReference.TryToResolve(out MethodDefinition var_InitializeMethodDefinition))
            {
                return null;
            }

            // Copy it to the _AssemblyInfo.
            CloneHelper.TryToCloneMethod(_AssemblyInfo.AssemblyDefinition, var_InitializeMethodDefinition, "b", out MethodDefinition _Target);

            // Add method to type.
            StaticStringObfuscationType.Methods.Add(_Target);

            // Add type to assembly.
            _AssemblyInfo.AssemblyDefinition.MainModule.Types.Add(StaticStringObfuscationType);

            return _Target;
        }

        /// <summary>
        /// Adds the _TamperingRecognitionMethod to all static constructors in _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TamperingRecognitionMethod"></param>
        /// <returns></returns>
        private bool OnPostObfuscate_AddAntiTamperingRuntime_InMethods(AssemblyInfo _AssemblyInfo, MethodDefinition _TamperingRecognitionMethod)
        {
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (!var_MethodDefinition.HasBody) continue;
                    if (var_MethodDefinition.IsConstructor && var_MethodDefinition.Name == ".cctor")
                    {
                        var_MethodDefinition.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                        var_MethodDefinition.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, _TamperingRecognitionMethod));
                    }
                }
            }

            return true;
        }
#endif

#endregion

        public override bool PostProcess_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            if (!base.PostProcess_Assemblies(_AssemblyLoadInfo))
            {
                return false;
            }

#if Obfuscator_Free
#else

            // Check if obfuscation got activated.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_AntiTampering_Protection))
            {
                return true;
            }

            String var_FilePath = _AssemblyLoadInfo.FilePath;

            // Get the md5 as byte, of the target.
            byte[] var_Md5bytes = System.Security.Cryptography.MD5.Create().ComputeHash(System.IO.File.ReadAllBytes(var_FilePath));
            // FileStream to edit the file's byte.
            using (var var_Stream = new FileStream(var_FilePath, FileMode.Append))
            {
                // Append md5 in the end.
                var_Stream.Write(var_Md5bytes, 0, var_Md5bytes.Length);
            }
#endif

            return true;
        }
    }
}
