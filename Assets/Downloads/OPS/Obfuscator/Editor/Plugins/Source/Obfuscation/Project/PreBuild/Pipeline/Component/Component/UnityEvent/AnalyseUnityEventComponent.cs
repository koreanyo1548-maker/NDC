// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Unity
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// OPS - Project - Pipeline
using OPS.Editor.Project.Pipeline;

// OPS - Project - Step
using OPS.Editor.Project.Step;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly.DotNet.Member.Extension;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    /// <summary>
    /// This component analyses Unity Components for Unity Events.
    /// </summary>
    internal class AnalyseUnityEventComponent : APreBuildComponent, IComponentProcessingComponent
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
                return "Analyse Unity Event Component";
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
                return "Analyse Unity Components for UnityEvents. Skip the attached method names.";
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
                return "Analyse Unity Components for UnityEvents. Skip the attached method names.";
            }
        }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// Return whether the component is activated or deactivated for the pipeline processing.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                // Check if setting is activated.
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CEnable_Try_Find_Inspector_Unity_Event_Methods))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // OnPreBuild
        #region OnPreBuild

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <returns></returns>
        public override bool OnPreBuild()
        {
            return true;
        }

        #endregion

        // Process Scenes or Prefabs
        #region Process Scenes or Prefabs

        /// <summary>
        /// Process scenes for Unity Events if enabled. Default is true.
        /// </summary>
        public bool ProcessScenes
        {
            get
            {
                return this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Scenes, true);
            } 
        }

        /// <summary>
        /// Process prefabs for Unity Events if enabled. Default is true.
        /// </summary>
        public bool ProcessPrefabs
        {
            get
            {
                return this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.UnityEventComponent.CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Prefabs, true);
            }
        }

        #endregion

        // Gui Methods
        #region Gui Methods

        /// <summary>
        /// The step output data key for the ReferencedEventMethodHashSet.
        /// </summary>
        public const String CReferencedEventMethodHashSet = "ReferencedEventMethodHashSet";

        /// <summary>
        /// A list of all references event methods.
        /// </summary>
        private HashSet<String> ReferencedEventMethodHashSet = new HashSet<string>();

        #endregion

        // On Analyse Component
        #region On Analyse Component

        /// <summary>
        /// Contains a list of already searched objects as hash, so they will not be iterated twice.
        /// </summary>
        private HashSet<int> alreadySearchedObjects = new HashSet<int>();

        /// <summary>
        /// Searchs the passed _Component for Unity Events, to later skip their refered method names.
        /// </summary>
        /// <param name="_Component"></param>
        /// <returns></returns>
        public bool OnAnalyse_Component(UnityEngine.Component _Component)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Find unity event methods in component '{0}' of type '{1}'.", _Component.name, _Component.GetType()));

            try
            {
                // Search for Unity Events in the passed Component.
                List<UnityEventBase> var_Events = this.GetUnityEvents(_Component);

                // Iterate the found Unity Events and stores their refered method names.
                for(int e = 0; e < var_Events.Count; e++)
                {
                    this.OnAnalyse_Component_IterateEvent(_Component, var_Events[e]);
                }
            }
            catch (Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not find methods in " + _Component.name + " of type " + _Component.GetType() + " with Exception: " + e.ToString());
            }

            return true;
        }

        /// <summary>
        /// Search the passed _Object for Unity Events.
        /// </summary>
        /// <param name="_Object"></param>
        /// <returns></returns>
        private List<UnityEventBase> GetUnityEvents(System.Object _Object)
        {
            // If no object got passed, return empty output.
            if (_Object == null)
            {
                return new List<UnityEventBase>();
            }

            // If the object was already iterated, skip it.
            if (this.alreadySearchedObjects.Contains(_Object.GetHashCode()))
            {
                return new List<UnityEventBase>();
            }

            // Add hash to searched objects.
            this.alreadySearchedObjects.Add(_Object.GetHashCode());

            // Create result of found Unity Events.
            List<UnityEventBase> var_Result = new List<UnityEventBase>();

            // Read the type.
            Type var_Type = _Object.GetType();

            // If the passed object is a Unity Event. Add and then return it.
            if (var_Type.IsSubclassOf(typeof(UnityEventBase)))
            {
                var_Result.Add(_Object as UnityEventBase);
            }
            // If the type is enumerable and also a collection. Iterate the passed objects. 
            else if (var_Type.IsEnumerable() && var_Type.IsCollection())
            {
                // Get as enumerable.
                IEnumerable var_Enumerable = (IEnumerable)_Object;

                // Iterate each element in the enumerable.
                foreach (var var_Element in var_Enumerable)
                {
                    // Skip empty elements.
                    if (var_Element == null)
                    {
                        continue;
                    }

                    // Add found UnityEvents.
                    var_Result.AddRange(this.GetUnityEvents(var_Element));
                }
            }
            // Else iterate the fields.
            else
            {
                // Find all private / protected / public fields.
                FieldInfo[] var_FieldInfos = var_Type.GetFields(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

                // Iterate all found fields, resolve them and search for Unity Events.
                for (int f = 0; f < var_FieldInfos.Length; f++)
                {
                    // Read the field type.
                    Type var_FieldType = var_FieldInfos[f].FieldType;

                    // Skip if not a class.
                    if (!var_FieldType.IsClass)
                    {
                        continue;
                    }

                    // Read field as object.
                    System.Object var_FieldObject = var_FieldInfos[f].GetValue(_Object) as System.Object;

                    // Skip null objects.
                    if (var_FieldObject == null)
                    {
                        continue;
                    }

                    // Recursive search and add.
                    var_Result.AddRange(this.GetUnityEvents(var_FieldObject));
                }
            }

            // Return the found Unity Events.
            return var_Result;
        }

        /// <summary>
        /// Adds the found methods in the passed _UnityEventBase to the ReferencedEventMethodHashSet.
        /// </summary>
        /// <param name="_Component"></param>
        /// <param name="_UnityEventBase"></param>
        private void OnAnalyse_Component_IterateEvent(UnityEngine.Component _Component, UnityEventBase _UnityEventBase)
        {
            if(_UnityEventBase == null)
            {
                return;
            }

            // Get the count of attached events.
            int var_EventCount = _UnityEventBase.GetPersistentEventCount();

            if(var_EventCount == 0)
            {
                return;
            }

            // Used for logging, stored method names.
            String[] var_EventMethodNameArray = new String[var_EventCount];

            // Iterate all events and ignore their names.
            for(int e = 0; e < var_EventCount; e++)
            {
                try
                {
                    String var_MethodName = _UnityEventBase.GetPersistentMethodName(e);

                    // Add method if not already added.
                    if (!this.ReferencedEventMethodHashSet.Contains(var_MethodName))
                    {
                        this.ReferencedEventMethodHashSet.Add(var_MethodName);
                    }

                    // Add to debug log array.
                    var_EventMethodNameArray[e] = var_MethodName;
                }
                catch(Exception ex)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not find method in " + _Component.name + " of type " + _Component.GetType() + " at event index " + e + " with Exception: " + ex.ToString());
                }
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found unity event methods in component '{0}'", _Component.name), Enumerable.Range(0, var_EventCount).Select(e => var_EventMethodNameArray[e]).ToList());
        }


        #endregion

        // On Process Component
        #region On Process Component

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="_Component"></param>
        /// <returns></returns>
        public bool OnProcess_Component(UnityEngine.Component _Component)
        {
            return true;
        }

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

        /// <summary>
        /// Stores the ReferencedEventMethodHashSet in the _StepOutput.
        /// </summary>
        /// <param name="_StepOutput"></param>
        /// <returns></returns>
        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            bool var_Result = base.OnPostPipelineProcess(_StepOutput);

            // Add types to datacontainer.
            var_Result = var_Result && _StepOutput.Add(AnalyseUnityEventComponent.CReferencedEventMethodHashSet, this.ReferencedEventMethodHashSet, true);

            return var_Result;
        }

        #endregion
    }
}
