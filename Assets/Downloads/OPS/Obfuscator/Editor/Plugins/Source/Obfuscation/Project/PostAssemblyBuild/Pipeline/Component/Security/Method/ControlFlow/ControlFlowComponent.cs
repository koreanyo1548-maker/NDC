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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;
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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security.Method.ControlFlow
{
    public class ControlFlowComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Method ControlFlow";
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
                return "Activate this setting to obfuscate the methods control flow. The control flow is how the computer reads and processes a methods instructions. Mostly top-down. Control flow obfuscation scrambles the instructions order and makes the method very difficult to read for a human. (Mono build only)";
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
                return "Obfuscate the methods control flow. (Beta, and requires .Net >= 5 or .Net Standard >= 2.1)";
            }
        }

#endregion

        // Settings
#region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Method_ControlFlow";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CEnable_Method_ControlFlow = "Enable_Method_ControlFlow";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Method_ControlFlow);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Info
            Row_Text var_InfoRow = new Row_Text("");
            var_InfoRow.Notification_Info = this.Description;
            var_Content.AddRow(var_InfoRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, true);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Check Assembly
        #region Check Assembly

        /// <summary>
        /// True: The _AssemblyLoadInfo should be processed by this component.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public override bool CanBeProcessedByComponent(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            if (!base.CanBeProcessedByComponent(_AssemblyLoadInfo))
            {
                return false;
            }

            // Only Mono Support.
            if(this.Step.BuildSettings.IsIL2CPPBuild)
            {
                return false;
            }

            return true;
        }

        #endregion

        // OnAnalyse
        #region OnAnalyse

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

    #endregion

        // OnPostAnalyse
        #region OnPostAnalyse

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        private System.Random random = new Random();

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
#else
            // Check if obfuscation got activated.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Method_ControlFlow))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The method control flow obfuscation got skipped by your settings!");

                return true;
            }

            this.OnObfuscate_Modify_ControlFlow(_AssemblyInfo);
#endif
            return true;
        }

#if Obfuscator_Free
#else
        private void OnObfuscate_Modify_ControlFlow(AssemblyInfo _AssemblyInfo)
        {
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if (this.OnObfuscate_Skip_Type_Modify_ControlFlow(var_TypeDefinition))
                {
                    continue;
                }

                this.OnObfuscate_Modify_ControlFlow(_AssemblyInfo, var_TypeDefinition);
            }
        }

        /// <summary>
        /// True: Skip _TypeDefinition for random code generation.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private bool OnObfuscate_Skip_Type_Modify_ControlFlow(TypeDefinition _TypeDefinition)
        {
            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotObfuscateMethodBodyAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotObfuscateMethodBody Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotUseClassForFakeCodeAttribute).Name))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotUseClassForFakeCode Attribute, so skip Random Code creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            if (!_TypeDefinition.HasMethods)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has not method, so skip method control flow obfuscation creation!", _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>())));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a list of all control flow protect able methods in _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private List<MethodDefinition> OnObfuscate_Helper_Get_Method_List(TypeDefinition _TypeDefinition)
        {
            List<MethodDefinition> var_MethodList = new List<MethodDefinition>();

            for (int i = 0; i < _TypeDefinition.Methods.Count; i++)
            {
                // Continue only methods with body.
                if (!_TypeDefinition.Methods[i].HasBody)
                {
                    continue;
                }

                // Continue only if not constructor.
                if (_TypeDefinition.Methods[i].IsConstructor)
                {
                    continue;
                }

                // Continue only if not extern method.
                if (_TypeDefinition.Methods[i].HasPInvokeInfo)
                {
                    continue;
                }

                // Continue property methods.
                // TODO: Better solution.
                // TODO: Event methods too?
                if (_TypeDefinition.Methods[i].Name.StartsWith("get_") || _TypeDefinition.Methods[i].Name.StartsWith("set_"))
                {
                    continue;
                }

                // Attribute
                if (AttributeHelper.HasCustomAttribute(_TypeDefinition.Methods[i], "DoNotObfuscateMethodBody"))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("{0} has DoNotObfuscateMethodBody Attribute, so skip method control flow obfuscation creation!", _TypeDefinition.Methods[i].GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>())));

                    continue;
                }

                var_MethodList.Add(_TypeDefinition.Methods[i]);
            }

            return var_MethodList;
        }

        /// <summary>
        /// Clone methods in _TypeDefinition and randomize instructions.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        private void OnObfuscate_Modify_ControlFlow(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            // Find protect able methods!
            List<MethodDefinition> var_ProtectAbleMethods = this.OnObfuscate_Helper_Get_Method_List(_TypeDefinition);

            // Continue only if has protect able methods.
            if (var_ProtectAbleMethods.Count == 0)
            {
                return;
            }

            // Protect all found.
            for (int i = 0; i < var_ProtectAbleMethods.Count; i++)
            {
                this.OnObfuscate_Modify_ControlFlow(var_ProtectAbleMethods[i]);
            }
        }

        private void OnObfuscate_Modify_ControlFlow(MethodDefinition _MethodDefinition)
        {
            _MethodDefinition.Body.SimplifyMacros();

            List<Block> var_BlockList = BlockParser.ParseMethod(_MethodDefinition);
            var_BlockList = OnObfuscate_Helper_Randomize(var_BlockList);
            _MethodDefinition.Body.Instructions.Clear();

            VariableDefinition var_Iterator_VariableDefinition = new VariableDefinition(_MethodDefinition.Module.TypeSystem.Int32);

            _MethodDefinition.Body.Variables.Add(var_Iterator_VariableDefinition);
            Instruction var_Target = Instruction.Create(OpCodes.Nop);
            Instruction var_BranchInstruction = Instruction.Create(OpCodes.Br, var_Target);

            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, var_BlockList.Single(x => x.Number == 0).RandomIdentifier));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, var_Iterator_VariableDefinition));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Br, var_BranchInstruction));
            _MethodDefinition.Body.Instructions.Add(var_Target);

            foreach (Block block in var_BlockList)
            {
                if (block != var_BlockList.Single(x => x.Number == var_BlockList.Count - 1))
                {
                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, var_Iterator_VariableDefinition));

                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, block.RandomIdentifier));

                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));

                    Instruction var_Block_Target = Instruction.Create(OpCodes.Nop);
                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, var_Block_Target));

                    foreach (Instruction instruction in block.Instructions)
                        _MethodDefinition.Body.Instructions.Add(instruction);

                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, var_BlockList.Single(x => x.Number == block.Number + 1).RandomIdentifier));

                    _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, var_Iterator_VariableDefinition));
                    _MethodDefinition.Body.Instructions.Add(var_Block_Target);
                }
            }
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, var_Iterator_VariableDefinition));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, var_BlockList.Single(x => x.Number == var_BlockList.Count - 1).RandomIdentifier));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, var_BranchInstruction));
            _MethodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Br, var_BlockList.Single(x => x.Number == var_BlockList.Count - 1).Instructions[0]));
            _MethodDefinition.Body.Instructions.Add(var_BranchInstruction);
            foreach (Instruction lastBlock in var_BlockList.Single(x => x.Number == var_BlockList.Count - 1).Instructions)
                _MethodDefinition.Body.Instructions.Add(lastBlock);

            _MethodDefinition.Body.OptimizeMacros();
        }

        private List<Block> OnObfuscate_Helper_Randomize(List<Block> input)
        {
            List<Block> var_ReturnList = new List<Block>();
            foreach (var group in input)
                var_ReturnList.Insert(this.random.Next(0, var_ReturnList.Count), group);
            return var_ReturnList;
        }

#endif

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