using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly
{
    /// <summary>
    /// Used to resolve all available assemblies.
    /// </summary>
    public class AssemblyResolver : DefaultAssemblyResolver
    {
        /// <summary>
        /// Registers an Assembly directly to be searchable for resolving.
        /// </summary>
        /// <param name="_Assembly"></param>
        public new void RegisterAssembly(AssemblyDefinition _Assembly)
        {
            base.RegisterAssembly(_Assembly);
        }
    }
}
