using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS Mono
using OPS.Mono.Cecil;

//OPS Obfuscator
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Accessibility;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for accessibility.
    /// </summary>
    public static class AccessibilityHelper
    {
        //Type
        #region Type

        /// <summary>
        ///  Returns the EAccessibilityLevel of a member.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static EAccessibilityLevel AccessibilityLevel_Type(TypeDefinition _TypeDefinition)
        {
            /*
            public -> IsPublic
            internal -> IsNotPublic
            (default)-> internal -> IsNotPublic
            Nested classes:
            public -> IsNestedPublic
            protected -> IsNestedFamily
            protected internal -> IsNestedFamilyOrAssembly
            private -> IsNestedPrivate
            (default)-> private -> IsNestedPrivate
            */

            //Public
            if(_TypeDefinition.IsPublic || _TypeDefinition.IsNestedPublic)
            {
                return EAccessibilityLevel.Public;
            }

            //Protected
            if (_TypeDefinition.IsNestedFamily)
            {
                return EAccessibilityLevel.Protected;
            }

            //Protected and Internal
            if (_TypeDefinition.IsNestedFamilyOrAssembly)
            {
                return EAccessibilityLevel.Protected_And_Internal;
            }

            //Protected or Private
            if (_TypeDefinition.IsNestedFamilyAndAssembly)
            {
                return EAccessibilityLevel.Protected_Or_Private;
            }

            //Internal
            if (_TypeDefinition.IsNotPublic)
            {
                return EAccessibilityLevel.Internal;
            }

            //Private
            return EAccessibilityLevel.Private;
        }

        #endregion

        //Method
        #region Method

        /// <summary>
        ///  Returns the EAccessibilityLevel of a member.
        /// </summary>
        /// <param name="_MethodDefinition"></param>
        /// <returns></returns>
        public static EAccessibilityLevel AccessibilityLevel_Method(MethodDefinition _MethodDefinition)
        {
            //Public
            if (_MethodDefinition.IsPublic)
            {
                return EAccessibilityLevel.Public;
            }

            //Protected
            if (_MethodDefinition.IsFamily)
            {
                return EAccessibilityLevel.Protected;
            }

            //Protected and Internal
            if (_MethodDefinition.IsFamilyOrAssembly)
            {
                return EAccessibilityLevel.Protected_And_Internal;
            }

            //Protected or Private
            if (_MethodDefinition.IsFamilyAndAssembly)
            {
                return EAccessibilityLevel.Protected_Or_Private;
            }

            //Internal
            if (_MethodDefinition.IsAssembly)
            {
                return EAccessibilityLevel.Internal;
            }

            //Private
            return EAccessibilityLevel.Private;
        }

        #endregion

        //Field
        #region Field

        /// <summary>
        ///  Returns the EAccessibilityLevel of a member.
        /// </summary>
        /// <param name="_FieldDefinition"></param>
        /// <returns></returns>
        public static EAccessibilityLevel AccessibilityLevel_Field(FieldDefinition _FieldDefinition)
        {
            //Public
            if (_FieldDefinition.IsPublic)
            {
                return EAccessibilityLevel.Public;
            }

            //Protected
            if (_FieldDefinition.IsFamily)
            {
                return EAccessibilityLevel.Protected;
            }

            //Protected and Internal
            if (_FieldDefinition.IsFamilyOrAssembly)
            {
                return EAccessibilityLevel.Protected_And_Internal;
            }

            //Protected or Private
            if (_FieldDefinition.IsFamilyAndAssembly)
            {
                return EAccessibilityLevel.Protected_Or_Private;
            }

            //Internal
            if (_FieldDefinition.IsAssembly)
            {
                return EAccessibilityLevel.Internal;
            }

            //Private
            return EAccessibilityLevel.Private;
        }

        #endregion

        //Property
        #region Property

        /// <summary>
        ///  Returns the EAccessibilityLevel of a member.
        /// </summary>
        /// <param name="_PropertyDefinition"></param>
        /// <returns></returns>
        public static EAccessibilityLevel AccessibilityLevel_Property(PropertyDefinition _PropertyDefinition)
        {
            //Public
            if(_PropertyDefinition.GetMethod != null)
            {
                if(AccessibilityLevel_Method(_PropertyDefinition.GetMethod) == EAccessibilityLevel.Public)
                {
                    return EAccessibilityLevel.Public;
                }
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.SetMethod) == EAccessibilityLevel.Public)
                {
                    return EAccessibilityLevel.Public;
                }
            }

            //Protected
            if (_PropertyDefinition.GetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.GetMethod) == EAccessibilityLevel.Protected)
                {
                    return EAccessibilityLevel.Protected;
                }
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.SetMethod) == EAccessibilityLevel.Protected)
                {
                    return EAccessibilityLevel.Protected;
                }
            }

            //Protected and Internal
            if (_PropertyDefinition.GetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.GetMethod) == EAccessibilityLevel.Protected_And_Internal)
                {
                    return EAccessibilityLevel.Protected_And_Internal;
                }
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.SetMethod) == EAccessibilityLevel.Protected_And_Internal)
                {
                    return EAccessibilityLevel.Protected;
                }
            }

            //Protected or Private
            if (_PropertyDefinition.GetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.GetMethod) == EAccessibilityLevel.Protected_Or_Private)
                {
                    return EAccessibilityLevel.Protected_Or_Private;
                }
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.SetMethod) == EAccessibilityLevel.Protected_Or_Private)
                {
                    return EAccessibilityLevel.Protected_Or_Private;
                }
            }

            //Internal
            if (_PropertyDefinition.GetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.GetMethod) == EAccessibilityLevel.Internal)
                {
                    return EAccessibilityLevel.Internal;
                }
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                if (AccessibilityLevel_Method(_PropertyDefinition.SetMethod) == EAccessibilityLevel.Internal)
                {
                    return EAccessibilityLevel.Internal;
                }
            }

            //Private
            return EAccessibilityLevel.Private;
        }

        #endregion

        //Event
        #region Event

        /// <summary>
        /// Returns the EAccessibilityLevel of a member.
        /// </summary>
        /// <param name="_EventDefinition"></param>
        /// <returns></returns>
        public static EAccessibilityLevel AccessibilityLevel_Event(EventDefinition _EventDefinition)
        {
            //Public
            if (_EventDefinition.AddMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.AddMethod) == EAccessibilityLevel.Public)
                {
                    return EAccessibilityLevel.Public;
                }
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.RemoveMethod) == EAccessibilityLevel.Public)
                {
                    return EAccessibilityLevel.Public;
                }
            }

            //Protected
            if (_EventDefinition.AddMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.AddMethod) == EAccessibilityLevel.Protected)
                {
                    return EAccessibilityLevel.Protected;
                }
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.RemoveMethod) == EAccessibilityLevel.Protected)
                {
                    return EAccessibilityLevel.Protected;
                }
            }

            //Protected and Internal
            if (_EventDefinition.AddMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.AddMethod) == EAccessibilityLevel.Protected_And_Internal)
                {
                    return EAccessibilityLevel.Protected_And_Internal;
                }
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.RemoveMethod) == EAccessibilityLevel.Protected_And_Internal)
                {
                    return EAccessibilityLevel.Protected;
                }
            }

            //Protected or Private
            if (_EventDefinition.AddMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.AddMethod) == EAccessibilityLevel.Protected_Or_Private)
                {
                    return EAccessibilityLevel.Protected_Or_Private;
                }
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.RemoveMethod) == EAccessibilityLevel.Protected_Or_Private)
                {
                    return EAccessibilityLevel.Protected_Or_Private;
                }
            }

            //Internal
            if (_EventDefinition.AddMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.AddMethod) == EAccessibilityLevel.Internal)
                {
                    return EAccessibilityLevel.Internal;
                }
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                if (AccessibilityLevel_Method(_EventDefinition.RemoveMethod) == EAccessibilityLevel.Internal)
                {
                    return EAccessibilityLevel.Internal;
                }
            }

            //Private
            return EAccessibilityLevel.Private;
        }

        #endregion
    }
}
