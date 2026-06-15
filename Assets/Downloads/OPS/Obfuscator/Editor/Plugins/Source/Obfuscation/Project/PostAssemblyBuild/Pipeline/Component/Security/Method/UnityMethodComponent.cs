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

// OPS - Obfuscator - Components
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;


#if Obfuscator_Use_Beta

// https://groups.google.com/forum/#!topic/mono-cecil/86_KLPvWqQY
// https://github.com/jbevain/cecil/blob/96026325ee1cb6627a3e4a32b924ab2905f02553/Test/Mono.Cecil.Tests/Extensions.cs

/*
namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Security
{
    public class UnityMethodComponent : AObfuscationPipelineComponent, IAssemblyProcessingComponent
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
                return "Protect Unity Methods";
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
                return "";
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
                return "";
            }
        }

#endregion

        // Analyse A
#region Analyse A

        public bool OnAnalyse_A_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

#endregion

        // Analyse B
#region Analyse B

        public bool OnAnalyse_B_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

#endregion

        // Obfuscate
#region Obfuscate

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
#else
            //Check if string obfuscation got activated.
            if (!this.Project.Settings.Get_ComponentSetting_As_Bool(Member.Method.MethodObfuscationComponent.CObfuscation_Component_Method, Member.Method.MethodObfuscationComponent.CObfuscate_Method_Unity))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "The protection of unity methods got skipped by your settings!");

                return true;
            }

            this.OnObfuscate_ProtectUnityMethods(_AssemblyInfo);
#endif
            return true;
        }

#if Obfuscator_Free
#else

        /// <summary>
        /// Random class.
        /// </summary>
        private Random random = new Random();

        private void OnObfuscate_ProtectUnityMethods(AssemblyInfo _AssemblyInfo)
        {
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only MonoBehaviour.
                if(!Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(var_TypeDefinition, this.Project.GetCache<TypeCache>()))
                {
                    continue;
                }

                // Iterate methods.
                foreach(MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    // Continue only types, that can be used for random code generation.
                    if (this.OnObfuscate_Skip_Method_ForProtection(var_MethodDefinition))
                    {
                        continue;
                    }

                    // Clone methods and randomize operands.
                    this.OnObfuscate_ProtectUnityMethod(_AssemblyInfo, var_MethodDefinition);
                }
            }
        }

        /// <summary>
        /// True: Skip method for unity method obfuscation.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        /// <returns></returns>
        private bool OnObfuscate_Skip_Method_ForProtection(MethodDefinition _MethodDefinition)
        {
            // Continue only unity methods.
            if(!this.OnObfuscate_Helper_IsUnityMethod(_MethodDefinition))
            {
                return true;
            }

            return false;
        }

        // Analyse
        // Mark unity methods here as do not obfuscate or obfuscate them :D
        // if some method is external skip obufscate

        // Obfuscate
        // If current iterated is mono method and root method and not interface.
        // Clone it and name original name, set attributes private and link from original one to new one.

        /// <summary>
        /// Protect the unity method.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_MethodDefinition"></param>
        private void OnObfuscate_ProtectUnityMethod(AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition)
        {
            // Create Method
            MethodDefinition var_ShownMethod = this.Obfuscate_Helper_CreateUnityShownMethod(var_OriginalMethod);

            // Set Type
            var_ShownMethod.DeclaringType = var_TypeDefinition;
            var_TypeDefinition.Methods.Add(var_ShownMethod);

            // Adjust
            this.OnObfuscate_Helper_AdjustOldMethod(_Project, var_OriginalMethod);

            // Link from shown method to hidden
            this.OnObfuscate_Helper_LinkMethods(var_ShownMethod, var_OriginalMethod);
        }

        /// <summary>
        /// True: _MethodDefinition name is a MonoBehaviour Unity method.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        /// <returns></returns>
        private bool OnObfuscate_Helper_IsUnityMethodName(MethodDefinition _MethodDefinition)
        {
            switch (_MethodDefinition.Name)
            {
                case "Awake":
                    { return true; }
                case "Start":
                    { return true; }
                case "Update":
                    { return true; }
                case "FixedUpdate":
                    { return true; }
                case "LateUpdate":
                    { return true; }
                case "OnGUI":
                    { return true; }
                case "OnEnable":
                    { return true; }
                case "OnDisable":
                    { return true; }
                case "OnMouseDown":
                    { return true; }
                case "OnMouseDrag":
                    { return true; }
                case "OnMouseEnter":
                    { return true; }
                case "OnMouseExit":
                    { return true; }
                case "OnMouseOver":
                    { return true; }
                case "OnMouseUp":
                    { return true; }
                case "OnRenderObject":
                    { return true; }
                case "Reset":
                    { return true; }
            }

            return false;
        }

        /// <summary>
        /// True: _MethodDefinition has return type void and has a unity method name.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        /// <returns></returns>
        private bool OnObfuscate_Helper_IsUnityMethod(MethodDefinition _MethodDefinition)
        {
            // Check if its an Unity Method
            if (this.OnObfuscate_Helper_IsUnityMethodName(_MethodDefinition))
            {
                if (_MethodDefinition.ReturnType.FullName == _MethodDefinition.Module.TypeSystem.Void.FullName)
                {
                    if (!_MethodDefinition.HasParameters)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// The type contains a method with the name _MethodName.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_MethodName"></param>
        /// <returns></returns>
        private bool OnObfuscate_Helper_TypeContainsMethod(TypeDefinition _TypeDefinition, String _MethodName)
        {
            for (int i = 0; i < _TypeDefinition.Methods.Count; i++)
            {
                if (_TypeDefinition.Methods[i].Name == _MethodName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clones a method with random values.
        /// If cannot be cloned or a choosen name already got used, returns null.
        /// </summary>
        /// <param name="_Project"></param>
        /// <param name="_AssemblyDefinition"></param>
        /// <param name="_Source"></param>
        /// <returns></returns>
        private MethodDefinition OnObfuscate_Helper_CloneMethod(AssemblyDefinition _AssemblyDefinition, MethodDefinition _Source)
        {
            // Random Name Index
            uint var_NameIndex = (uint)this.random.Next(1, 9999);

            // Find name.
            String var_ObfuscatedName = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_NameIndex);

            // TODO: Retry!

            // Check if the type already contains a method like this!
            if (this.OnObfuscate_Helper_TypeContainsMethod(_Source.DeclaringType, var_ObfuscatedName))
            {
                return null;
            }

            // Try to clone method with var_ObfuscatedName.
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Clone.CloneHelper.TryToCloneMethod(_AssemblyDefinition, _Source, var_ObfuscatedName, out MethodDefinition _Target))
            {
                return _Target;
            }

            return null;
        }


        private MethodDefinition Obfuscate_Helper_CreateUnityShownMethod(MethodDefinition _Source)
        {
            // Copy Attributes
            MethodAttributes var_Attributes = _Source.Attributes;

            // Create new Method
            MethodDefinition var_MethodDefinition = new MethodDefinition(_Source.Name, var_Attributes, _Source.Module.TypeSystem.Void);

            return var_MethodDefinition;
        }

        private MethodDefinition OnObfuscate_Helper_AdjustOldMethod(MethodDefinition _Source)
        {
            //Set to public!
            _Source.Attributes = MethodAttributes.Public; // Note: (Hab ich gemacht weil, das ding n random name gibt und einige methoden noch hierrauf verweisen!)
            //(Falls die attribute übernommen werden, kann es vorkommen das es override/abstract/... ist, und es die nachfolgenden/vorherigen methoden nicht gibt!)

            //New Random Name!
            _Source.Name = _Project.RenameManager.GetUniqueObfuscatedNameMethod(_Source, int.MaxValue);

            //Remove Attributes!
            _Source.CustomAttributes.Clear();

            return _Source;
        }

        private void OnObfuscate_Helper_LinkMethods(MethodDefinition _Shown, MethodDefinition _Hidden)
        {
            // Replace Body
            if (_Shown.HasBody)
            {
                // Clear Variables
                _Shown.Body.Variables.Clear();

                // Clear Instructions
                var var_Instructions = _Shown.Body.Instructions;
                var_Instructions.Clear();

                MethodReference var_CalledMethod = _Hidden;

                //The Type has GenericParameters, so the Parent needs those.
                if (_Hidden.DeclaringType.HasGenericParameters)
                {
                    List<TypeReference> var_GenericArray = new List<TypeReference>();
                    for (int g = 0; g < _Hidden.DeclaringType.GenericParameters.Count; g++)
                    {
                        var_GenericArray.Add(_Hidden.DeclaringType.GenericParameters[g]);
                    }
                    var_CalledMethod = OnObfuscate_Helper_MakeHostInstanceGeneric(_Hidden, var_GenericArray.ToArray());
                }

                // Replace Method
                var_Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                var_Instructions.Add(Instruction.Create(OpCodes.Call, var_CalledMethod));
                var_Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
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
*/















/*

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//OPS Mono
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

//OPS Obfuscator
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Extension;
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Logging;
using OPS.Obfuscator.Editor.Member.Cache;
using OPS.Obfuscator.Editor.Member.Helper;
using OPS.Obfuscator.Editor.Project;
using OPS.Obfuscator.Editor.Setting;

namespace OPS.Obfuscator.Editor.Components.Internal
{
    internal class UnityMethodComponent : AObfuscationComponent
    {
        //Info
#region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Unity Method Obfuscation";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Obfuscate Unity Methods.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Obfuscate Unity Methods.";
            }
        }

#endregion

#if Obfuscator_Free
#else

        //Obfuscation
#region Obfuscation

        public override bool OnObfuscate(Project.ObfuscationProject _Project)
        {
            //Check if unity method obfuscation got activated.
            if (!_Project.Settings.Get_ComponentSetting_As_Bool(MethodComponent.CObfuscation_Component_Method, MethodComponent.CObfuscate_Method_Unity))
            {
                //
                Logging.Logger.Log(ELogType.Info, "(" + this.Name + ") OnObfuscate", "The obfuscation of unity methods got skipped by your settings!");
                //

                return true;
            }

            //Analyse all Methods.
            foreach (AssemblyInfo var_AssemblyInfo in _Project.ObfuscateAssemblyList)
            {
                //
                Logging.Logger.Log(ELogType.Info, "(" + this.Name + ") OnObfuscate", "[" + var_AssemblyInfo.Name + "] Obfuscating unity methods...");
                //

                //Find names.
                this.OnObfuscate_ObfuscateUnityMethods(_Project, var_AssemblyInfo);
            }

            return true;
        }

        private void OnObfuscate_ObfuscateUnityMethods(Project.ObfuscationProject _Project, AssemblyInfo _AssemblyInfo)
        {
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if(!UnityHelper.IsMonoBehaviour(var_TypeDefinition, _Project.TypeCache))
                {
                    continue;
                }

                List<MethodDefinition> var_FoundUnityMethods = new List<MethodDefinition>();
                foreach (MethodDefinition var_Method in var_TypeDefinition.Methods)
                {
                    if(!this.OnObfuscate_Helper_IsUnityMethod(var_Method))
                    {
                        continue;
                    }

                    var_FoundUnityMethods.Add(var_Method);
                }

                foreach (MethodDefinition var_OriginalMethod in var_FoundUnityMethods)
                {
                    //
                    Logging.Logger.Log(ELogType.Info, "(" + this.Name + ") OnObfuscate", String.Format("{0} is MonoBehaviour, so Unity Method {1} gets obfuscated.", _Project.TypeCache.GetTypesOriginalFullName(var_TypeDefinition), var_OriginalMethod.Name));
                    //

                    //Create Method
                    MethodDefinition var_ShownMethod = this.Obfuscate_Helper_CreateUnityShownMethod(var_OriginalMethod);

                    //Set Type
                    var_ShownMethod.DeclaringType = var_TypeDefinition;
                    var_TypeDefinition.Methods.Add(var_ShownMethod);

                    //Adjust
                    this.OnObfuscate_Helper_AdjustOldMethod(_Project, var_OriginalMethod);

                    //Link from shown method to hidden
                    this.OnObfuscate_Helper_LinkMethods(var_ShownMethod, var_OriginalMethod);
                }
            }
        }

        private bool OnObfuscate_Helper_IsUnityMethodName(MethodDefinition _MethodDefinition)
        {
            switch (_MethodDefinition.Name)
            {
                case "Awake":
                    { return true; }
                case "Start":
                    { return true; }
                case "Update":
                    { return true; }
                case "FixedUpdate":
                    { return true; }
                case "LateUpdate":
                    { return true; }
                case "OnGUI":
                    { return true; }
                case "OnEnable":
                    { return true; }
                case "OnDisable":
                    { return true; }
                case "OnMouseDown":
                    { return true; }
                case "OnMouseDrag":
                    { return true; }
                case "OnMouseEnter":
                    { return true; }
                case "OnMouseExit":
                    { return true; }
                case "OnMouseOver":
                    { return true; }
                case "OnMouseUp":
                    { return true; }
                case "OnRenderObject":
                    { return true; }
                case "Reset":
                    { return true; }
            }

            return false;
        }

        private bool OnObfuscate_Helper_IsUnityMethod(MethodDefinition _MethodDefinition)
        {
            //Check if its an Unity Method
            if (this.OnObfuscate_Helper_IsUnityMethodName(_MethodDefinition))
            {
                if (_MethodDefinition.ReturnType.FullName == _MethodDefinition.Module.TypeSystem.Void.FullName)
                {
                    if (!_MethodDefinition.HasParameters)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private MethodDefinition Obfuscate_Helper_CreateUnityShownMethod(MethodDefinition _Source)
        {
            //Copy Attributes
            var var_Attributes = _Source.Attributes;

            //Create new Method
            MethodDefinition var_MethodDefinition = new MethodDefinition(_Source.Name, var_Attributes, _Source.Module.TypeSystem.Void);

            return var_MethodDefinition;
        }

        private MethodDefinition OnObfuscate_Helper_AdjustOldMethod(Project.ObfuscationProject _Project, MethodDefinition _Source)
        {
            //Set to private!
            _Source.Attributes = MethodAttributes.Public; //TODO:? warum hab ich das gemacht?! (Hab ich gemacht weil, das ding n random name gibt und einige methoden noch hierrauf verweisen!)
            //(Falls die attribute übernommen werden, kann es vorkommen das es override/abstract/... ist, und es die nachfolgenden/vorherigen methoden nicht gibt!)

            //New Random Name!
            _Source.Name = _Project.RenameManager.GetUniqueObfuscatedNameMethod(_Source, int.MaxValue);

            //Remove Attributes!
            _Source.CustomAttributes.Clear();

            return _Source;
        }

        private void OnObfuscate_Helper_LinkMethods(MethodDefinition _Shown, MethodDefinition _Hidden)
        {
            //Replace Body
            if (_Shown.HasBody)
            {
                //Clear Variables
                _Shown.Body.Variables.Clear();

                //Clear Instructions
                var var_Instructions = _Shown.Body.Instructions;
                var_Instructions.Clear();

                MethodReference var_CalledMethod = _Hidden;

                //The Type has GenericParameters, so the Parent needs those.
                if (_Hidden.DeclaringType.HasGenericParameters)
                {
                    List<TypeReference> var_GenericArray = new List<TypeReference>();
                    for (int g = 0; g < _Hidden.DeclaringType.GenericParameters.Count; g++)
                    {
                        var_GenericArray.Add(_Hidden.DeclaringType.GenericParameters[g]);
                    }
                    var_CalledMethod = OnObfuscate_Helper_MakeHostInstanceGeneric(_Hidden, var_GenericArray.ToArray());
                }

                //Replace Method
                var_Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                var_Instructions.Add(Instruction.Create(OpCodes.Call, var_CalledMethod));
                var_Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
        }

        private GenericInstanceType OnObfuscate_Helper_MakeGenericInstanceType(TypeReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (arguments.Length == 0)
                throw new ArgumentException();
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);

            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        private MethodReference OnObfuscate_Helper_MakeHostInstanceGeneric(MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType, OnObfuscate_Helper_MakeGenericInstanceType(self.DeclaringType, arguments))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

#endregion

#endif
    }
}

*/

#endif