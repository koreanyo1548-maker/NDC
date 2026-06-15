using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member
{
    /// <summary>
    /// Inherit from this component if you want to obfuscate members.
    /// TMemberReference is the type of reference this component works with. The ones getting renamed too.
    /// TMemberKey the type of key used to identify the references.
    /// </summary>
    /// <typeparam name="TMemberReference"></typeparam>
    /// <typeparam name="TMemberKey"></typeparam>
    public abstract class AMemberObfuscationComponent<TMemberReference, TMemberKey> : APostAssemblyBuildComponent, IGuiComponent, IMemberObfuscationComponent
        where TMemberReference : MemberReference
        where TMemberKey : IMemberKey
    {
        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public abstract String SettingsKey { get; }

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
                return EObfuscatorCategory.Obfuscation;
            }
        }

        /// <summary>
        /// Shown gui in the obfuscator settings window.
        /// </summary>
        public abstract ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings);

        #endregion

        // Analyse A
        #region Analyse A

        /// <summary>
        /// Contains a mapping of all member references inside the to obfuscate assemblies.
        /// </summary>
        public MemberReferenceMapping<TMemberReference, TMemberKey> MemberReferenceMapping { get; private set; } = new MemberReferenceMapping<TMemberReference, TMemberKey>();

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Analyse Member...");

            // Get all references and add them to the mapping.

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find MemberReferences...");

            List<TMemberReference> var_MemberReferenceList = this.OnAnalyse_A_FindMemberReferences(_AssemblyInfo);

            for (int m = 0; m < var_MemberReferenceList.Count; m++)
            {
                if(var_MemberReferenceList[m] == null)
                {
                    continue;
                }

                this.OnAnalyse_A_ResolveAndAddToMapping(var_MemberReferenceList[m]);
            }

            // Get all members to not rename!

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Member to skip...");

            this.OnAnalyse_A_FindMemberToNotRename(_AssemblyInfo);

            return true;
        }

        /// <summary>
        /// Add a _MemberReference to the memberReferenceMapping if _MemberReference can be resolved and is in to obfuscate assemblies.
        /// </summary>
        /// <param name="_MemberReference"></param>
        private void OnAnalyse_A_ResolveAndAddToMapping(TMemberReference _MemberReference)
        {
            if (_MemberReference == null)
            {
                throw new ArgumentNullException("_MemberReference");
            }

            // The declaring type.
            TypeReference var_DeclaringType = _MemberReference is TypeReference ? _MemberReference as TypeReference : _MemberReference.DeclaringType;

            if (!this.Pipeline.Step.IsTypeInObfuscateAssemblies(var_DeclaringType))
            {
                //Is a reference but not part of the too obfuscate assemblies.
                return;
            }

            try
            {
                // Create a member key for this reference.
                if(!MemberKeyHelper.TryToCreateMemberKey(_MemberReference, out IMemberKey _MemberKey))
                {
                    // Failed!

                    return;
                }

                // Prove again if _MemberKey is a TMemberKey and so not null!
                if (_MemberKey is TMemberKey)
                {
                    //Add resolved Reference to the mapping.
                    this.MemberReferenceMapping.Add(_MemberReference, (TMemberKey)_MemberKey);
                }
                else
                {
                    // Something is wrong here! Should never happen!!!
                }
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Cannot resolve reference " + MemberReferenceHelper.GetExtendedFullName(_MemberReference));
            }
        }

        /// <summary>
        /// Return all the references this component will obfuscate too.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        protected abstract List<TMemberReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo);

        /// <summary>
        /// Mark the Members to not rename here.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        protected abstract bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo);

        #endregion

        // Analyse B
        #region Analyse B

        public virtual bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Find Names
        #region Find Names

        /// <summary>
        /// Find names for the member to obfuscate.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public abstract bool OnFindMemberNames_Assemblies(AssemblyInfo _AssemblyInfo);

        #endregion

        // Obfuscate
        #region Obfuscate

        /// <summary>
        /// Obfuscate the member and references with the previous found names.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public abstract bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo);

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public virtual bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion
    }
}
