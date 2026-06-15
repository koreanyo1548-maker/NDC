using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;
using OPS.Mono.Cecil.Rocks;

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
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security
{
    public class CloneMethodComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Add Random Code";
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
                return "Activate to add random code to your classes.";
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
                return "Activate to add random code to your classes.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Add_Random_Code";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CEnable_Add_Random_Code = "Enable_Add_Random_Code";

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

        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Add_Random_Code);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Info
            Row_Text var_InfoRow = new Row_Text("");
            var_InfoRow.Notification_Info = "This security component generates random code and/or mixed code based on existing methods.";
            var_Content.AddRow(var_InfoRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, true);

            return var_ObfuscatorContainer;
        }

        #endregion

        // OnAnalyse
        #region OnAnalyse

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // OnPostAnalyse
        #region OnPostAnalyse

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        private System.Random random = new Random();

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
#else
            //Check if string obfuscation got activated.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Add_Random_Code))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The random code generation got skipped by your settings!");

                return true;
            }

            // Clone methods in types and randomize operands.
            this.OnObfuscate_AddRandomCode(_AssemblyInfo);
#endif
            return true;
        }

#if Obfuscator_Free
#else
        /// <summary>
        /// Clone methods in types and randomize operands.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnObfuscate_AddRandomCode(AssemblyInfo _AssemblyInfo)
        {
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only types, that can be used for random code generation.
                if (this.OnObfuscate_Skip_Type_ForRandomCodeGeneration(var_TypeDefinition))
                {
                    continue;
                }

                // Clone methods and randomize operands.
                this.OnObfuscate_Generate_RandomCode(_AssemblyInfo, var_TypeDefinition);
            }
        }

        /// <summary>
        /// True: Skip _TypeDefinition for random code generation.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private bool OnObfuscate_Skip_Type_ForRandomCodeGeneration(TypeDefinition _TypeDefinition)
        {
            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} is not obfuscated, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotObfuscateMethodBodyAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotObfuscateMethodBody Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotUseClassForFakeCodeAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotUseClassForFakeCode Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            if (!_TypeDefinition.HasMethods)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has not method, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Clone methods in _TypeDefinition and randomize instructions.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        private void OnObfuscate_Generate_RandomCode(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            // Find clone able methods!
            List<MethodDefinition> var_CloneAbleMethods = this.OnObfuscate_Helper_GetListOfToCloneMethods(_TypeDefinition);

            // Continue only if has clone able methods.
            if (var_CloneAbleMethods.Count == 0)
            {
                return;
            }

            // Create multiple clones.
            for (int i = 0; i < var_CloneAbleMethods.Count; i++)
            {
                int var_CloneTimes = this.random.Next(0, 5);

                for (int t = 0; t < var_CloneTimes; t++)
                {
                    // Clone method.
                    MethodDefinition var_ClonedMethod = this.OnObfuscate_Helper_CloneMethod(_AssemblyInfo.AssemblyDefinition, var_CloneAbleMethods[i]);
                    if (var_ClonedMethod != null)
                    {
                        // Randomize instructions.
                        this.OnObfuscate_Helper_Randomize_Method_Instructions(var_ClonedMethod);

                        // Add to type.
                        _TypeDefinition.Methods.Add(var_ClonedMethod);
                    }
                }
            }

            // Shuffle methods in type.
            this.OnObfuscate_Helper_ShuffleMethods(_TypeDefinition);
        }

        /// <summary>
        /// Returns a list of all cloneable methods in _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private List<MethodDefinition> OnObfuscate_Helper_GetListOfToCloneMethods(TypeDefinition _TypeDefinition)
        {
            List<MethodDefinition> var_MethodList = new List<MethodDefinition>();

            for (int i = 0; i < _TypeDefinition.Methods.Count; i++)
            {
                // Continue only methods with body.
                if (!_TypeDefinition.Methods[i].HasBody)
                {
                    continue;
                }

                // Continue only if not constructor.
                if (_TypeDefinition.Methods[i].IsConstructor)
                {
                    continue;
                }

                // Continue only if not virtual.
                if (_TypeDefinition.Methods[i].IsVirtual)
                {
                    continue;
                }

                // Continue only if not abstract.
                if (_TypeDefinition.Methods[i].IsAbstract)
                {
                    continue;
                }

                // Continue only if managed code!
                if (_TypeDefinition.Methods[i].IsUnmanaged || _TypeDefinition.Methods[i].IsUnmanagedExport)
                {
                    continue;
                }

                // Continue only if not a special name.
                if (_TypeDefinition.Methods[i].IsSpecialName)
                {
                    continue;
                }

                // Continue only if not extern method.
                if (_TypeDefinition.Methods[i].HasPInvokeInfo)
                {
                    continue;
                }

                // Continue only if has no generic parameters.
                if (_TypeDefinition.Methods[i].HasGenericParameters || _TypeDefinition.Methods[i].IsGenericInstance || _TypeDefinition.Methods[i].ContainsGenericParameter)
                {
                    continue;
                }

                // Continue only if not a property method.
                if (_TypeDefinition.Methods[i].IsGetter || _TypeDefinition.Methods[i].IsSetter)
                {
                    continue;
                }

                // Continue only if not a event method.
                if (_TypeDefinition.Methods[i].IsAddOn || _TypeDefinition.Methods[i].IsRemoveOn)
                {
                    continue;
                }

                // Continue only if has no custom attributes.
                if (_TypeDefinition.Methods[i].HasCustomAttributes)
                {
                    continue;
                }

                // Attribute
                if (AttributeHelper.HasCustomAttribute(_TypeDefinition.Methods[i], "DoNotObfuscateMethodBody"))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotObfuscateMethodBody Attribute, so skip Random Code creation!", _TypeDefinition.Methods[i].GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>())));

                    continue;
                }

                var_MethodList.Add(_TypeDefinition.Methods[i]);
            }

            return var_MethodList;
        }

        /// <summary>
        /// The type contains a method with the name _MethodName.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_MethodName"></param>
        /// <returns></returns>
        private bool OnObfuscate_TypeContainsMethod(TypeDefinition _TypeDefinition, String _MethodName)
        {
            for (int i = 0; i < _TypeDefinition.Methods.Count; i++)
            {
                if (_TypeDefinition.Methods[i].Name == _MethodName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clones a method with random values.
        /// If cannot be cloned or a choosen name already got used, returns null.
        /// </summary>
        /// <param name="_AssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <returns></returns>
        private MethodDefinition OnObfuscate_Helper_CloneMethod(AssemblyDefinition _AssemblyDefinition, MethodDefinition _Source)
        {
            // Random Name Index
            uint var_NameIndex = (uint)this.random.Next(1, 9999);

            // Find name.
            String var_ObfuscatedName = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_NameIndex);

            // Check if the type already contains a method like this!
            if (this.OnObfuscate_TypeContainsMethod(_Source.DeclaringType, var_ObfuscatedName))
            {
                return null;
            }

            // Try to clone method with var_ObfuscatedName.
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Clone.CloneHelper.TryToCloneMethod(_AssemblyDefinition, _Source, var_ObfuscatedName, out MethodDefinition _Target))
            {
                return _Target;
            }

            return null;
        }

        /// <summary>
        /// Randomize Int/Float/Double/String operands.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        private void OnObfuscate_Helper_Randomize_Method_Instructions(MethodDefinition _MethodDefinition)
        {
            // Continue only methods that have a body.
            if (!_MethodDefinition.HasBody)
            {
                return;
            }

            // Iterate all instructions and adjust instruction operands.
            for (int i = 0; i < _MethodDefinition.Body.Instructions.Count; i++)
            {
                // Current iterated Instruction in the source method.
                Instruction var_CurrentInstruction = _MethodDefinition.Body.Instructions[i];
                if (var_CurrentInstruction.Operand == null)
                {
                }
                else
                {
                    //Int/Float/Double/...
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineI)
                    {
                        var_CurrentInstruction.Operand = random.Next(-99, 99);
                    }
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.ShortInlineI)
                    {
                        if (var_CurrentInstruction.OpCode.Code == Code.Ldc_I4_S)
                        {
                            var_CurrentInstruction.Operand = (sbyte)random.Next(-99, 99);
                        }
                        else
                        {
                            var_CurrentInstruction.Operand = (byte)random.Next(0, 99);
                        }
                    }
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineI8)
                    {
                        var_CurrentInstruction.Operand = (long)random.Next(-9999, 9999);
                    }
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.ShortInlineR)
                    {
                        var_CurrentInstruction.Operand = (float)random.Next(-9999, 9999);
                    }
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineR)
                    {
                        var_CurrentInstruction.Operand = (double)random.Next(-9999, 9999);
                    }

                    //String
                    if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineString)
                    {
                        var_CurrentInstruction.Operand = this.RandomString();
                    }
                }
            }
        }

        /// <summary>
        /// Create a random string.
        /// </summary>
        /// <returns></returns>
        private string RandomString()
        {
            byte[] var_ByteBuffer = new byte[32];
            this.random.NextBytes(var_ByteBuffer);
            return System.Convert.ToBase64String(var_ByteBuffer);
        }

        /// <summary>
        /// Shuffle all methods in _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        private void OnObfuscate_Helper_ShuffleMethods(TypeDefinition _TypeDefinition)
        {
            int var_Count = _TypeDefinition.Methods.Count - 1;
            for (int i = 0; i < var_Count; i++)
            {
                int var_MethodId = this.random.Next(0, _TypeDefinition.Methods.Count);
                int var_SwitchMethodId = this.random.Next(0, _TypeDefinition.Methods.Count);
                MethodDefinition var_Temp = _TypeDefinition.Methods[var_SwitchMethodId];

                _TypeDefinition.Methods[var_SwitchMethodId] = _TypeDefinition.Methods[var_MethodId];
                _TypeDefinition.Methods[var_MethodId] = var_Temp;
            }
        }
#endif

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
