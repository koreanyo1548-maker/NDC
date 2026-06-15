using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

#if Obfuscator_Free
#else

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security.Method.ControlFlow
{
    /// <summary>
    /// Represents a block of instructions in the control flow process.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// The instructions in the block.
        /// </summary>
        public List<Instruction> Instructions { get; set; } = new List<Instruction>();

        /// <summary>
        /// The block ordered number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Randomizer.
        /// </summary>
        private Random random = new Random();

        /// <summary>
        /// Represents a random index from int - max to max.
        /// </summary>
        private int randomIdentifier;

        /// <summary>
        /// Returns a random index from int - max to max.
        /// </summary>
        public int RandomIdentifier
        {
            get
            {
                if(this.randomIdentifier == 0)
                {
                    // Min is inclusive, Max is exclusive.
                    this.randomIdentifier = this.random.Next(1, Int32.MaxValue);

                    // Calculate *-1 if random equals 0.
                    if(this.random.Next(0,2) == 0)
                    {
                        this.randomIdentifier *= -1;
                    }
                }

                return this.randomIdentifier;
            }
        }
    }
}
#endif