using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Clone
{
    // Nice to know: https://github.com/MonoMod/MonoMod.Common/blob/c14fa4301932d0d4e3a8eee0033f4440a980a83a/Utils/Extensions.Relinker.cs
    // Nice to know too: https://github.com/XenocodeRCE/Noisette-Obfuscator/blob/master/Noisette/Protection/AntiTampering/Inject!Helper.cs

    /// <summary>
    /// Helper class for cloning member.
    /// </summary>
    public static class CloneHelper
    {
        // TypeDefinition
        #region TypeDefinition

        /// <summary>
        /// Tries to create a sub type for _SourceType with a namespace _SubTypeNamespace and type name _SubTypeName.
        /// The created _SubType will be added an empty constructor.
        /// </summary>
        /// <param name="_BaseType"></param>
        /// <param name="_SubTypeNamespace"></param>
        /// <param name="_SubTypeName"></param>
        /// <param name="_SubType"></param>
        /// <returns></returns>
        public static bool TryToCreateEmptySubType(TypeDefinition _BaseType, String _SubTypeNamespace, String _SubTypeName, out TypeDefinition _SubType)
        {
            //Create a empty type.
            TypeDefinition var_SubType = new TypeDefinition(_SubTypeNamespace, _SubTypeName, _BaseType.Attributes);

            //Assing basetype.
            var_SubType.BaseType = _BaseType;

            //Add constructor to base type.
            MethodDefinition var_BaseConstrutor = TypeDefinitionHelper.GetEmptyConstructor(_BaseType);

            //Check base constructor!
            if (var_BaseConstrutor == null)
            {
                _SubType = null;

                return false;
            }

            //Add empty constructor
            TypeDefinitionHelper.AddEmptyConstructor(_BaseType.Module, var_SubType, var_BaseConstrutor);

            _SubType = var_SubType;

            return true;
        }

        /// <summary>
        /// Makes a type reference a generic reference.
        /// </summary>
        /// <param name="_Self"></param>
        /// <param name="_GenericArguments"></param>
        /// <returns></returns>
        public static GenericInstanceType MakeTypeGeneric(TypeReference _Self, params TypeReference[] _GenericArguments)
        {
            if (_Self == null)
                throw new ArgumentNullException("self");
            if (_GenericArguments == null)
                throw new ArgumentNullException("arguments");
            if (_GenericArguments.Length == 0)
                throw new ArgumentException();
            if (_Self.GenericParameters.Count != _GenericArguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(_Self);

            foreach (var argument in _GenericArguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        /// <summary>
        /// Tries to clone a type. It will NOT be added to some assembly!
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_TargetNamespace"></param>
        /// <param name="_TargetName"></param>
        /// <param name="_Target"></param>
        /// <returns></returns>
        public static bool TryToCloneType(AssemblyDefinition _TargetAssemblyDefinition, TypeDefinition _Source, String _TargetNamespace, String _TargetName, out TypeDefinition _Target)
        {
            try
            {
                // Create new TypeDefinition
                _Target = new TypeDefinition(_TargetNamespace, _TargetName, _Source.Attributes);

                // BaseType - TODO: Clone - Now is just copy.
                _Target.BaseType = _TargetAssemblyDefinition.MainModule.ImportReference(_Source.BaseType);

                // Copy Layout
                _Target.PackingSize = _Source.PackingSize;
                _Target.ClassSize = _Source.ClassSize;

                // Copy Generic Parameter
                foreach (GenericParameter var_GenericParameter in _Source.GenericParameters)
                {
                    // TODO: CustomAttributes
                    _Target.GenericParameters.Add(CloneGenericParameter(_TargetAssemblyDefinition, _Target, var_GenericParameter));
                }

                // Copy Interfaces
                foreach (InterfaceImplementation var_InterfaceImplementation in _Source.Interfaces)
                {
                    // TODO: CustomAttributes
                    _Target.Interfaces.Add(new InterfaceImplementation(_TargetAssemblyDefinition.MainModule.ImportReference(var_InterfaceImplementation.InterfaceType)));
                }

                // TODO: Fields
                // TODO: Properties
                // TODO: Events
                // TODO: Methods

                return true;
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                _Target = null;

                return false;
            }

        }

        #endregion

        // FieldDefinition
        #region FieldDefinition

        /// <summary>
        /// Tries to clone a field. It will NOT be added to some type!
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_TargetName"></param>
        /// <param name="_Target"></param>
        /// <returns></returns>
        public static bool TryToCloneField(AssemblyDefinition _TargetAssemblyDefinition, FieldDefinition _Source, String _TargetName, out FieldDefinition _Target)
        {
            try
            {
                // Resolve FieldType
                TypeReference var_FieldType = _Source.FieldType;

                try
                {
                    var_FieldType = _TargetAssemblyDefinition.MainModule.ImportReference(_Source.FieldType);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());
                }

                // Create new FieldDefinition
                _Target = new FieldDefinition(_TargetName, _Source.Attributes, var_FieldType);

                // Clone custom attributes.
                foreach (CustomAttribute var_CustomAttribute in _Source.CustomAttributes)
                {
                    _Target.CustomAttributes.Add(CloneCustomAttribute(_TargetAssemblyDefinition, var_CustomAttribute));
                }

                return true;
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                _Target = null;

                return false;
            }
        }

        #endregion

        // MethodDefinition
        #region MethodDefinition

        /// <summary>
        /// Tries to clone a method. It will NOT be added to some type!
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_TargetName"></param>
        /// <param name="_Target"></param>
        /// <returns></returns>
        public static bool TryToCloneMethod(AssemblyDefinition _TargetAssemblyDefinition, MethodDefinition _Source, String _TargetName, out MethodDefinition _Target)
        {
            try
            {
                // Import the return type to the _TargetAssemblyDefinition.
                TypeReference var_ReturnType = _Source.ReturnType;
                try
                {
                    var_ReturnType = _TargetAssemblyDefinition.MainModule.ImportReference(_Source.ReturnType);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());
                }

                // Create a new method.
                _Target = new MethodDefinition(_TargetName, _Source.Attributes, var_ReturnType);

                // Copy Layout
                // TODO: Why not this? Why is this the same:  c.MetadataToken = c.MetadataToken; // Is somehow the internal id or so?!
                _Target.ImplAttributes = _Source.ImplAttributes;
                _Target.PInvokeInfo = _Source.PInvokeInfo;
                _Target.IsPreserveSig = _Source.IsPreserveSig;
                _Target.IsPInvokeImpl = _Source.IsPInvokeImpl;
                _Target.CallingConvention = _Source.CallingConvention;

                // Clone custom attributes.
                foreach (CustomAttribute var_CustomAttribute in _Source.CustomAttributes)
                {
                    _Target.CustomAttributes.Add(CloneCustomAttribute(_TargetAssemblyDefinition, var_CustomAttribute));
                }

                // TODO:
                /*foreach (MethodReference @override in o.Overrides)
                    c.Overrides.Add(@override);*/

                // Process
                CloneMethodGenericParameter(_TargetAssemblyDefinition, _Source, _Target);
                CloneMethodVariables(_TargetAssemblyDefinition, _Source, _Target);
                CloneMethodParameters(_TargetAssemblyDefinition, _Source, _Target);
                CloneMethodInstructions(_TargetAssemblyDefinition, _Source, _Target);

                return true;
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                _Target = null;

                return false;
            }
        }

        /// <summary>
        /// Clones the GenericParameters from _Source to _Target.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_Target"></param>
        private static void CloneMethodGenericParameter(AssemblyDefinition _TargetAssemblyDefinition, MethodDefinition _Source, MethodDefinition _Target)
        {
            if (_Source.GenericParameters == null)
            {
                return;
            }

            foreach (GenericParameter var_GenericParameter in _Source.GenericParameters)
            {
                _Target.GenericParameters.Add(CloneGenericParameter(_TargetAssemblyDefinition, _Target, var_GenericParameter));
            }
        }

        /// <summary>
        /// Clones the variables from _Source to _Target.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_Target"></param>
        private static void CloneMethodVariables(AssemblyDefinition _TargetAssemblyDefinition, MethodDefinition _Source, MethodDefinition _Target)
        {
            // Check for body.
            if (_Source.Body == null)
            {
                return;
            }

            // Check for variables.
            if (_Source.Body.Variables == null)
            {
                return;
            }

            // Iterate _Source variables and add to _Target.
            foreach (VariableDefinition var_Variable in _Source.Body.Variables)
            {
                try
                {
                    VariableDefinition var_ImportedVariable = new VariableDefinition(_TargetAssemblyDefinition.MainModule.ImportReference(var_Variable.VariableType));
                    _Target.Body.Variables.Add(var_ImportedVariable);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                    _Target.Body.Variables.Add(var_Variable);
                }
            }
        }

        /// <summary>
        /// Clone the method paramter from _Source to _Target.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_Target"></param>
        private static void CloneMethodParameters(AssemblyDefinition _TargetAssemblyDefinition, MethodDefinition _Source, MethodDefinition _Target)
        {
            foreach (ParameterDefinition var_Parameter in _Source.Parameters)
            {
                // Clone Parameter.
                ParameterDefinition var_ClonedParameterDefinition = CloneParameterDefinition(_TargetAssemblyDefinition, var_Parameter);

                // Add to method!
                _Target.Parameters.Add(var_ClonedParameterDefinition);
            }
        }

        /// <summary>
        /// Clones all the Instruction from _Source to _Target.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <param name="_Target"></param>
        private static void CloneMethodInstructions(AssemblyDefinition _TargetAssemblyDefinition, MethodDefinition _Source, MethodDefinition _Target)
        {
            if (_Source.Body == null)
            {
                return;
            }

            Dictionary<Instruction, Instruction[]> var_SwitchDictionary = new Dictionary<Instruction, Instruction[]>();
            Dictionary<Instruction, Instruction> var_BranchDictionary = new Dictionary<Instruction, Instruction>();
            IDictionary<int, Instruction> var_OffsetDictionary = new Dictionary<int, Instruction>();

            ILProcessor var_TargetILProcessor = _Target.Body.GetILProcessor();

            for (int i = 0; i < _Source.Body.Instructions.Count; i++)
            {
                // Current iterated Instruction in the source method.
                Instruction var_CurrentInstruction = _Source.Body.Instructions[i];

                // Cloned Instruction.
                Instruction var_ProcessedInstruction = CloneMethodInstruction(_TargetAssemblyDefinition, var_CurrentInstruction);

                // Instruction have a offset (position in method body).
                var_ProcessedInstruction.Offset = var_CurrentInstruction.Offset;

                // The offset dictionary maps each Offset to Instruction. Used to adjust later switch and branches.
                var_OffsetDictionary[var_CurrentInstruction.Offset] = var_ProcessedInstruction;

                // Instruction is a switch. Map switch Instruction to all switches instructions.
                if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    var_SwitchDictionary[var_ProcessedInstruction] = (Instruction[])var_ProcessedInstruction.Operand;
                }
                // Instruction is a branch. Map branch Instruction to target instruction.
                else if (var_CurrentInstruction.OpCode.OperandType == OperandType.InlineBrTarget
                    || var_CurrentInstruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                {
                    var_BranchDictionary[var_ProcessedInstruction] = (Instruction)var_CurrentInstruction.Operand;
                }

                // Instruction is a Variable, remap Variable.
                if (var_CurrentInstruction.OpCode.OperandType == OperandType.ShortInlineVar 
                    || var_CurrentInstruction.OpCode.OperandType == OperandType.InlineVar)
                {
                    // Find Variable in original method.
                    int var_VariableIndex = -1;

                    for(int v = 0; v < _Source.Body.Variables.Count; v++)
                    {
                        if(_Source.Body.Variables[v] == (VariableDefinition)var_CurrentInstruction.Operand)
                        {
                            var_VariableIndex = v;
                            break;
                        }
                    }

                    // Find Variable in cloned / target method.
                    if (var_VariableIndex != -1)
                    {
                        // If found. Assign Variable from _Target Body.
                        var_ProcessedInstruction.Operand = _Target.Body.Variables[var_VariableIndex];
                    }
                    // Else just reuse the Variable.
                }

                // Instruction is a Parameter, remap Parameter.
                if (var_CurrentInstruction.OpCode.OperandType == OperandType.ShortInlineArg 
                    || var_CurrentInstruction.OpCode.OperandType == OperandType.InlineArg)
                {
                    // Find Parameter in original method.
                    int var_ParameterIndex = -1;

                    for (int p = 0; p < _Source.Parameters.Count; p++)
                    {
                        if (_Source.Parameters[p] == (ParameterDefinition)var_CurrentInstruction.Operand)
                        {
                            var_ParameterIndex = p;
                            break;
                        }
                    }

                    // Find Parameter in cloned / target method.
                    if (var_ParameterIndex != -1)
                    {
                        // If found. Assign Parameter from _Target Body.
                        var_ProcessedInstruction.Operand = _Target.Parameters[var_ParameterIndex];
                    }
                    // Else just reuse the Parameter.
                }

                var_TargetILProcessor.Append(var_ProcessedInstruction);
            }

            // Now iterate all switches and reset targets based on offsets.
            foreach (var var_Pair in var_SwitchDictionary)
            {
                Instruction var_Switch_Instruction = var_Pair.Key;
                Instruction[] var_Target_Instruction_Array = var_Pair.Value;

                for (int t = 0; t < var_Target_Instruction_Array.Length; t++)
                {
                    if (var_OffsetDictionary.ContainsKey(var_Target_Instruction_Array[t].Offset))
                    {
                        ((Instruction[])var_Switch_Instruction.Operand)[t] = var_OffsetDictionary[var_Target_Instruction_Array[t].Offset];
                    }
                }
            }

            // Now iterate all branches and reset target based on offsets.
            foreach (var var_Pair in var_BranchDictionary)
            {
                Instruction var_Branch_Instruction = var_Pair.Key;
                Instruction var_Target_Instruction = var_Pair.Value;

                if (var_OffsetDictionary.ContainsKey(var_Target_Instruction.Offset))
                {
                    var_Branch_Instruction.Operand = var_OffsetDictionary[var_Target_Instruction.Offset];
                }
            }

            // Adjustments.
            _Target.Body.InitLocals = _Source.Body.InitLocals;
            _Target.Body.MaxStackSize = _Source.Body.MaxStackSize;
            _Target.Body.LocalVarToken = _Source.Body.LocalVarToken;

            // Add Exception Handler.
            if (_Source.Body.HasExceptionHandlers)
            {
                foreach (ExceptionHandler exceptionHandler in _Source.Body.ExceptionHandlers)
                {
                    ExceptionHandler item = CloneMethodExceptionHandler(_Source, _Target, exceptionHandler);
                    _Target.Body.ExceptionHandlers.Add(item);
                }
            }
        }

        /// <summary>
        /// Clones the _Instruction in _TargetAssemblyDefinition context.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition"></param>
        /// <param name="_Instruction"></param>
        /// <returns></returns>
        private static Instruction CloneMethodInstruction(AssemblyDefinition _TargetAssemblyDefinition, Instruction _Instruction)
        {
            // No Operand
            if (_Instruction.OpCode.OperandType == OperandType.InlineNone)
            {
                return Instruction.Create(_Instruction.OpCode);
            }

            // Has Operand

            // (InlineField/InlineMethod/InlineType) Metadata tokens cross-reference other metadata tables and heaps. Tokens are 4-byte unsigned integers and a combination of the table identifier and RID

            // Field
            if (_Instruction.OpCode.OperandType == OperandType.InlineField)
            {
                return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((FieldReference)_Instruction.Operand));
            }

            // Method

            // Calls the method indicated on the evaluation stack (as a pointer to an entry point) with arguments described by a calling convention.
            if (_Instruction.OpCode.Code == Code.Calli)
            {
                return Instruction.Create(_Instruction.OpCode, (CallSite)_Instruction.Operand);
            }

            if (_Instruction.OpCode.OperandType == OperandType.InlineMethod)
            {
                return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((MethodReference)_Instruction.Operand));
            }

            // Type
            if (_Instruction.OpCode.OperandType == OperandType.InlineType)
            {
                return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((TypeReference)_Instruction.Operand));
            }

            // Values

            // Int/float/Double/....
            if (_Instruction.OpCode.OperandType == OperandType.ShortInlineI)
            {
                if (_Instruction.OpCode.Code == Code.Ldc_I4_S)
                {
                    return Instruction.Create(_Instruction.OpCode, (sbyte)_Instruction.Operand);
                }
                return Instruction.Create(_Instruction.OpCode, (byte)_Instruction.Operand);
            }
            if (_Instruction.OpCode.OperandType == OperandType.InlineI)
            {
                return Instruction.Create(_Instruction.OpCode, (int)_Instruction.Operand);
            }
            if (_Instruction.OpCode.OperandType == OperandType.InlineI8)
            {
                return Instruction.Create(_Instruction.OpCode, (long)_Instruction.Operand);
            }
            if (_Instruction.OpCode.OperandType == OperandType.ShortInlineR)
            {
                return Instruction.Create(_Instruction.OpCode, (float)_Instruction.Operand);
            }
            if (_Instruction.OpCode.OperandType == OperandType.InlineR)
            {
                return Instruction.Create(_Instruction.OpCode, (double)_Instruction.Operand);
            }

            // String
            if (_Instruction.OpCode.OperandType == OperandType.InlineString)
            {
                return Instruction.Create(_Instruction.OpCode, (string)((string)_Instruction.Operand).Clone());
            }

            // Branching
            if (_Instruction.OpCode.OperandType == OperandType.InlineBrTarget || _Instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
            {
                // Note: Operand will be later reassigned! So no need to create the operand instruction here.
                return Instruction.Create(_Instruction.OpCode, (Instruction)_Instruction.Operand);
            }
            if (_Instruction.OpCode.OperandType == OperandType.InlineSwitch)
            {
                // Note: Operand will be later reassigned! So no need to create the operand instructions here.
                Instruction[] var_SourceSwitchInstructions = (Instruction[])_Instruction.Operand;
                Instruction[] var_TargetSwitchInstructions = new Instruction[var_SourceSwitchInstructions.Length];
                Array.Copy(var_SourceSwitchInstructions, var_TargetSwitchInstructions, var_SourceSwitchInstructions.Length);

                return Instruction.Create(_Instruction.OpCode, var_TargetSwitchInstructions);
            }

            // Variable
            if (_Instruction.OpCode.OperandType == OperandType.ShortInlineVar || _Instruction.OpCode.OperandType == OperandType.InlineVar)
            {
                // Note: Operand will be later reassigned! 
                VariableDefinition variable = (VariableDefinition)_Instruction.Operand;
                Instruction instruction = Instruction.Create(_Instruction.OpCode, variable);
                return instruction;
            }

            // Parameter
            if (_Instruction.OpCode.OperandType == OperandType.ShortInlineArg || _Instruction.OpCode.OperandType == OperandType.InlineArg)
            {
                // Note: Operand will be later reassigned! 
                ParameterDefinition parameter = (ParameterDefinition)_Instruction.Operand;
                return Instruction.Create(_Instruction.OpCode, parameter);
            }

            // The operand is a FieldRef, MethodRef, or TypeRef token. Need all of them! Example: for type typeof(...), for fields while array init.
            if (_Instruction.OpCode.OperandType == OperandType.InlineTok) /* Reference Token */
            {
                if (_Instruction.Operand.GetType() == typeof(FieldDefinition))
                {
                    return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((FieldDefinition)_Instruction.Operand));
                }
                if (_Instruction.Operand.GetType() == typeof(FieldReference))
                {
                    return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((FieldReference)_Instruction.Operand));
                }
                if (_Instruction.Operand.GetType() == typeof(MethodDefinition))
                {
                    return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((MethodDefinition)_Instruction.Operand));
                }
                if (_Instruction.Operand.GetType() == typeof(MethodReference))
                {
                    MethodReference var_MethodReference = (MethodReference)_Instruction.Operand;

                    if (var_MethodReference.HasGenericParameters || var_MethodReference.IsGenericInstance || var_MethodReference.ContainsGenericParameter)
                    {
                        return Instruction.Create(OpCodes.Nop);
                    }

                    return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference(var_MethodReference));
                }
                if (_Instruction.Operand.GetType() == typeof(TypeDefinition))
                {
                    return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((TypeDefinition)_Instruction.Operand));
                }

                return Instruction.Create(_Instruction.OpCode, _TargetAssemblyDefinition.MainModule.ImportReference((TypeReference)_Instruction.Operand));
            }
            return null;
        }

        /// <summary>
        /// Clones and returns a _SourceExceptionHandler.
        /// </summary>
        /// <param name="_Source"></param>
        /// <param name="_Target"></param>
        /// <param name="_SourceExceptionHandler"></param>
        /// <returns></returns>
        private static ExceptionHandler CloneMethodExceptionHandler(MethodDefinition _Source, MethodDefinition _Target, ExceptionHandler _SourceExceptionHandler)
        {
            ExceptionHandler var_ExceptionHandler = new ExceptionHandler(_SourceExceptionHandler.HandlerType);
            var_ExceptionHandler.CatchType = _SourceExceptionHandler.CatchType;
            if (_SourceExceptionHandler.TryEnd != null)
            {
                var_ExceptionHandler.TryEnd = _Target.Body.Instructions[_Source.Body.Instructions.IndexOf(_SourceExceptionHandler.TryEnd)];
            }
            if (_SourceExceptionHandler.TryStart != null)
            {
                var_ExceptionHandler.TryStart = _Target.Body.Instructions[_Source.Body.Instructions.IndexOf(_SourceExceptionHandler.TryStart)];
            }
            if (_SourceExceptionHandler.HandlerEnd != null)
            {
                var_ExceptionHandler.HandlerEnd = _Target.Body.Instructions[_Source.Body.Instructions.IndexOf(_SourceExceptionHandler.HandlerEnd)];
            }
            if (_SourceExceptionHandler.HandlerStart != null)
            {
                var_ExceptionHandler.HandlerStart = _Target.Body.Instructions[_Source.Body.Instructions.IndexOf(_SourceExceptionHandler.HandlerStart)];
            }
            if (_SourceExceptionHandler.FilterStart != null)
            {
                var_ExceptionHandler.FilterStart = _Target.Body.Instructions[_Source.Body.Instructions.IndexOf(_SourceExceptionHandler.FilterStart)];
            }
            return var_ExceptionHandler;
        }

        #endregion

        // Parameter
        #region Parameter

        /// <summary>
        /// Clone the given parameter definition.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition">Target assembly.</param>
        /// <param name="_ParameterDefinition">The original parameter definition.</param>
        /// <returns>A clone of the original parameter definition.</returns>
        public static ParameterDefinition CloneParameterDefinition(AssemblyDefinition _TargetAssemblyDefinition, ParameterDefinition _ParameterDefinition)
        {
            // Import ParameterType.
            TypeReference var_ParameterType = _ParameterDefinition.ParameterType;
            try
            {
                var_ParameterType = _TargetAssemblyDefinition.MainModule.ImportReference(_ParameterDefinition.ParameterType);
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());
            }

            // Create new Parameter.
            ParameterDefinition var_ParameterDefinition = new ParameterDefinition(_ParameterDefinition.Name, _ParameterDefinition.Attributes, var_ParameterType)
            {
                IsIn = _ParameterDefinition.IsIn,
                IsLcid = _ParameterDefinition.IsLcid,
                IsOptional = _ParameterDefinition.IsOptional,
                IsOut = _ParameterDefinition.IsOut,
                IsReturnValue = _ParameterDefinition.IsReturnValue,
                MarshalInfo = _ParameterDefinition.MarshalInfo
            };

            // Add constant value, if has some.
            if (_ParameterDefinition.HasConstant)
                var_ParameterDefinition.Constant = _ParameterDefinition.Constant;

            // Clone custom attributes.
            foreach (CustomAttribute var_CustomAttribute in _ParameterDefinition.CustomAttributes)
            {
                var_ParameterDefinition.CustomAttributes.Add(CloneCustomAttribute(_TargetAssemblyDefinition, var_CustomAttribute));
            }

            return var_ParameterDefinition;
        }

        #endregion

        // CustomAttribute
        #region CustomAttribute

        /// <summary>
        /// Clone the given custom attribute.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition">Target assembly</param>
        /// <param name="_CustomAttribute">The original custom attribute.</param>
        /// <returns>A clone of the original custom attribute.</returns>
        public static CustomAttribute CloneCustomAttribute(AssemblyDefinition _TargetAssemblyDefinition, CustomAttribute _CustomAttribute)
        {
            // Import constructor
            MethodReference var_ConstructorReference = _TargetAssemblyDefinition.MainModule.ImportReference(_CustomAttribute.Constructor);

            // Create new attribute
            CustomAttribute var_NewCustomAttribute = new CustomAttribute(var_ConstructorReference);

            // Clone arguments
            foreach (CustomAttributeArgument attribArg in _CustomAttribute.ConstructorArguments)
            {
                var_NewCustomAttribute.ConstructorArguments.Add(new CustomAttributeArgument(
                    _TargetAssemblyDefinition.MainModule.ImportReference(attribArg.Type), 
                    attribArg.Value));
            }

            // Clone fields
            foreach (CustomAttributeNamedArgument attribArg in _CustomAttribute.Fields)
            {
                var_NewCustomAttribute.Fields.Add(new CustomAttributeNamedArgument(attribArg.Name,
                    new CustomAttributeArgument(
                        _TargetAssemblyDefinition.MainModule.ImportReference(attribArg.Argument.Type), 
                        attribArg.Argument.Value))
                );
            }

            // Clone properties
            foreach (CustomAttributeNamedArgument attribArg in _CustomAttribute.Properties)
            {
                var_NewCustomAttribute.Properties.Add(new CustomAttributeNamedArgument(attribArg.Name,
                    new CustomAttributeArgument(
                        _TargetAssemblyDefinition.MainModule.ImportReference(attribArg.Argument.Type), 
                        attribArg.Argument.Value))
                );
            }

            return var_NewCustomAttribute;
        }

        #endregion

        // GenericParameter
        #region GenericParameter

        /// <summary>
        /// Clone the given generic parameter.
        /// </summary>
        /// <param name="_TargetAssemblyDefinition">Target assembly.</param>
        /// <param name="_Target">Target parameter owner.</param>
        /// <param name="_GenericParameter">The original generic parameter.</param>
        /// <returns>A clone of the original generic parameter.</returns>
        public static GenericParameter CloneGenericParameter(AssemblyDefinition _TargetAssemblyDefinition, IGenericParameterProvider _Target, GenericParameter _GenericParameter)
        {
            GenericParameter var_GenericParameter = new GenericParameter(_GenericParameter.Name, _Target)
            {
                Attributes = _GenericParameter.Attributes,
                Owner = _GenericParameter.Owner,
                Position = _GenericParameter.Position,
                Type = _GenericParameter.Type,
            };

            // Note: TypeReference in cecil 0.10, GenericParameterConstraint in cecil 0.11
            foreach (GenericParameterConstraint var_Constraint in _GenericParameter.Constraints)
            {
                // Import Constraint.
                try
                {
                    GenericParameterConstraint var_ClonedConstraint = new GenericParameterConstraint(_TargetAssemblyDefinition.MainModule.ImportReference(var_Constraint.ConstraintType));
                    
                    var_GenericParameter.Constraints.Add(var_ClonedConstraint);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                    var_GenericParameter.Constraints.Add(var_Constraint);
                }
            }
            return var_GenericParameter;
        }

        #endregion
    }
}
