using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

#if Obfuscator_Free
#else

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security.Method.ControlFlow
{
    /// <summary>
    /// Helper class to parse a method instructions into blocks.
    /// </summary>
    public static class BlockParser
    {
        /// <summary>
        /// Parse _MethodDefinition instructions into a ordered block list.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        /// <returns></returns>
        public static List<Block> ParseMethod(MethodDefinition _MethodDefinition)
        {
            bool var_HasReturnValue = _MethodDefinition.ReturnType.FullName != "System.Void";

            List<Block> blocks = new List<Block>();

            Block block = new Block();
            int Id = 0;
            int usage = 0;
            block.Number = Id;
            block.Instructions.Add(Instruction.Create(OpCodes.Nop));
            blocks.Add(block);
            block = new Block();
            Stack<ExceptionHandler> handlers = new Stack<ExceptionHandler>();

            int stacks = 0;
            int pops = 0;

            int var_Instruction_Count = 0;

            foreach (Instruction instruction in _MethodDefinition.Body.Instructions)
            {
                foreach (var eh in _MethodDefinition.Body.ExceptionHandlers)
                {
                    if (eh.HandlerStart == instruction || eh.TryStart == instruction || eh.FilterStart == instruction)
                        handlers.Push(eh);
                }
                foreach (var eh in _MethodDefinition.Body.ExceptionHandlers)
                {
                    if (eh.HandlerEnd == instruction || eh.TryEnd == instruction)
                        handlers.Pop();
                }
                InstructionHelper.CalculateStackUsage(instruction, var_HasReturnValue, out stacks, out pops);
                block.Instructions.Add(instruction);

                if (instruction == _MethodDefinition.Body.Instructions.Last())
                {
                    block.Number = ++Id;
                    blocks.Add(block);
                    block = new Block();
                }
                else
                {
                    usage += stacks - pops;
                    if (stacks == 0)
                    {
                        if (instruction.OpCode != OpCodes.Nop)
                        {
                            if ((usage == 0 || instruction.OpCode == OpCodes.Ret /*|| instruction.OpCode.OperandType == OperandType.InlineBrTarget || instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget*/) && handlers.Count == 0)
                            {

                                block.Number = ++Id;
                                blocks.Add(block);
                                block = new Block();
                            }
                        }
                    }
                }

                var_Instruction_Count += 1;
            }

            var_Instruction_Count = 0;
            foreach (Block var_Block in blocks)
            {
                foreach (Instruction instruction in var_Block.Instructions)
                {
                    var_Instruction_Count += 1;
                }
            }

            return blocks;
        }

    }
}

#endif