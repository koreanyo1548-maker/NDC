// System
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Serialization.Unity.Helper
{
    /// <summary>
    /// Helper class for Unity SerializedProperties.
    /// </summary>
    public static class SerializedPropertyHelper
    {
        // MonoScript
        #region MonoScript

        /// <summary>
        /// Returns all MonoScript references in _Object without duplicates.
        /// </summary>
        /// <param name="_Object"></param>
        /// <returns></returns>
        public static List<MonoScript> GetAllMonoScriptReferences(UnityEngine.Object _Object)
        {
            return SearchVariablesWithSerializedObject(_Object);
        }

        /// <summary>
        /// Search through variables of an object with SerializedObject.
        /// Returns found MonoScripts.
        /// </summary>
        /// <param name="_Object"></param>
        /// <returns></returns>
        private static List<MonoScript> SearchVariablesWithSerializedObject(UnityEngine.Object _Object)
        {
            // The found MonoScripts.
            HashSet<MonoScript> var_Result = new HashSet<MonoScript>();

            try
            {
                // Get the UnityEngine.SerializedObject of the UnityEngine.Object
                SerializedObject var_SerializedObject = new SerializedObject(_Object);

                // Get iterator over the SerializedProperties.
                SerializedProperty var_Iterator = var_SerializedObject.GetIterator().Copy();

                // Note: Has do be done?
                var_Iterator.NextVisible(true);

                // Get iterator over SerializedProperties Children. (Array elements are children too!)
                SerializedProperty var_Iterator_Children = var_SerializedObject.GetIterator().Copy();

                // Deep iterate over all SerializedProperties.
                bool var_SearchChildren = true;

                // Store the already searched path, so if a ring happens break loop.
                // TODO: How is it with arrays? do the elements have the same path?
                HashSet<String> var_PropertyPathAlreadySearchedHashSet = new HashSet<string>();

                // Iterate in two loops over the SerializedProperties.
                // var_Iterator only iterated SerializedProperties on level 0.
                // var_Iterator_Children iterates the SerializedProperties and their children one level deep.
                
                // Note: Has do be done?
                if (var_Iterator_Children.NextVisible(var_SearchChildren))
                {
                    do
                    {
                        // Debug: UnityEngine.Debug.Log(var_Iterator.propertyPath + " : " + var_Iterator_Children.propertyPath);

                        // Already searched.
                        if (var_PropertyPathAlreadySearchedHashSet.Contains(var_Iterator_Children.propertyPath))
                        {
                            break;
                        }

                        // Add to already searched.
                        var_PropertyPathAlreadySearchedHashSet.Add(var_Iterator_Children.propertyPath);

                        // Are same element.
                        if(SerializedProperty.EqualContents(var_Iterator, var_Iterator_Children))
                        {
                            // Again in level deep 0.
                            var_Iterator.NextVisible(false);

                            // Start again search children.
                            var_SearchChildren = true;
                        }
                        else
                        {
                            // Already in level deep 1.
                            var_SearchChildren = false;
                        }

                        // Only MonoScript references are of interest.
                        switch (var_Iterator_Children.propertyType)
                        {
                            case SerializedPropertyType.ObjectReference:
                                {
                                    if (var_Iterator_Children.objectReferenceValue is MonoScript)
                                    {
                                        MonoScript var_MonoScript = (MonoScript)var_Iterator_Children.objectReferenceValue;

                                        if (!var_Result.Contains(var_MonoScript))
                                        {
                                            var_Result.Add(var_MonoScript);
                                        }
                                    }

                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    } while (var_Iterator_Children.NextVisible(var_SearchChildren));
                }
            }
            catch(Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());
            }

            return var_Result.ToList();
        }

        #endregion
    }
}
