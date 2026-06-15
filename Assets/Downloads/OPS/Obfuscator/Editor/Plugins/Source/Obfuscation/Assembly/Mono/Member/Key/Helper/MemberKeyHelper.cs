using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Key.Helper
{
    /// <summary>
    /// Static helper class for member keys.
    /// </summary>
    public static class MemberKeyHelper
    {
        /// <summary>
        /// Returns the matching _MemberKey for _MemberReference.
        /// </summary>
        /// <param name="_MemberReference"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public static bool TryToCreateMemberKey(MemberReference _MemberReference, out IMemberKey _MemberKey)
        {
            // The MemberDefinition for which the MemberKey should be created.
            IMemberDefinition var_MemberDefinition;

            // The _MemberReference is already a MemberDefinition.
            if (_MemberReference is IMemberDefinition)
            {
                var_MemberDefinition = _MemberReference as IMemberDefinition;
            }
            else
            {
                // Check if can be resolved, else something is missing!
                if (!MemberReferenceHelper.TryToResolve(_MemberReference, out var_MemberDefinition))
                {
                    // Could not resolve! Could not create member key!
                    _MemberKey = null;
                    return false;
                }
            }

            // Try to create a key based on a memberdefinition.
            return TryToCreateMemberKey(var_MemberDefinition, out _MemberKey);
        }

        /// <summary>
        /// Returns the matching _MemberKey for _MemberDefinition.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public static bool TryToCreateMemberKey(IMemberDefinition _MemberDefinition, out IMemberKey _MemberKey)
        {
            // Set result member key default to null.
            _MemberKey = null;

            // Create member key based on memberdefinition.
            if (_MemberDefinition is TypeDefinition)
            {
                _MemberKey = new TypeKey(_MemberDefinition as TypeDefinition);
            }
            else if (_MemberDefinition is MethodDefinition)
            {
                _MemberKey = new MethodKey(_MemberDefinition as MethodDefinition);
            }
            else if (_MemberDefinition is FieldDefinition)
            {
                _MemberKey = new FieldKey(_MemberDefinition as FieldDefinition);
            }
            else if (_MemberDefinition is PropertyDefinition)
            {
                _MemberKey = new PropertyKey(_MemberDefinition as PropertyDefinition);
            }
            else if (_MemberDefinition is EventDefinition)
            {
                _MemberKey = new EventKey(_MemberDefinition as EventDefinition);
            }

            return _MemberKey != null;
        }

        /// <summary>
        /// Tries to create a _MemberKey based on _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public static bool TryToCreateMemberKey(EMemberType _MemberType, out IMemberKey _MemberKey)
        {
            // Set result member key default to null.
            _MemberKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                case EMemberType.Type:
                    {
                        _MemberKey = (IMemberKey)Activator.CreateInstance(typeof(TypeKey), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null);

                        break;
                    }
                case EMemberType.Method:
                    {
                        _MemberKey = (IMemberKey)Activator.CreateInstance(typeof(MethodKey), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null);

                        break;
                    }
                case EMemberType.Field:
                    {
                        _MemberKey = (IMemberKey)Activator.CreateInstance(typeof(FieldKey), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null);

                        break;
                    }
                case EMemberType.Property:
                    {
                        _MemberKey = (IMemberKey)Activator.CreateInstance(typeof(PropertyKey), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null);

                        break;
                    }
                case EMemberType.Event:
                    {
                        _MemberKey = (IMemberKey)Activator.CreateInstance(typeof(EventKey), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, null, null);

                        break;
                    }
            }

            return _MemberKey != null;
        }
    }
}
