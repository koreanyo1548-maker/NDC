using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

#if Obfuscator_Free
#else

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for IL Instructions.
    /// </summary>
    public static class InstructionHelper
    {
        /// <summary>
		/// Calculates the stack usage.
		/// </summary>
		/// <param name="_Instruction">Instruction to calculate</param>
		/// <param name="pushes">Updated with number of stack pushes</param>
		/// <param name="pops">Updated with number of stack pops or <c>-1</c> if the stack should be cleared.</param>
		public static void CalculateStackUsage(Instruction _Instruction, out int pushes, out int pops) => CalculateStackUsage(_Instruction, false, out pushes, out pops);

        /// <summary>
        /// Calculates the stack usage.
        /// </summary>
		/// <param name="_Instruction">Instruction to calculate</param>
        /// <param name="methodHasReturnValue"><c>true</c> if method has a return value</param>
        /// <param name="pushes">Updated with number of stack pushes</param>
        /// <param name="pops">Updated with number of stack pops or <c>-1</c> if the stack should be cleared.</param>
        public static void CalculateStackUsage(Instruction _Instruction, bool methodHasReturnValue, out int pushes, out int pops)
        {
            var opCode = _Instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Call)
                CalculateStackUsageCall(_Instruction, opCode.Code, out pushes, out pops);
            else
                CalculateStackUsageNonCall(_Instruction, opCode, methodHasReturnValue, out pushes, out pops);
        }

        /// <summary>
        /// Calculates the stack usage for a method call.
        /// </summary>
		/// <param name="_Instruction">Instruction to calculate</param>
        /// <param name="code">The instruction code.</param>
        /// <param name="pushes">Updated with number of stack pushes</param>
        /// <param name="pops">Updated with number of stack pops or <c>-1</c> if the stack should be cleared.</param>
        private static void CalculateStackUsageCall(Instruction _Instruction, Code code, out int pushes, out int pops)
        {
            pushes = 0;
            pops = 0;

            // It doesn't push or pop anything. The stack should be empty when JMP is executed.
            if (code == Code.Jmp)
                return;

            IMethodSignature var_MethodSignature = (IMethodSignature)_Instruction.Operand;

            if (var_MethodSignature is null)
                return;

            bool implicitThis = MethodReferenceHelper.HasImplicitThis(var_MethodSignature);
            if (!(var_MethodSignature.ReturnType.FullName == "System.Void") || (code == Code.Newobj && var_MethodSignature.HasThis))
                pushes++;

            pops += var_MethodSignature.Parameters.Count;
            var paramsAfterSentinel = MethodReferenceHelper.GetParameterAfterSentinel(var_MethodSignature);
            if (paramsAfterSentinel > 0)
            {
                pops += paramsAfterSentinel;
            }
            if (implicitThis && code != Code.Newobj)
                pops++;
            if (code == Code.Calli)
                pops++;
        }

        /// <summary>
        /// Calculates the stack usage for a not method call.
        /// </summary>
		/// <param name="_Instruction">Instruction to calculate</param>
        /// <param name="opCode">The instruction code.</param>
        /// <param name="hasReturnValue"><c>true</c> if method has a return value</param>
        /// <param name="pushes">Updated with number of stack pushes</param>
        /// <param name="pops">Updated with number of stack pops or <c>-1</c> if the stack should be cleared.</param>
        private static void CalculateStackUsageNonCall(Instruction _Instruction, OpCode opCode, bool hasReturnValue, out int pushes, out int pops)
        {
            switch (opCode.StackBehaviourPush)
            {
                case StackBehaviour.Push0:
                    pushes = 0;
                    break;

                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    pushes = 1;
                    break;

                case StackBehaviour.Push1_push1:
                    pushes = 2;
                    break;

                case StackBehaviour.Varpush:    // only call, calli, callvirt which are handled elsewhere
                default:
                    pushes = 0;
                    break;
            }

            switch (opCode.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    pops = 0;
                    break;

                case StackBehaviour.Pop1:
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                    pops = 1;
                    break;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    pops = 2;
                    break;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                //case StackBehaviour.Popref_popi_pop1:
                    pops = 3;
                    break;

                case StackBehaviour.PopAll:
                    pops = -1;
                    break;

                case StackBehaviour.Varpop: // call, calli, callvirt, newobj (all handled elsewhere), and ret
                    if (hasReturnValue)
                        pops = 1;
                    else
                        pops = 0;
                    break;

                default:
                    pops = 0;
                    break;
            }
        }
    }
}

#endif
