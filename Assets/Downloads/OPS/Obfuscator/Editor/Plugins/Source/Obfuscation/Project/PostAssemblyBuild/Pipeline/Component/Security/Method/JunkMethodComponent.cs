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

#if Obfuscator_Use_Beta

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security
{
    public class JunkMethodComponent : AObfuscationPipelineComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Add Random Junk Code";
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
                return "Activate to add junk random code to your classes.";
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
                return "Activate to add junk random code to your classes.";
            }
        }

#endregion

        // Settings
#region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Add_Junk_Random_Code";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CEnable_Add_Junk_Random_Code = "Enable_Add_Junk_Random_Code";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Add_Junk_Random_Code);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Info
            Row_Text var_InfoRow = new Row_Text("");
            var_InfoRow.Notification_Info = "This security component generates junk random code methods to your classes.";
            var_Content.AddRow(var_InfoRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, true);

            return var_ObfuscatorContainer;
        }

#endregion

        // Analyse A
#region Analyse A

        public bool OnAnalyse_A_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

#endregion

        // Analyse B
#region Analyse B

        public bool OnAnalyse_B_Assemblies(AssemblyInfo _AssemblyInfo)
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
            // Check if junk code obfuscation got activated.
            if (!this.Project.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Add_Junk_Random_Code))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The junk random code generation got skipped by your settings!");

                return true;
            }

            // Add random junk code.
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

                // Create junk methods and add.
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
            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotObfuscateMethodBodyAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotObfuscateMethodBody Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Project.GetCache<TypeCache>())));

                return true;
            }

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotUseClassForFakeCodeAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotUseClassForFakeCode Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Project.GetCache<TypeCache>())));

                return true;
            }

            if (_TypeDefinition.IsInterface)
            {
                return true;
            }

            if (_TypeDefinition.IsValueType)
            {
                return true;
            }

            if (!_TypeDefinition.HasMethods)
            {
                //Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has not method, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Project.GetCache<TypeCache>())));

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
            int var_CloneTimes = 1; // this.random.Next(0, 5);

            for (int t = 0; t < var_CloneTimes; t++)
            {
                // Clone method.
                MethodDefinition var_JunkMethod = this.OnObfuscate_Helper_Create_JunkMethod(_AssemblyInfo.AssemblyDefinition, _TypeDefinition);
                if (var_JunkMethod != null)
                {
                    // Add to type.
                    _TypeDefinition.Methods.Add(var_JunkMethod);

                    // Assign the random junk body.
                    MsilBodyGenerator.AssignRandomMethodBody(var_JunkMethod);

                    // Rename parameter.
                    uint pIndex = 0;
                    foreach (ParameterDefinition var_Parameter in var_JunkMethod.Parameters)
                    {
                        var_Parameter.Name = this.DataContainer.RenameManager.NameGenerator.GetNextName(pIndex++);
                    }
                }
            }

            // Shuffle methods in type.
            // this.OnObfuscate_Helper_ShuffleMethods(_TypeDefinition);
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
        /// <param name="_Project"></param>
        /// <param name="_AssemblyDefinition"></param>
        /// <returns></returns>
        private MethodDefinition OnObfuscate_Helper_Create_JunkMethod(AssemblyDefinition _AssemblyDefinition, TypeDefinition _TypeDefinition)
        {
            // Random Name Index
            uint var_NameIndex = (uint)this.random.Next(1, 9999);

            // Find name.
            String var_ObfuscatedName = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_NameIndex);

            // Check if the type already contains a method like this!
            if (this.OnObfuscate_TypeContainsMethod(_TypeDefinition, var_ObfuscatedName))
            {
                return null;
            }

            // Create method attributes.
            MethodAttributes var_MethodAttributes;

            switch (this.random.Next(2))
            {
                case 0:
                    {
                        var_MethodAttributes = MethodAttributes.Private;
                        break;
                    }
                default:
                    {
                        var_MethodAttributes = MethodAttributes.Public;
                        break;
                    }
            }

            if (TypeDefinitionHelper.IsTypeStatic(_TypeDefinition))
            {
                var_MethodAttributes = var_MethodAttributes | MethodAttributes.Static;
            }

            // Create Junk Method.
            MethodDefinition var_JunkMethod = new MethodDefinition(var_ObfuscatedName, var_MethodAttributes, _AssemblyDefinition.MainModule.TypeSystem.Void);

            return var_JunkMethod;
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

        public class MsilBodyGenerator
        {
            // Constructor
#region Constructor

            private MsilBodyGenerator(MethodDefinition _Method)
            {
                // Get methods module.
                this.module = _Method.Module;

                // Get all non static types.
                this.allNonStaticTypes = this.module.Types.Where(t => !TypeDefinitionHelper.IsTypeStatic(t) && !t.IsInterface && !t.HasGenericParameters && t.FullName != "<Module>").ToArray();

                // Get all static types.
                this.allStaticTypes = this.module.Types.Where(t => TypeDefinitionHelper.IsTypeStatic(t) && !t.IsInterface && !t.HasGenericParameters && t.FullName != "<Module>").ToArray();

                // Resolve element types.
                this.bool_ElementTypes = new[]
                {
                    this.module.TypeSystem.Boolean.Resolve(),
                };
                this.char_ElementTypes = new[]
                {
                    this.module.TypeSystem.Char.Resolve(),
                };
                this.string_ElementTypes = new[]
                {
                    this.module.TypeSystem.String.Resolve(),
                };
                this.number_ElementTypes = new[]
                {
                    this.module.TypeSystem.Byte.Resolve(),
                    this.module.TypeSystem.SByte.Resolve(),
                    this.module.TypeSystem.UInt16.Resolve(),
                    this.module.TypeSystem.Int16.Resolve(),
                    this.module.TypeSystem.UInt32.Resolve(),
                    this.module.TypeSystem.Int32.Resolve(),
                    this.module.TypeSystem.UInt64.Resolve(),
                    this.module.TypeSystem.Int64.Resolve(),
                };

                List<TypeDefinition> var_TypeDefinition = new List<TypeDefinition>();

                var_TypeDefinition.AddRange(this.bool_ElementTypes);
                var_TypeDefinition.AddRange(this.char_ElementTypes);
                var_TypeDefinition.AddRange(this.string_ElementTypes);
                var_TypeDefinition.AddRange(this.number_ElementTypes);

                this.elementTypes = var_TypeDefinition.ToArray();

                // Set the method.
                this.method = _Method;

                // Assign method parameter.
                int var_ParameterCount = random.Next(-4, 4);
                if (var_ParameterCount < 0)
                    var_ParameterCount = 0;
                for (int j = 0; j < var_ParameterCount; j++)
                {
                    this.method.Parameters.Add(new ParameterDefinition(this.module.ImportReference(this.GetRandomType(null, false))));
                }

                // Get the methods to the list.
                this.validParameters = _Method.Parameters.ToList();

                // Get Body.
                this.body = _Method.Body = new MethodBody(_Method);
                this.processor = body.GetILProcessor();
            }

#endregion

            // Random
#region Random

            private static readonly Random random = new Random();

#endregion

            // Module
#region Module

            private readonly ModuleDefinition module;

            private readonly TypeDefinition[] allNonStaticTypes;

            private readonly TypeDefinition[] allStaticTypes;

            private readonly TypeDefinition[] bool_ElementTypes;

            private readonly TypeDefinition[] char_ElementTypes;

            private readonly TypeDefinition[] string_ElementTypes;

            private readonly TypeDefinition[] number_ElementTypes;

            private readonly TypeDefinition[] elementTypes;

            private TypeDefinition GetRandomType(MethodBodyBlock _Current_MethodBodyBlock, bool _CanBeStatic)
            {
                // Get a type in module or element type.
                switch (random.Next(4))
                {
                    case 0:
                        {
                            if (_CanBeStatic)
                            {
                                // Can be either static or non static.
                                int var_RandomIndex = random.Next(this.allStaticTypes.Length + this.allNonStaticTypes.Length);

                                if (var_RandomIndex >= this.allStaticTypes.Length)
                                {
                                    // Index is in non static range.
                                    var_RandomIndex = var_RandomIndex - this.allStaticTypes.Length;

                                    if (this.allNonStaticTypes.Length != 0)
                                    {
                                        // There are non static classes, return it.
                                        return this.allNonStaticTypes[var_RandomIndex];
                                    }
                                    else
                                    {
                                        // There are no non static classes!
                                        return this.elementTypes[random.Next(this.elementTypes.Length)];
                                    }
                                }
                                else
                                {
                                    // Is in static range.
                                    return this.allStaticTypes[var_RandomIndex];
                                }
                            }
                            else
                            {
                                if (this.allNonStaticTypes.Length != 0)
                                {
                                    // There are non static classes, return it.
                                    return this.allNonStaticTypes[random.Next(this.allNonStaticTypes.Length)];
                                }
                                else
                                {
                                    // There are no non static classes!
                                    return this.elementTypes[random.Next(this.elementTypes.Length)];
                                }
                            }
                        }
                    case 1:
                        {
                            return this.elementTypes[random.Next(this.elementTypes.Length)];
                        }
                    case 2:
                        {
                            if (_Current_MethodBodyBlock == null || this.validParameters.Count == 0)
                            {
                                return this.elementTypes[random.Next(this.elementTypes.Length)];
                            }
                            else
                            {
                                int var_RandomIndex = random.Next(this.validParameters.Count);

                                return this.validParameters[var_RandomIndex].ParameterType.Resolve();
                            }
                        }
                    default:
                        {
                            if (_Current_MethodBodyBlock == null || _Current_MethodBodyBlock.ValidVariables.Count == 0)
                            {
                                return this.elementTypes[random.Next(this.elementTypes.Length)];
                            }
                            else
                            {
                                int var_RandomIndex = random.Next(_Current_MethodBodyBlock.ValidVariables.Count);

                                return _Current_MethodBodyBlock.ValidVariables[var_RandomIndex].VariableType.Resolve();
                            }
                        }
                }
            }

            private bool IsVoid(TypeReference _Reference)
            {
                return this.module.TypeSystem.Void.Resolve().FullName == _Reference.Resolve().FullName;
            }

            private bool IsBool(TypeReference _Reference)
            {
                return this.module.TypeSystem.Boolean.Resolve().FullName == _Reference.Resolve().FullName;
            }

            private TypeDefinition GetRandomBoolType()
            {
                return this.module.TypeSystem.Boolean.Resolve();
            }

            private bool IsChar(TypeReference _Reference)
            {
                return this.module.TypeSystem.Char.Resolve().FullName == _Reference.Resolve().FullName;
            }

            private TypeDefinition GetRandomCharType()
            {
                return this.module.TypeSystem.Char.Resolve();
            }

            private bool IsString(TypeReference _Reference)
            {
                return this.module.TypeSystem.String.Resolve().FullName == _Reference.Resolve().FullName;
            }

            private TypeDefinition GetRandomStringType()
            {
                return this.module.TypeSystem.String.Resolve();
            }

            private bool IsNumber(TypeReference _Reference)
            {
                return this.module.TypeSystem.Byte.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.SByte.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.UInt16.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.Int16.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.UInt32.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.Int32.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.UInt64.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.Int64.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.Single.Resolve().FullName == _Reference.Resolve().FullName ||
                this.module.TypeSystem.Double.Resolve().FullName == _Reference.Resolve().FullName;
            }

            private TypeDefinition GetRandomNumberType()
            {
                return this.number_ElementTypes[random.Next(this.number_ElementTypes.Length)];
            }

            private bool IsObject(TypeReference _Reference)
            {
                return this.module.TypeSystem.Object.Resolve().FullName == _Reference.Resolve().FullName;
            }

#endregion

            // Method
#region Method

            private readonly MethodDefinition method;

            private readonly List<ParameterDefinition> validParameters;

            public static void AssignRandomMethodBody(MethodDefinition method)
            {
                MsilBodyGenerator var_Generator = new MsilBodyGenerator(method);

                MethodBodyBlock var_MethodBodyBlock = new MethodBodyBlock();

                int var_BlockCount = random.Next(CMinBlocks, CMaxBlocks);
                for (int i = 0; i < var_BlockCount; i++)
                {
                    var_Generator.AppendBlock(var_MethodBodyBlock);
                }

                var_Generator.FinalizeMethod();
            }

#endregion

            // Method Body
#region Method Body

            /// <summary>
            /// Max blocks deep. Means how many try-catch/loops/if/... can be stacked in each other.
            /// </summary>
            private const int CMaxBlockDepth = 2;

            /// <summary>
            /// Min blocks per method body.
            /// </summary>
            private const int CMinBlocks = 2;

            /// <summary>
            /// Max blocks per method body.
            /// </summary>
            private const int CMaxBlocks = 10;

            /// <summary>
            /// The Methods body itself.
            /// </summary>
            private readonly MethodBody body;

            /// <summary>
            /// Methods body processor.
            /// </summary>
            private readonly ILProcessor processor;

            /// <summary>
            /// Current iterated blocks deep.
            /// </summary>
            private int blockDepth = 0;

            private class MethodBodyBlock
            {
                public int BlockDepth;

                public List<VariableDefinition> ValidVariables;

                public MethodBodyBlock()
                {
                    this.BlockDepth = 1;
                    this.ValidVariables = new List<VariableDefinition>();
                }

                public MethodBodyBlock(MethodBodyBlock _Top_MethodBodyBlock)
                {
                    this.BlockDepth = (_Top_MethodBodyBlock == null ? 0 : _Top_MethodBodyBlock.BlockDepth) + 1;
                    this.ValidVariables = (_Top_MethodBodyBlock == null ? new List<VariableDefinition>() : new List<VariableDefinition>(_Top_MethodBodyBlock.ValidVariables));
                }
            }

            /// <summary>
            /// Appends a block to method with top context _Top_MethodBodyBlock.
            /// </summary>
            /// <param name="_Current_MethodBodyBlock"></param>
            private void AppendBlock(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Global block depth.
                this.blockDepth += 1;

                // If max block depth, iterate down and create meanwhile simple blocks.
                if (this.blockDepth >= CMaxBlockDepth)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        AppendSimpleBlock(_Current_MethodBodyBlock);
                    }
                    this.blockDepth -= 1;
                    return;
                }

                // Create simple block or complex block.
                switch (random.Next(8))
                {
                    case 0:
                    case 1:
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                this.AppendSimpleBlock(_Current_MethodBodyBlock);
                            }
                            break;
                        }
                    case 2:
                    case 3:
                        {
                            this.AppendIfStatement(new MethodBodyBlock(_Current_MethodBodyBlock));
                            break;
                        }
                    case 4:
                        {
                            this.AppendDoLoopStatement(new MethodBodyBlock(_Current_MethodBodyBlock));
                            break;
                        }
                    case 5:
                    case 6:
                        {
                            this.AppendForLoopStatement(new MethodBodyBlock(_Current_MethodBodyBlock));
                            break;
                        }
                    default:
                        {
                            this.AppendTryStatement(new MethodBodyBlock(_Current_MethodBodyBlock));
                            break;
                        }
                }

                // Global block depth.
                this.blockDepth -= 1;
            }

            /// <summary>
            /// Append a variable init or a void method call.
            /// </summary>
            private void AppendSimpleBlock(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // How many statements in the block.
                var statementCount = random.Next(-8, 8);
                if (statementCount <= 0)
                {
                    statementCount = 1;
                }

                // Add statement. Either assign or invoke.
                for (int i = 0; i < statementCount; i++)
                {
                    switch (random.Next(5))
                    {
                        case 0:
                            {
                                // Create variable.
                                this.AppendAssignmentStatement(_Current_MethodBodyBlock);
                                break;
                            }
                        default:
                            {
                                // Create a void method.
                                this.AppendInvocationStatement(_Current_MethodBodyBlock);
                                break;
                            }
                    }
                }
            }

            /// <summary>
            /// Append a variable init.
            /// </summary>
            private void AppendAssignmentStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Get and import a random type.
                TypeDefinition var_TypeDefinition = this.GetRandomType(_Current_MethodBodyBlock, false);

                // Create Variable of type!
                VariableDefinition var_VariableDefinition = this.CreateVariable(var_TypeDefinition);

                // Append value.
                this.AppendCreateNewValue(_Current_MethodBodyBlock, var_TypeDefinition);

                // Assign to variable.
                this.AppendAssignVariable(var_VariableDefinition);

                // Append to _Current_MethodBodyBlock valid variables.
                _Current_MethodBodyBlock.ValidVariables.Add(var_VariableDefinition);
            }

            private VariableDefinition CreateVariable(TypeReference _Variable_TypeReference)
            {
                // Check if there is already a variable of this type.
                VariableDefinition var_VariableDefinition = new VariableDefinition(this.module.ImportReference(_Variable_TypeReference));

                // Append Variable to
                this.body.Variables.Add(var_VariableDefinition);

                return var_VariableDefinition;
            }

            private bool TryGetVariable(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _VariableType, out VariableDefinition _VariableDefinition)
            {
                // Check if there is already a variable of this type.
                VariableDefinition var_Variable = _Current_MethodBodyBlock.ValidVariables.FirstOrDefault(x => x.VariableType.Resolve().FullName == _VariableType.Resolve().FullName);

                if (var_Variable != null)
                {
                    _VariableDefinition = var_Variable;
                    return true;
                }

                _VariableDefinition = null;
                return false;
            }

            private bool TryAppendLoadVariable(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _ValueType)
            {
                if (this.TryGetVariable(_Current_MethodBodyBlock, _ValueType, out VariableDefinition var_VariableDefinition))
                {
                    this.processor.Emit(OpCodes.Ldloc, var_VariableDefinition);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void AppendAssignVariable(VariableDefinition _VariableDefinition)
            {
                this.processor.Emit(OpCodes.Stloc, _VariableDefinition);
            }

            private bool TryGetParameter(TypeReference _ParameterType, out ParameterDefinition _ParameterDefinition)
            {
                // Check if there is already a variable of this type.
                ParameterDefinition var_Parameter = this.validParameters.FirstOrDefault(x => x.ParameterType.Resolve().FullName == _ParameterType.Resolve().FullName);

                if (var_Parameter != null)
                {
                    _ParameterDefinition = var_Parameter;
                    return true;
                }

                _ParameterDefinition = null;
                return false;
            }

            private bool TryAppendLoadParameter(TypeReference _ValueType)
            {
                if (this.TryGetParameter(_ValueType, out ParameterDefinition var_ParameterDefinition))
                {
                    this.processor.Emit(OpCodes.Ldarg, var_ParameterDefinition);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void AppendCreateNewValue(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _ValueType)
            {
                if (_ValueType.IsArray)
                {
                    // Note: Import stays array.
                    // TypeReference a = this.module.ImportReference(_ValueType);
                    // Note: Resolve gets the element type. Is no array anymore!
                    // TypeDefinition var_Type = _ValueType.Resolve();
                    this.processor.Emit(OpCodes.Ldnull);
                    return;
                }

                /*if(_ValueType.IsGenericInstance || _ValueType.HasGenericParameters || _ValueType.ContainsGenericParameter)
                {
                    this.processor.Emit(OpCodes.Ldnull);
                    return;
                }*/

                switch (_ValueType.Name)
                {
                    case "String":
                        processor.Emit(OpCodes.Ldstr, (String)GenerateRandomString());
                        break;
                    case "SByte":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(sbyte.MinValue, sbyte.MaxValue));
                        break;
                    case "Int16":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(short.MinValue, short.MaxValue));
                        break;
                    case "Int32":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(int.MinValue, int.MaxValue));
                        break;
                    case "Int64":
                        processor.Emit(OpCodes.Ldc_I8, (Int64)random.Next(int.MinValue, int.MaxValue));
                        break;
                    case "Byte":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(0, byte.MaxValue));
                        break;
                    case "Char":
                    case "UInt16":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(0, ushort.MaxValue));
                        break;
                    case "UInt32":
                        processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(0, int.MaxValue));
                        break;
                    case "UInt64":
                        processor.Emit(OpCodes.Ldc_I8, (Int64)random.Next(0, int.MaxValue));
                        break;
                    case "Single":
                        {
                            this.processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(0, int.MaxValue));
                            this.processor.Emit(OpCodes.Conv_R4);

                            break;
                        }
                    case "Double":
                        {
                            this.processor.Emit(OpCodes.Ldc_I4, (Int32)random.Next(0, int.MaxValue));
                            this.processor.Emit(OpCodes.Conv_R8);
                            break;
                        }
                    case "Boolean":
                        processor.Emit(random.Next(100) > 50 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
                        break;
                    default:
                        if (_ValueType.IsValueType)
                        {
                            if (this.TryGetVariable(_Current_MethodBodyBlock, _ValueType, out VariableDefinition var_VariableDefinition))
                            {
                            }
                            else
                            {
                                // Create a new variable.
                                var_VariableDefinition = this.CreateVariable(_ValueType);

                                // Append to _Current_MethodBodyBlock valid variables.
                                _Current_MethodBodyBlock.ValidVariables.Add(var_VariableDefinition);
                            }

                            this.processor.Emit(OpCodes.Ldloca, var_VariableDefinition);
                            // Note: Never init TypeDefinition only TypeReference!
                            this.processor.Emit(OpCodes.Initobj, this.module.ImportReference(_ValueType));
                            this.processor.Emit(OpCodes.Ldloc, var_VariableDefinition);
                        }
                        else
                        {
                            if (this.TryAppendConstructor(_Current_MethodBodyBlock, _ValueType))
                            {

                            }
                            else
                            {
                                this.processor.Emit(OpCodes.Ldnull);
                            }
                        }
                        break;
                }
            }

            private static string GenerateRandomString()
            {
                var characters = new char[random.Next(1, 10)];
                for (int i = 0; i < characters.Length; i++)
                    characters[i] = (char)random.Next(65, 91);
                return new string(characters);
            }

            /// <summary>
            /// Append a method void call.
            /// </summary>
            private void AppendInvocationStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                List<MethodDefinition> var_Valid_ReturningMethods = new List<MethodDefinition>();

                try
                {
                    var_Valid_ReturningMethods.AddRange(this.GetRandomMethods(_Current_MethodBodyBlock, 4, true));
                }
                catch (Exception e)
                {
                    return;
                }

                if (var_Valid_ReturningMethods.Count == 0)
                {
                    return;
                }

                // Random Method
                int var_Random_Index = random.Next(0, var_Valid_ReturningMethods.Count);

                // Prever a method with exisiting types.
                for (int m = 0; m < var_Valid_ReturningMethods.Count; m++)
                {
                    for (int p = 0; p < var_Valid_ReturningMethods[m].Parameters.Count; p++)
                    {
                        TypeDefinition var_ParameterType = var_Valid_ReturningMethods[m].Parameters[p].ParameterType.Resolve();

                        if (_Current_MethodBodyBlock.ValidVariables.Count(v => v.VariableType.Resolve().FullName == var_ParameterType.FullName) > 0)
                        {
                            var_Random_Index = m;
                            break;
                        }

                        if (this.validParameters.Count(v => v.ParameterType.Resolve().FullName == var_ParameterType.FullName) > 0)
                        {
                            var_Random_Index = m;
                            break;
                        }
                    }
                }

                // Get the method.
                MethodReference var_MethodReference = var_Valid_ReturningMethods[var_Random_Index];

                // Append call!
                this.AppendCall(_Current_MethodBodyBlock, var_MethodReference);

                // If not void, apply value to Variable.
                if (this.IsVoid(var_MethodReference.ReturnType))
                {

                }
                else
                {
                    // Create Variable of type!
                    VariableDefinition var_VariableDefinition = this.CreateVariable(var_MethodReference.ReturnType);

                    // Assign to variable.
                    this.AppendAssignVariable(var_VariableDefinition);
                }
            }

            private void AppendCall(MethodBodyBlock _Current_MethodBodyBlock, MethodReference _MethodReference)
            {
                OpCode var_OpCode;

                if (_MethodReference.Resolve().IsStatic)
                {
                    if (_MethodReference.Resolve().IsVirtual)
                    {
                        var_OpCode = OpCodes.Callvirt;
                    }
                    else
                    {
                        var_OpCode = OpCodes.Call;
                    }
                }
                else
                {
                    if (_MethodReference.Resolve().IsConstructor)
                    {
                        var_OpCode = OpCodes.Newobj;
                    }
                    else
                    {
                        // Get the value to call the method on.
                        this.AppendLoadValue(_Current_MethodBodyBlock, _MethodReference.DeclaringType);

                        if (_MethodReference.Resolve().IsVirtual)
                        {
                            var_OpCode = OpCodes.Callvirt;
                        }
                        else
                        {
                            var_OpCode = OpCodes.Call;
                        }
                    }
                }

                // Method has parameter, assign parameter.
                foreach (var var_Parameter in _MethodReference.Parameters)
                {
                    AppendLoadValue(_Current_MethodBodyBlock, var_Parameter.ParameterType);
                }

                // Emit method call!
                // Note: Never call MethodDefinition only MethodReference!
                processor.Emit(var_OpCode, this.module.ImportReference(_MethodReference));
            }

            private List<MethodDefinition> GetRandomMethods(MethodBodyBlock _Current_MethodBodyBlock, int _IteratedTypes, bool _TypeCanBeStatic)
            {
                List<MethodDefinition> var_Valid_ReturningMethods = new List<MethodDefinition>();

                for (int i = 0; i < _IteratedTypes; i++)
                {
                    TypeDefinition var_Random_TypeDefinition = this.GetRandomType(_Current_MethodBodyBlock, _TypeCanBeStatic).Resolve();

                    if (var_Random_TypeDefinition.IsValueType)
                    {
                        continue;
                    }

                    for (int m = 0; m < var_Random_TypeDefinition.Methods.Count; m++)
                    {
                        if (!var_Random_TypeDefinition.Methods[m].IsPublic)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].IsConstructor)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].HasGenericParameters)
                        {
                            continue;
                        }

                        if (MethodReferenceHelper.IsVarArg(var_Random_TypeDefinition.Methods[m]))
                        {
                            continue;
                        }

                        // Note: Why skip those?
                        /*if (var_Random_TypeDefinition.Methods[m].IsVirtual)
                        {
                            continue;
                        }*/

                        if (var_Random_TypeDefinition.Methods[m].IsNative)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].IsPInvokeImpl)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].IsUnmanaged)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].ReturnType.IsPointer)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].ReturnType.IsArray)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].ReturnType.Resolve().HasGenericParameters)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].Parameters.Count(p => p.ParameterType.IsPointer) > 0)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].Parameters.Count(p => p.ParameterType.IsArray) > 0)
                        {
                            continue;
                        }

                        if (var_Random_TypeDefinition.Methods[m].Parameters.Count(p => p.ParameterType.IsGenericInstance || p.ParameterType.HasGenericParameters || p.ParameterType.ContainsGenericParameter) > 0)
                        {
                            continue;
                        }

                        var_Valid_ReturningMethods.Add(var_Random_TypeDefinition.Methods[m]);
                    }
                }

                // Shuffle!
                int var_Count = var_Valid_ReturningMethods.Count - 1;
                for (int i = 0; i < var_Count; i++)
                {
                    int var_MethodId = random.Next(0, var_Valid_ReturningMethods.Count);
                    int var_SwitchMethodId = random.Next(0, var_Valid_ReturningMethods.Count);
                    MethodDefinition var_Temp = var_Valid_ReturningMethods[var_SwitchMethodId];

                    var_Valid_ReturningMethods[var_SwitchMethodId] = var_Valid_ReturningMethods[var_MethodId];
                    var_Valid_ReturningMethods[var_MethodId] = var_Temp;
                }

                return var_Valid_ReturningMethods;
            }

            private bool TryAppendRandomCall(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _ReturnType)
            {
                List<MethodDefinition> var_Valid_ReturningMethods = new List<MethodDefinition>();

                try
                {
                    var_Valid_ReturningMethods.AddRange(this.GetRandomMethods(_Current_MethodBodyBlock, 4, true));
                }
                catch (Exception e)
                {
                    return false;
                }

                if (var_Valid_ReturningMethods.Count == 0)
                {
                    return false;
                }

                MethodDefinition var_Method = null;

                for (int m = 0; m < var_Valid_ReturningMethods.Count; m++)
                {
                    try
                    {
                        // TODO: Add array check! or do not check resolved fullname?
                        if (var_Valid_ReturningMethods[m].ReturnType.Resolve().FullName == _ReturnType.Resolve().FullName)
                        {
                            var_Method = var_Valid_ReturningMethods[m];
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }

                if (var_Method == null)
                {
                    return false;
                }

                this.AppendCall(_Current_MethodBodyBlock, var_Method);

                /*if (method.ReturnType.FullName != _ReturnType.FullName)
                {
                    _processor.Emit(method.ReturnType.IsValueType ? OpCodes.Isinst : OpCodes.Castclass, _module.ImportReference(_ReturnType));
                }*/

                return true;
            }

            private bool TryAppendConstructor(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _Return_TypeReference)
            {
                TypeDefinition _Return_TypeDefinition = _Return_TypeReference.Resolve();

                if (_Return_TypeDefinition == null)
                {
                    return false;
                }

                if (_Return_TypeDefinition.IsAbstract || _Return_TypeDefinition.HasGenericParameters)
                {
                    return false;
                }

                List<MethodDefinition> var_Valid_ConstructorMethods = new List<MethodDefinition>();

                try
                {
                    List<MethodDefinition> var_All_ConstructorMethods = TypeDefinitionHelper.GetAllConstructor(_Return_TypeDefinition);

                    for (int m = 0; m < var_All_ConstructorMethods.Count; m++)
                    {
                        if (MethodReferenceHelper.IsVarArg(var_All_ConstructorMethods[m]))
                        {
                            continue;
                        }

                        if (var_All_ConstructorMethods[m].Parameters.Count(p => p.ParameterType.IsPointer) > 0)
                        {
                            continue;
                        }

                        if (var_All_ConstructorMethods[m].Parameters.Count(p => p.ParameterType.IsArray) > 0)
                        {
                            continue;
                        }

                        if (var_All_ConstructorMethods[m].Parameters.Count(p => p.ParameterType.IsGenericInstance || p.ParameterType.HasGenericParameters || p.ParameterType.ContainsGenericParameter) > 0)
                        {
                            continue;
                        }

                        var_Valid_ConstructorMethods.Add(this.module.ImportReference(var_All_ConstructorMethods[m]).Resolve());
                    }

                    if (var_Valid_ConstructorMethods.Count == 0)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }

                int var_Random_Constructor_Index = random.Next(var_Valid_ConstructorMethods.Count);

                MethodDefinition var_Method = this.module.ImportReference(var_Valid_ConstructorMethods[var_Random_Constructor_Index]).Resolve();

                if (var_Method == null)
                {
                    return false;
                }

                this.AppendCall(_Current_MethodBodyBlock, var_Method);

                return true;
            }

            private void AppendLoadValue(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _ValueType)
            {
                // First try to use variable or parameter!

                // Append a loaded variable.
                if (this.TryAppendLoadVariable(_Current_MethodBodyBlock, _ValueType))
                {
                    return;
                }
                else
                {
                    if (this.TryAppendLoadParameter(_ValueType))
                    {
                        return;
                    }
                }

                // No variable or parameter found, use new values, operators or calls.
                switch (random.Next(3))
                {
                    case 0:
                        {
                            // Append a new created value.
                            this.AppendCreateNewValue(_Current_MethodBodyBlock, _ValueType);
                            break;
                        }
                    case 1:
                        {
                            // Try an operation.
                            if (this.TryAppendBinaryOperation(_Current_MethodBodyBlock, _ValueType))
                            {

                            }
                            else
                            {
                                // Append a new created value.
                                this.AppendCreateNewValue(_Current_MethodBodyBlock, _ValueType);
                            }
                            break;
                        }
                    default:
                        {
                            // Append a method call.
                            if (this.TryAppendRandomCall(_Current_MethodBodyBlock, _ValueType))
                            {

                            }
                            else
                            {
                                // Append a new created value.
                                this.AppendCreateNewValue(_Current_MethodBodyBlock, _ValueType);
                            }
                        }
                        break;
                }
            }

            private bool TryAppendBinaryOperation(MethodBodyBlock _Current_MethodBodyBlock, TypeReference _ValueType)
            {
                if (this.IsBool(_ValueType))
                {
                    TypeDefinition var_Random_TypeDefinition = this.GetRandomType(_Current_MethodBodyBlock, false);

                    if (this.IsString(var_Random_TypeDefinition))
                    {
                        return false;
                    }
                    else
                    {
                        // Note: No need!
                        // TODO: call bool [mscorlib]System.String::op_Equality(string, string)
                    }

                    this.AppendLoadValue(_Current_MethodBodyBlock, var_Random_TypeDefinition);

                    this.processor.Emit(OpCodes.Box, this.module.ImportReference(var_Random_TypeDefinition));

                    this.AppendLoadValue(_Current_MethodBodyBlock, var_Random_TypeDefinition);

                    this.processor.Emit(OpCodes.Box, this.module.ImportReference(var_Random_TypeDefinition));

                    this.processor.Emit(OpCodes.Ceq);

                    return true;
                }

                if (this.IsNumber(_ValueType))
                {
                    TypeDefinition var_Random_TypeDefinition = this.GetRandomNumberType();

                    this.AppendLoadValue(_Current_MethodBodyBlock, var_Random_TypeDefinition);
                    this.AppendLoadValue(_Current_MethodBodyBlock, var_Random_TypeDefinition);

                    switch (random.Next(2))
                    {
                        case 0:
                            {
                                this.processor.Emit(OpCodes.Add);
                                break;
                            }
                        default:
                            {
                                this.processor.Emit(OpCodes.Sub);
                                break;
                            }
                    }

                    switch (_ValueType.Name)
                    {
                        case "Byte":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "SByte":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "Int16":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "Int32":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "Int64":
                            processor.Emit(OpCodes.Conv_I8);
                            break;
                        case "UInt16":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "UInt32":
                            processor.Emit(OpCodes.Conv_I4);
                            break;
                        case "UInt64":
                            processor.Emit(OpCodes.Conv_I8);
                            break;
                        case "Single":
                            {
                                this.processor.Emit(OpCodes.Conv_R4);
                                break;
                            }
                        case "Double":
                            {
                                this.processor.Emit(OpCodes.Conv_R8);
                                break;
                            }
                    }

                    return true;
                }

                return false;
            }

            private void AppendIfStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Get a bool value.
                this.AppendLoadValue(_Current_MethodBodyBlock, this.GetRandomBoolType());

                // Check if add else block.
                bool var_AppendElseBlock = random.Next(100) > 50;

                // If target.
                Instruction var_Target1 = Instruction.Create(OpCodes.Nop);

                // Else target.
                Instruction var_Target2 = Instruction.Create(OpCodes.Nop);

                // If true go to var_Target1
                this.processor.Emit(OpCodes.Brtrue, var_Target1);

                // Add content (the else content)
                this.AppendBlock(_Current_MethodBodyBlock);

                // Else branch, go to var_Target2
                if (var_AppendElseBlock)
                {
                    this.processor.Emit(OpCodes.Br, var_Target2);
                }

                // Jump for true.
                this.processor.Append(var_Target1);

                if (var_AppendElseBlock)
                {
                    // Add content (the if content)
                    this.AppendBlock(_Current_MethodBodyBlock);

                    // Else jump.
                    this.processor.Append(var_Target2);
                }
            }

            private void AppendDoLoopStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Is do or do-while
                bool var_IsDoLoop = random.Next(100) > 50;

                // Targets.
                var target1 = Instruction.Create(OpCodes.Nop);
                var target2 = Instruction.Create(OpCodes.Nop);

                if (!var_IsDoLoop)
                {
                    this.processor.Emit(OpCodes.Br, target2);
                }

                this.processor.Append(target1);

                // Append Content.
                this.AppendBlock(_Current_MethodBodyBlock);

                this.processor.Append(target2);

                // The bool value for the loop.
                this.AppendLoadValue(_Current_MethodBodyBlock, this.GetRandomBoolType());

                this.processor.Emit(OpCodes.Brtrue, target1);
            }

            private void AppendForLoopStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Create Variables.
                VariableDefinition var_For_Int_Variable = this.CreateVariable(this.module.TypeSystem.Int32);
                VariableDefinition var_For_Bool_Variable = this.CreateVariable(this.module.TypeSystem.Boolean);

                // Append to _Current_MethodBodyBlock valid variables.
                // Note: Else the for will use var_For_Int_Variable in not valid / stupid cases.
                // _Current_MethodBodyBlock.ValidVariables.Add(var_For_Int_Variable);

                // Create Targets.
                Instruction var_Target_Check = Instruction.Create(OpCodes.Nop);
                Instruction var_Target_Body = Instruction.Create(OpCodes.Nop);

                // Load and assign iterator (int i = 0,
                this.processor.Emit(OpCodes.Ldc_I4_0);
                this.processor.Emit(OpCodes.Stloc, var_For_Int_Variable);

                // Start branch
                this.processor.Emit(OpCodes.Br, var_Target_Check);

                // Add Content Target.
                this.processor.Append(var_Target_Body);

                // Append Content.
                this.AppendBlock(_Current_MethodBodyBlock);

                // Increase iterator , i++
                this.processor.Emit(OpCodes.Ldloc, var_For_Int_Variable);
                this.processor.Emit(OpCodes.Ldc_I4_1);
                this.processor.Emit(OpCodes.Add);
                this.processor.Emit(OpCodes.Stloc, var_For_Int_Variable);

                // Add Check Target.
                this.processor.Append(var_Target_Check);

                // Check , i < Number,
                this.processor.Emit(OpCodes.Ldloc, var_For_Int_Variable);

                this.AppendLoadValue(_Current_MethodBodyBlock, this.module.TypeSystem.Int32);
                //this.processor.Emit(OpCodes.Ldc_I4, 100);

                this.processor.Emit(OpCodes.Clt);
                this.processor.Emit(OpCodes.Stloc, var_For_Bool_Variable);
                this.processor.Emit(OpCodes.Ldloc, var_For_Bool_Variable);
                this.processor.Emit(OpCodes.Brtrue, var_Target_Body);
            }

            private void AppendTryStatement(MethodBodyBlock _Current_MethodBodyBlock)
            {
                // Either try-catch or try-finally.
                var handler = new ExceptionHandler(random.Next(2) == 1 ? ExceptionHandlerType.Catch : ExceptionHandlerType.Finally);

                // End instruction after try.
                var target = Instruction.Create(OpCodes.Nop);

                // Create try.
                this.processor.Append(handler.TryStart = Instruction.Create(OpCodes.Nop));

                // Add try block content.
                this.AppendBlock(_Current_MethodBodyBlock);

                // Add the target after try.
                this.processor.Emit(OpCodes.Leave, target);

                if (handler.HandlerType == ExceptionHandlerType.Catch)
                {
                    // Is catch handle, so add catch.
                    this.processor.Append(handler.TryEnd = handler.HandlerStart = Instruction.Create(OpCodes.Pop));
                    handler.CatchType = this.module.ImportReference(typeof(Exception));
                }
                else
                {
                    // Is catch handle, so add finally.
                    this.processor.Append(handler.TryEnd = handler.HandlerStart = Instruction.Create(OpCodes.Nop));
                }

                // Append catch/finally block.
                this.AppendBlock(_Current_MethodBodyBlock);

                // Leave this block.
                if (handler.HandlerType == ExceptionHandlerType.Catch)
                {
                    this.processor.Emit(OpCodes.Leave_S, target);
                }
                else
                {
                    this.processor.Append(Instruction.Create(OpCodes.Endfinally));
                }

                // Add handler end.
                this.processor.Append(handler.HandlerEnd = target);

                // Add exception to handler.
                this.body.ExceptionHandlers.Add(handler);
            }


            /// <summary>
            /// Closes the method with a return.
            /// </summary>
            private void FinalizeMethod()
            {
                this.processor.Emit(OpCodes.Ret);
            }

#endregion
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

#endif