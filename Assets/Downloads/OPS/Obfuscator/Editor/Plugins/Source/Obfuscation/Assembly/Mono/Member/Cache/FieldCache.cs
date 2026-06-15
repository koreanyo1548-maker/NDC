using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Mdb;
using OPS.Mono.Cecil.Pdb;

// OPS - Obfuscator - Assemby
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assemby - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Project
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    public class FieldCache : AMemberCache<FieldDefinition, FieldKey>
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the FieldCache using the assemblies from _Step.
        /// </summary>
        /// <param name="_Step"></param>
        public FieldCache(PostAssemblyBuildStep _Step)
            :base(_Step)
        {
            // Add members.
            foreach (AssemblyInfo var_AssemblyInfo in this.Step.DataContainer.ObfuscateAssemblyList)
            {
                foreach (TypeDefinition var_TypeDefinition in var_AssemblyInfo.GetAllTypeDefinitions())
                {
                    // Iterate all member in _TypeDefinition and add.
                    foreach (FieldDefinition var_MemberDefinition in var_TypeDefinition.Fields)
                    {
                        this.Add_MemberDefinition(var_MemberDefinition);
                    }
                }
            }
        }

        #endregion
    }
}
