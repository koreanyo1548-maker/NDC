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
using OPS.Obfuscator.Editor.Extension;

// OPS - Obfuscator - Project
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    /// <summary>
    /// Contains all methods in the project and its belonging group.
    /// </summary>
    public class MethodCache : AMemberGroupCache<MethodDefinition, MethodKey>
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the MethodCache using the assemblies from _Step.
        /// </summary>
        /// <param name="_Step"></param>
        public MethodCache(PostAssemblyBuildStep _Step)
            : base(_Step)
        {
            // Add members.
            foreach (AssemblyInfo var_AssemblyInfo in this.Step.DataContainer.ObfuscateAssemblyList)
            {
                foreach (TypeDefinition var_TypeDefinition in var_AssemblyInfo.GetAllTypeDefinitions())
                {
                    // Iterate all member in _TypeDefinition and add.
                    foreach (MethodDefinition var_MemberDefinition in var_TypeDefinition.Methods)
                    {
                        this.Add_MemberDefinition(var_MemberDefinition);
                    }

                    // Update group whole for type, to include members which are not directly added in current var_TypeDefinition.
                    this.UpdateMemberGroup(var_TypeDefinition);
                }
            }
        }

        #endregion

        // Group
        #region Group

        /// <summary>
        /// Returns if the method is virtual.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        protected override bool IsVirtual(MethodDefinition _MemberDefinition)
        {
            return _MemberDefinition.IsVirtual;
        }

        /// <summary>
        /// Returns if the _Left and _Right member match.
        /// </summary>
        /// <param name="_Left"></param>
        /// <param name="_Right"></param>
        /// <returns></returns>
        protected override bool MembersMatch(MethodDefinition _Left, MethodDefinition _Right)
        {
            return Match.MethodMatchHelper.MethodMatch(_Left, _Right)
                    || Match.MethodMatchHelper.MethodMatch(_Right, _Left);
        }

        #endregion
    }
}
