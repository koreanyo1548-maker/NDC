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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Project
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    public class EventCache : AMemberGroupCache<EventDefinition, EventKey>
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the EventCache using the assemblies from _Step.
        /// </summary>
        /// <param name="_Step"></param>
        public EventCache(PostAssemblyBuildStep _Step)
            : base(_Step)
        {
            // Add members.
            foreach (AssemblyInfo var_AssemblyInfo in this.Step.DataContainer.ObfuscateAssemblyList)
            {
                foreach (TypeDefinition var_TypeDefinition in var_AssemblyInfo.GetAllTypeDefinitions())
                {
                    // Iterate all member in _TypeDefinition and add.
                    foreach (EventDefinition var_MemberDefinition in var_TypeDefinition.Events)
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

        protected override bool IsVirtual(EventDefinition _MemberDefinition)
        {
            if ((_MemberDefinition.AddMethod != null && _MemberDefinition.AddMethod.IsVirtual)
                    || (_MemberDefinition.RemoveMethod != null && _MemberDefinition.RemoveMethod.IsVirtual))
            {
                return true;
            }

            return false;
        }

        protected override bool MembersMatch(EventDefinition _Left, EventDefinition _Right)
        {
            return Match.EventMatchHelper.EventMatch(_Left, _Right)
                    || Match.EventMatchHelper.EventMatch(_Right, _Left);
        }

        #endregion
    }
}
