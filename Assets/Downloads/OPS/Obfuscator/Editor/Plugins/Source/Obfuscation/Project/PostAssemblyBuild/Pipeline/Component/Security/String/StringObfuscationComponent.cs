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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security
{
    public class StringObfuscationComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "String - Obfuscation";
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
                return "Activate the obfuscation of strings.";
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
                return "Activate the obfuscation of strings.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_String";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CEnable_String_Obfuscation = "Enable_String_Obfuscation";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_String_Obfuscation);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = "Activate this setting to use a strong string obfuscation.";
            var_Content.AddRow(var_ExplanationRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, true);

            return var_ObfuscatorContainer;
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

        //String Helper
        #region String Helper

#if Obfuscator_Free
#else

        private class StringObfuscationManager
        {
            private TypeDefinition StaticStringObfuscationType { get; set; }

            private TypeDefinition StructType { get; set; }

            private FieldDefinition DataConstantField { get; set; }

            private FieldDefinition DataField { get; set; }

            private FieldDefinition StringArrayField { get; set; }

            private MethodDefinition StringGetterMethodDefinition { get; set; }

            private TypeReference SystemStringTypeReference { get; set; }

            private TypeReference SystemVoidTypeReference { get; set; }

            private TypeReference SystemByteTypeReference { get; set; }

            private TypeReference SystemIntTypeReference { get; set; }

            private MethodReference SystemInitializeArrayReference { get; set; }

            private int _stringIndex;

            private readonly Dictionary<string, MethodDefinition> _methodByString =
                new Dictionary<string, MethodDefinition>();

            private uint _nameIndex;

            // Array of bytes receiving the obfuscated strings in UTF8 format.
            private readonly List<byte> _dataBytes = new List<byte>();

            // Assembly
            #region Assembly

            private readonly AssemblyDefinition assemblyDefinition;

            private bool initialized;

            #endregion

            public StringObfuscationManager(AssemblyInfo var_AssemblyInfo)
            {
                this.assemblyDefinition = var_AssemblyInfo.AssemblyDefinition;
            }

            private void Initialize()
            {
                if (initialized)
                    return;

                initialized = true;
                var library = assemblyDefinition;

                // We get the most used type references
                var systemObjectTypeReference = library.MainModule.TypeSystem.Object;
                SystemVoidTypeReference = library.MainModule.TypeSystem.Void;
                SystemStringTypeReference = library.MainModule.TypeSystem.String;
                var systemValueTypeTypeReference = new TypeReference("System", "ValueType", library.MainModule,
                    library.MainModule.TypeSystem.CoreLibrary);
                SystemByteTypeReference = library.MainModule.TypeSystem.Byte;
                SystemIntTypeReference = library.MainModule.TypeSystem.Int32;
                var encoding = new TypeReference("System.Text", "Encoding", library.MainModule,
                    library.MainModule.TypeSystem.CoreLibrary).Resolve();

                if (encoding == null)
                {
                    return;
                }

                var method1 =
                    library.MainModule.ImportReference(encoding.Methods.FirstOrDefault(method => method.Name == "get_UTF8"));
                var method2 = library.MainModule.ImportReference(encoding.Methods.FirstOrDefault(method =>
                    method.FullName ==
                    "System.String System.Text.Encoding::GetString(System.Byte[],System.Int32,System.Int32)"));
                var runtimeHelpers = new TypeReference("System.Runtime.CompilerServices", "RuntimeHelpers",
                    library.MainModule, library.MainModule.TypeSystem.CoreLibrary).Resolve();

                SystemInitializeArrayReference = library.MainModule.ImportReference((System.Reflection.MethodBase)typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray", new Type[2]
                {
                    typeof(Array),
                    typeof(RuntimeFieldHandle)
                }));

                // New static class with a method for each unique string we substitute.
                StaticStringObfuscationType = new TypeDefinition(
                    "<PrivateImplementationDetails>{" + Guid.NewGuid().ToString().ToUpper() + "}",
                    /*Guid.NewGuid().ToString().ToUpper()*/"a",
                    TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit, systemObjectTypeReference);

                // Add struct for constant byte array data
                StructType = new TypeDefinition("", "a_",
                    TypeAttributes.ExplicitLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed |
                    TypeAttributes.NestedPrivate, systemValueTypeTypeReference);
                StructType.PackingSize = 1;
                StaticStringObfuscationType.NestedTypes.Add(StructType);

                // Add field with constant string data
                DataConstantField = new FieldDefinition("a_",
                    FieldAttributes.HasFieldRVA | FieldAttributes.Private | FieldAttributes.Static |
                    FieldAttributes.Assembly, StructType);
                StaticStringObfuscationType.Fields.Add(DataConstantField);

                // Add data field where constructor copies the data to
                DataField = new FieldDefinition("a__",
                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly,
                    new ArrayType(SystemByteTypeReference));
                StaticStringObfuscationType.Fields.Add(DataField);

                // Add string array of deobfuscated strings
                StringArrayField = new FieldDefinition("a___",
                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly,
                    new ArrayType(SystemStringTypeReference));
                StaticStringObfuscationType.Fields.Add(StringArrayField);

                // Add method to extract a string from the byte array. It is called by the indiviual string getter methods we add later to the class.
                StringGetterMethodDefinition = new MethodDefinition("a_",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig,
                    SystemStringTypeReference);
                StringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(SystemIntTypeReference));
                StringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(SystemIntTypeReference));
                StringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(SystemIntTypeReference));
                StringGetterMethodDefinition.Body.Variables.Add(new VariableDefinition(SystemStringTypeReference));
                ILProcessor worker3 = StringGetterMethodDefinition.Body.GetILProcessor();

                worker3.Emit(OpCodes.Call, method1);
                worker3.Emit(OpCodes.Ldsfld, DataField);
                worker3.Emit(OpCodes.Ldarg_1);
                worker3.Emit(OpCodes.Ldarg_2);
                worker3.Emit(OpCodes.Callvirt, method2);
                worker3.Emit(OpCodes.Stloc_0);

                worker3.Emit(OpCodes.Ldsfld, StringArrayField);
                worker3.Emit(OpCodes.Ldarg_0);
                worker3.Emit(OpCodes.Ldloc_0);
                worker3.Emit(OpCodes.Stelem_Ref);

                worker3.Emit(OpCodes.Ldloc_0);
                worker3.Emit(OpCodes.Ret);
                StaticStringObfuscationType.Methods.Add(StringGetterMethodDefinition);
            }

            public void Squeeze()
            {
                if (!initialized)
                    return;

                // Now that we know the total size of the byte array, we can update the struct size and store it in the constant field
                StructType.ClassSize = _dataBytes.Count;
                for (int i = 0; i < _dataBytes.Count; i++)
                    _dataBytes[i] = (byte)(_dataBytes[i] ^ (byte)i ^ 0xAA);
                DataConstantField.InitialValue = _dataBytes.ToArray();

                // Add static constructor which initializes the dataField from the constant data field
                MethodDefinition ctorMethodDefinition = new MethodDefinition(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, SystemVoidTypeReference);
                StaticStringObfuscationType.Methods.Add(ctorMethodDefinition);
                ctorMethodDefinition.Body = new MethodBody(ctorMethodDefinition);
                ctorMethodDefinition.Body.Variables.Add(new VariableDefinition(SystemIntTypeReference));

                ILProcessor worker2 = ctorMethodDefinition.Body.GetILProcessor();
                worker2.Emit(OpCodes.Ldc_I4, _stringIndex);
                worker2.Emit(OpCodes.Newarr, SystemStringTypeReference);
                worker2.Emit(OpCodes.Stsfld, StringArrayField);


                worker2.Emit(OpCodes.Ldc_I4, _dataBytes.Count);
                worker2.Emit(OpCodes.Newarr, SystemByteTypeReference);
                worker2.Emit(OpCodes.Dup);
                worker2.Emit(OpCodes.Ldtoken, DataConstantField);
                worker2.Emit(OpCodes.Call, SystemInitializeArrayReference);
                worker2.Emit(OpCodes.Stsfld, DataField);

                worker2.Emit(OpCodes.Ldc_I4_0);
                worker2.Emit(OpCodes.Stloc_0);

                Instruction backlabel1 = worker2.Create(OpCodes.Br_S, ctorMethodDefinition.Body.Instructions[0]);
                worker2.Append(backlabel1);
                Instruction label2 = worker2.Create(OpCodes.Ldsfld, DataField);
                worker2.Append(label2);
                worker2.Emit(OpCodes.Ldloc_0);
                worker2.Emit(OpCodes.Ldsfld, DataField);
                worker2.Emit(OpCodes.Ldloc_0);
                worker2.Emit(OpCodes.Ldelem_U1);
                worker2.Emit(OpCodes.Ldloc_0);
                worker2.Emit(OpCodes.Xor);
                worker2.Emit(OpCodes.Ldc_I4, 0xAA);
                worker2.Emit(OpCodes.Xor);
                worker2.Emit(OpCodes.Conv_U1);
                worker2.Emit(OpCodes.Stelem_I1);
                worker2.Emit(OpCodes.Ldloc_0);
                worker2.Emit(OpCodes.Ldc_I4_1);
                worker2.Emit(OpCodes.Add);
                worker2.Emit(OpCodes.Stloc_0);
                backlabel1.Operand = worker2.Create(OpCodes.Ldloc_0);
                worker2.Append((Instruction)backlabel1.Operand);
                worker2.Emit(OpCodes.Ldsfld, DataField);
                worker2.Emit(OpCodes.Ldlen);
                worker2.Emit(OpCodes.Conv_I4);
                worker2.Emit(OpCodes.Clt);
                worker2.Emit(OpCodes.Brtrue, label2);
                worker2.Emit(OpCodes.Ret);

                assemblyDefinition.MainModule.Types.Add(StaticStringObfuscationType);
            }

            public void ProcessStrings(PostAssemblyBuildStep _Step, AssemblyInfo var_AssemblyInfo, MethodDefinition _Method)
            {
                if (_Method.Body == null)
                {
                    return;
                }

                Initialize();

                // Unroll short form instructions so they can be auto-fixed by Cecil
                // automatically when instructions are inserted/replaced
                _Method.Body.SimplifyMacros();

                ILProcessor worker = _Method.Body.GetILProcessor();

                //
                // Make a dictionary of all instructions to replace and their replacement.
                //
                //Dictionary<Instruction, InstructionReplacement> oldToNewStringInstructions = new Dictionary<Instruction, InstructionReplacement>();

                for (int index = 0; index < _Method.Body.Instructions.Count; index++)
                {
                    Instruction instruction = _Method.Body.Instructions[index];

                    if (instruction.OpCode == OpCodes.Ldstr)
                    {
                        string str = (string)instruction.Operand;
                        MethodDefinition individualStringMethodDefinition;
                        if (!_methodByString.TryGetValue(str, out individualStringMethodDefinition))
                        {
                            string methodName = _Step.DataContainer.RenameManager.NameGenerator.GetNextName(_nameIndex++);

                            // Add the string to the data array
                            byte[] stringBytes = Encoding.UTF8.GetBytes(str);
                            int start = _dataBytes.Count;
                            _dataBytes.AddRange(stringBytes);
                            int count = _dataBytes.Count - start;

                            // Add a method for this string to our new class
                            individualStringMethodDefinition = new MethodDefinition(
                                methodName,
                                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig,
                                SystemStringTypeReference);
                            individualStringMethodDefinition.Body = new MethodBody(individualStringMethodDefinition);
                            ILProcessor worker4 = individualStringMethodDefinition.Body.GetILProcessor();

                            worker4.Emit(OpCodes.Ldsfld, StringArrayField);
                            worker4.Emit(OpCodes.Ldc_I4, _stringIndex);
                            worker4.Emit(OpCodes.Ldelem_Ref);
                            worker4.Emit(OpCodes.Dup);
                            Instruction label20 = worker4.Create(
                                OpCodes.Brtrue_S,
                                StringGetterMethodDefinition.Body.Instructions[0]);
                            worker4.Append(label20);
                            worker4.Emit(OpCodes.Pop);
                            worker4.Emit(OpCodes.Ldc_I4, _stringIndex);
                            worker4.Emit(OpCodes.Ldc_I4, start);
                            worker4.Emit(OpCodes.Ldc_I4, count);
                            worker4.Emit(OpCodes.Call, StringGetterMethodDefinition);

                            label20.Operand = worker4.Create(OpCodes.Ret);
                            worker4.Append((Instruction)label20.Operand);

                            StaticStringObfuscationType.Methods.Add(individualStringMethodDefinition);
                            _methodByString.Add(str, individualStringMethodDefinition);

                            _stringIndex++;
                        }

                        // Replace Ldstr with Call

                        Instruction var_NewInstruction = worker.Create(OpCodes.Call, individualStringMethodDefinition);
                        var_NewInstruction.Offset = instruction.Offset;

                        Helper_Replace_Instruction(_Method, instruction, var_NewInstruction);

                        for (Instruction next = var_NewInstruction.Next; next != null; next = next.Next)
                        {
                            next.Offset = var_NewInstruction.Offset + var_NewInstruction.GetSize();
                            var_NewInstruction = next;
                        }

                        //oldToNewStringInstructions.Add(instruction, new InstructionReplacement(index, var_NewInstruction));
                    }
                }

                //worker.ReplaceAndFixReferences(_Method.Body, oldToNewStringInstructions);

                // Optimize method back
                _Method.Body.OptimizeMacros();
            }
        }

        public void HideStrings(PostAssemblyBuildStep _Step, AssemblyInfo var_AssemblyInfo)
        {
            AssemblyDefinition var_AssemblyDefinition = var_AssemblyInfo.AssemblyDefinition;

            Dictionary<string, MethodDefinition> methodByString = new Dictionary<string, MethodDefinition>();

            uint nameIndex = 0;

            // We get the most used type references
            TypeReference systemObjectTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(Object));
            TypeReference systemVoidTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(void));
            TypeReference systemStringTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(String));
            TypeReference systemValueTypeTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(ValueType));
            TypeReference systemByteTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(byte));
            TypeReference systemIntTypeReference = var_AssemblyDefinition.MainModule.ImportReference(typeof(int));

            // New static class with a method for each unique string we substitute.
            TypeDefinition newtype = new TypeDefinition("<PrivateImplementationDetails>{" + Guid.NewGuid().ToString().ToUpper() + "}", Guid.NewGuid().ToString().ToUpper(), TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, systemObjectTypeReference);

            // Array of bytes receiving the obfuscated strings in UTF8 format.
            List<byte> databytes = new List<byte>();

            // Add struct for constant byte array data
            TypeDefinition structType = new TypeDefinition("", "a_", TypeAttributes.ExplicitLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.NestedPrivate, systemValueTypeTypeReference);
            structType.PackingSize = 1;
            newtype.NestedTypes.Add(structType);

            // Add field with constant string data
            FieldDefinition dataConstantField = new FieldDefinition("b_", FieldAttributes.HasFieldRVA | FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly, structType);
            newtype.Fields.Add(dataConstantField);

            // Add data field where constructor copies the data to
            FieldDefinition dataField = new FieldDefinition("b__", FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly, new ArrayType(systemByteTypeReference));
            newtype.Fields.Add(dataField);

            // Add string array of deobfuscated strings
            FieldDefinition stringArrayField = new FieldDefinition("b___", FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Assembly, new ArrayType(systemStringTypeReference));
            newtype.Fields.Add(stringArrayField);

            // Add method to extract a string from the byte array. It is called by the indiviual string getter methods we add later to the class.
            MethodDefinition stringGetterMethodDefinition = new MethodDefinition("c_", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig, systemStringTypeReference);
            stringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(systemIntTypeReference));
            stringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(systemIntTypeReference));
            stringGetterMethodDefinition.Parameters.Add(new ParameterDefinition(systemIntTypeReference));
            stringGetterMethodDefinition.Body.Variables.Add(new VariableDefinition(systemStringTypeReference));
            ILProcessor worker3 = stringGetterMethodDefinition.Body.GetILProcessor();

            worker3.Emit(OpCodes.Call, var_AssemblyDefinition.MainModule.ImportReference(typeof(System.Text.Encoding).GetProperty("UTF8").GetGetMethod()));
            worker3.Emit(OpCodes.Ldsfld, dataField);
            worker3.Emit(OpCodes.Ldarg_1);
            worker3.Emit(OpCodes.Ldarg_2);
            worker3.Emit(OpCodes.Callvirt, var_AssemblyDefinition.MainModule.ImportReference(typeof(System.Text.Encoding).GetMethod("GetString", new[] {
                    typeof(byte[]),
                    typeof(int),
                    typeof(int)
                })));
            worker3.Emit(OpCodes.Stloc_0);

            worker3.Emit(OpCodes.Ldsfld, stringArrayField);
            worker3.Emit(OpCodes.Ldarg_0);
            worker3.Emit(OpCodes.Ldloc_0);
            worker3.Emit(OpCodes.Stelem_Ref);

            worker3.Emit(OpCodes.Ldloc_0);
            worker3.Emit(OpCodes.Ret);
            newtype.Methods.Add(stringGetterMethodDefinition);

            int stringIndex = 0;

            // Look for all string load operations and replace them with calls to indiviual methods in our new class
            foreach (TypeDefinition type in var_AssemblyInfo.GetAllTypeDefinitions() /* info.Definition.MainModule.Types */)
            {
                if (type.FullName == "<Module>")
                    continue;

                foreach (MethodDefinition method in type.Methods)
                {
                    if (method.Body != null)
                    {
                        method.Body.SimplifyMacros();

                        ILProcessor worker = method.Body.GetILProcessor();

                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            Instruction instruction = method.Body.Instructions[i];
                            if (instruction.OpCode == OpCodes.Ldstr)
                            {
                                string str = (string)instruction.Operand;
                                MethodDefinition individualStringMethodDefinition = null;
                                if (!methodByString.TryGetValue(str, out individualStringMethodDefinition))
                                {
                                    string methodName = _Step.DataContainer.RenameManager.NameGenerator.GetNextName(nameIndex++);

                                    // Add the string to the data array
                                    byte[] stringBytes = Encoding.UTF8.GetBytes(str);
                                    int start = databytes.Count;
                                    databytes.AddRange(stringBytes);
                                    int count = databytes.Count - start;

                                    // Add a method for this string to our new class
                                    individualStringMethodDefinition = new MethodDefinition(methodName, MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, systemStringTypeReference);
                                    individualStringMethodDefinition.Body = new MethodBody(individualStringMethodDefinition);
                                    ILProcessor worker4 = individualStringMethodDefinition.Body.GetILProcessor();

                                    worker4.Emit(OpCodes.Ldsfld, stringArrayField);
                                    worker4.Emit(OpCodes.Ldc_I4, stringIndex);
                                    worker4.Emit(OpCodes.Ldelem_Ref);
                                    worker4.Emit(OpCodes.Dup);
                                    Instruction label20 = worker4.Create(OpCodes.Brtrue_S, stringGetterMethodDefinition.Body.Instructions[0]);
                                    worker4.Append(label20);
                                    worker4.Emit(OpCodes.Pop);
                                    worker4.Emit(OpCodes.Ldc_I4, stringIndex);
                                    worker4.Emit(OpCodes.Ldc_I4, start);
                                    worker4.Emit(OpCodes.Ldc_I4, count);
                                    worker4.Emit(OpCodes.Call, stringGetterMethodDefinition);


                                    label20.Operand = worker4.Create(OpCodes.Ret);
                                    worker4.Append((Instruction)label20.Operand);

                                    newtype.Methods.Add(individualStringMethodDefinition);
                                    methodByString.Add(str, individualStringMethodDefinition);

                                    stringIndex++;
                                }
                                // Replace Ldstr with Call



                                Instruction var_Newinstruction = worker.Create(OpCodes.Call, individualStringMethodDefinition);
                                var_Newinstruction.Offset = instruction.Offset;
                                Helper_Replace_Instruction(method, instruction, var_Newinstruction);

                                for (Instruction next = var_Newinstruction.Next; next != null; next = next.Next)
                                {
                                    next.Offset = var_Newinstruction.Offset + var_Newinstruction.GetSize();
                                    var_Newinstruction = next;
                                }
                            }
                        }

                        method.Body.OptimizeMacros();
                    }
                }
            }

            // Now that we know the total size of the byte array, we can update the struct size and store it in the constant field
            structType.ClassSize = databytes.Count;
            for (int i = 0; i < databytes.Count; i++)
                databytes[i] = (byte)(databytes[i] ^ (byte)i ^ 0xAA);
            dataConstantField.InitialValue = databytes.ToArray();

            // Add static constructor which initializes the dataField from the constant data field
            MethodDefinition ctorMethodDefinition = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, systemVoidTypeReference);
            newtype.Methods.Add(ctorMethodDefinition);
            ctorMethodDefinition.Body = new MethodBody(ctorMethodDefinition);
            ctorMethodDefinition.Body.Variables.Add(new VariableDefinition(systemIntTypeReference));

            ILProcessor worker2 = ctorMethodDefinition.Body.GetILProcessor();
            worker2.Emit(OpCodes.Ldc_I4, stringIndex);
            worker2.Emit(OpCodes.Newarr, systemStringTypeReference);
            worker2.Emit(OpCodes.Stsfld, stringArrayField);


            worker2.Emit(OpCodes.Ldc_I4, databytes.Count);
            worker2.Emit(OpCodes.Newarr, systemByteTypeReference);
            worker2.Emit(OpCodes.Dup);
            worker2.Emit(OpCodes.Ldtoken, dataConstantField);
            worker2.Emit(OpCodes.Call, var_AssemblyDefinition.MainModule.ImportReference(typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray")));
            worker2.Emit(OpCodes.Stsfld, dataField);

            worker2.Emit(OpCodes.Ldc_I4_0);
            worker2.Emit(OpCodes.Stloc_0);

            Instruction backlabel1 = worker2.Create(OpCodes.Br_S, ctorMethodDefinition.Body.Instructions[0]);
            worker2.Append(backlabel1);
            Instruction label2 = worker2.Create(OpCodes.Ldsfld, dataField);
            worker2.Append(label2);
            worker2.Emit(OpCodes.Ldloc_0);
            worker2.Emit(OpCodes.Ldsfld, dataField);
            worker2.Emit(OpCodes.Ldloc_0);
            worker2.Emit(OpCodes.Ldelem_U1);
            worker2.Emit(OpCodes.Ldloc_0);
            worker2.Emit(OpCodes.Xor);
            worker2.Emit(OpCodes.Ldc_I4, 0xAA);
            worker2.Emit(OpCodes.Xor);
            worker2.Emit(OpCodes.Conv_U1);
            worker2.Emit(OpCodes.Stelem_I1);
            worker2.Emit(OpCodes.Ldloc_0);
            worker2.Emit(OpCodes.Ldc_I4_1);
            worker2.Emit(OpCodes.Add);
            worker2.Emit(OpCodes.Stloc_0);
            backlabel1.Operand = worker2.Create(OpCodes.Ldloc_0);
            worker2.Append((Instruction)backlabel1.Operand);
            worker2.Emit(OpCodes.Ldsfld, dataField);
            worker2.Emit(OpCodes.Ldlen);
            worker2.Emit(OpCodes.Conv_I4);
            worker2.Emit(OpCodes.Clt);
            worker2.Emit(OpCodes.Brtrue, label2);
            worker2.Emit(OpCodes.Ret);

            var_AssemblyDefinition.MainModule.Types.Add(newtype);
        }

        private static void Helper_Replace_Instruction(MethodDefinition var_MethodDefinition, Instruction var_OldInstruction, Instruction var_NewInstruction)
        {
            foreach (Instruction instruction in var_MethodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    Instruction[] array = (Instruction[])instruction.Operand;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != null && array[i] == var_OldInstruction)
                        {
                            array[i] = var_NewInstruction;
                        }
                    }
                }
                else if (instruction.OpCode.OperandType == OperandType.InlineBrTarget && (Instruction)instruction.Operand == var_OldInstruction)
                {
                    instruction.Operand = var_NewInstruction;
                }
            }
            int index = var_MethodDefinition.Body.Instructions.IndexOf(var_OldInstruction);
            var_MethodDefinition.Body.Instructions.Remove(var_OldInstruction);
            var_MethodDefinition.Body.Instructions.Insert(index, var_NewInstruction);
            Helper_Exception(var_MethodDefinition, var_OldInstruction, var_NewInstruction);
        }

        private static void Helper_Exception(MethodDefinition var_MethodDefinition, Instruction var_OldInstruction, Instruction var_NewInstruction)
        {
            foreach (ExceptionHandler exceptionHandler in var_MethodDefinition.Body.ExceptionHandlers)
            {
                if (exceptionHandler.FilterStart == var_OldInstruction)
                {
                    exceptionHandler.FilterStart = var_NewInstruction;
                }
                if (exceptionHandler.HandlerEnd == var_OldInstruction)
                {
                    exceptionHandler.HandlerEnd = var_NewInstruction;
                }
                if (exceptionHandler.HandlerStart == var_OldInstruction)
                {
                    exceptionHandler.HandlerStart = var_NewInstruction;
                }
                if (exceptionHandler.TryEnd == var_OldInstruction)
                {
                    exceptionHandler.TryEnd = var_NewInstruction;
                }
                if (exceptionHandler.TryStart == var_OldInstruction)
                {
                    exceptionHandler.TryStart = var_NewInstruction;
                }
            }
        }

#endif

        #endregion

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
#else
            // Check if string obfuscation got activated.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_String_Obfuscation))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The string obfuscation got skipped by your settings!");

                return true;
            }

            // Instantiate the StringObfuscationManager.
            StringObfuscationManager var_StringObfuscationEmitter = new StringObfuscationManager(_AssemblyInfo);

            // Iterate over all types of the assembly and obfuscate the strings.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Check if the type has the DoNotObfuscateMethodBody Attribute, if so skip the string obfuscation.
                if (AttributeHelper.HasCustomAttribute(var_TypeDefinition, "DoNotObfuscateMethodBody"))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, String.Format("{0} has a DoNotObfuscateMethodBody Attribute, do not obfuscate strings!", var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));
                    continue;
                }

                // Check if the type has the BurstCompile Attribute, if so skip the string obfuscation. Native code cannot call managed code.
                if (AttributeHelper.HasCustomAttribute(var_TypeDefinition, "BurstCompile"))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, String.Format("{0} has a BurstCompile Attribute, do not obfuscate strings! Native code is not compatible with string obfuscation.", var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));
                    continue;
                }                

                // Iterate over all methods of the type and obfuscate the strings.  
                foreach (var var_Method in var_TypeDefinition.Methods)
                {
                    // Check if the method has the DoNotObfuscateMethodBody Attribute, if so skip the string obfuscation.
                    if (AttributeHelper.HasCustomAttribute(var_TypeDefinition, "DoNotObfuscateMethodBody"))
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, String.Format("{0} has a DoNotObfuscateMethodBody Attribute, do not obfuscate string!", var_Method.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>())));
                        continue;
                    }

                    var_StringObfuscationEmitter.ProcessStrings(this.Step, _AssemblyInfo, var_Method);
                }
            }

            // Squeeze the obuscated strings in binary format.
            var_StringObfuscationEmitter.Squeeze();
#endif
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
