using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Extension;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for types.
    /// </summary>
    public static class TypeDefinitionHelper
    {
        // Name and Namespace
        #region Name and Namespace

        /// <summary>
        /// Outputs the _Namespace and _Name by _Fullname.
        /// If _Fullname is a nested type, namespace is empty.
        /// </summary>
        /// <param name="_FullName"></param>
        /// <param name="_Namespace"></param>
        /// <param name="_Name"></param>
        public static void SplitFullName(string _FullName, out string _Namespace, out string _Name)
        {
            // Check if nested.
            if (IsNested(_FullName))
            {
                // Is nested, set the last name to be the _FullName.
                String[] var_Names = _FullName.Split('/');
                _FullName = var_Names.Last();
            }

            // Split namespace and name.
            var var_LastDot = _FullName.LastIndexOf('.');

            if (var_LastDot == -1)
            {
                _Namespace = string.Empty;
                _Name = _FullName;
            }
            else
            {
                _Namespace = _FullName.Substring(0, var_LastDot);
                _Name = _FullName.Substring(var_LastDot + 1);
            }
        }

        #endregion

        // Nested
        #region Nested

        /// <summary>
        /// Returns true if the _FullName is nested (Containing a '/').
        /// </summary>
        /// <param name="_FullName"></param>
        /// <returns></returns>
        public static bool IsNested(String _FullName)
        {
            return _FullName.Contains('/');
        }

        #endregion

        // Attribute
        #region Attribute

        /// <summary>
        /// TypeKey of System.Attribute
        /// </summary>
        private static Mono.Member.Key.TypeKey System_Attribute_TypeKey = new Mono.Member.Key.TypeKey("mscorlib", "System.Attribute");

        /// <summary>
        /// True: The Type is a System.Attribute.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeAnAttribute(TypeDefinition _TypeDefinition)
        {
            // Iterate TypeDefinition and base.
            TypeDefinition var_CurrentTypeDefinition = _TypeDefinition;

            // The TypeKey for the current iterated TypeDefinition.
            TypeKey var_CurrentTypeKey = null;

            while (var_CurrentTypeDefinition != null)
            {
                var_CurrentTypeKey = new TypeKey(var_CurrentTypeDefinition);

                // Is System.Attribute so return here.
                if (var_CurrentTypeKey == System_Attribute_TypeKey)
                {
                    return true;
                }

                // Has no base type.
                if (var_CurrentTypeDefinition.BaseType == null)
                {
                    return false;
                }

                // Resolve base type.
                if (!MemberReferenceHelper.TryToResolve(var_CurrentTypeDefinition.BaseType, out var_CurrentTypeDefinition))
                {
                    // Log
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Cannot resolve base: " + Mono.Member.Helper.MemberReferenceHelper.GetExtendedFullName(var_CurrentTypeDefinition.BaseType));
                }
            }
            return false;
        }

        #endregion

        // Generic
        #region Generic

        /// <summary>
        /// True: The Type is a generic type.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeGeneric(TypeDefinition _TypeDefinition)
        {
            return _TypeDefinition.Name.Contains("`");
        }

        /// <summary>
        /// Returns a types name without the generic ` suffix.
        /// For example: MyType`2 => MyType
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static String TypeNameWithoutGenericSuffix(TypeDefinition _TypeDefinition)
        {
            return TypeNameWithoutGenericSuffix(_TypeDefinition.Name);
        }

        /// <summary>
        /// Returns a types name without the generic ` suffix.
        /// For example: MyType`2 => MyType
        /// </summary>
        /// <param name="_TypeName"></param>
        /// <returns></returns>
        public static String TypeNameWithoutGenericSuffix(String _TypeName)
        {
            if (_TypeName.Contains("`"))
            {
                return _TypeName.Remove(_TypeName.LastIndexOf('`'));
            }
            return _TypeName;
        }

        #endregion

        // Abstract
        #region Abstract

        /// <summary>
        /// True: The Type is an abstract type.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeAbstract(TypeDefinition _TypeDefinition)
        {
            if (_TypeDefinition.IsAbstract && (_TypeDefinition.IsSealed || _TypeDefinition.IsInterface))
            {
                return false;
            }

            return _TypeDefinition.IsAbstract;
        }

        #endregion

        // Static
        #region Static

        /// <summary>
        /// True: The Type is a static type.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeStatic(TypeDefinition _TypeDefinition)
        {
            return _TypeDefinition.IsAbstract && _TypeDefinition.IsSealed;
        }

        #endregion

        // Struct
        #region Struct

        /// <summary>
        /// Determine if the given type is a struct (also known as "value type") and not a struct-alike (f.e. primitive).
        /// </summary>
        /// <param name="_TypeReference">The type to check.</param>
        /// <returns>True if the type is a struct, and not primitive or similar.</returns>
        public static bool IsTypeStruct(TypeReference _TypeReference)
        {
            if (!_TypeReference.IsValueType)
            {
                // Is not value type or struct alike.
                return false;
            }
            if (_TypeReference.IsPrimitive)
            {
                // Is a primitive value type (int, bool, float, ...).
                return false;
            }

            // Is a struct!
            return true;
        }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// TypeKey of System.Object
        /// </summary>
        private static Mono.Member.Key.TypeKey System_Object_TypeKey = new Mono.Member.Key.TypeKey("mscorlib", "System.Object");

        /// <summary>
        /// True: The _TypeDefinition IsSerializable or has System.SerializableAttribute.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeSerializable(TypeDefinition _TypeDefinition)
        {
            try
            {
                // Is serializeable so return.
                if (_TypeDefinition.IsSerializable)
                {
                    return true;
                }

                // Has a SerializableAttribute
                if (TypeHasSerializableAttribute(_TypeDefinition))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Exception: " + e.ToString());
            }

            return false;
        }

        /// <summary>
        /// True: The _TypeDefinition or any base IsSerializable or has System.SerializableAttribute.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsTypeOrBaseSerializable(TypeDefinition _TypeDefinition)
        {
            try
            {
                // Iterate TypeDefinition and base.
                TypeDefinition var_CurrentTypeDefinition = _TypeDefinition;

                // The TypeKey for the current iterated TypeDefinition.
                Mono.Member.Key.TypeKey var_CurrentTypeKey = null;

                while (var_CurrentTypeDefinition != null)
                {
                    var_CurrentTypeKey = new Mono.Member.Key.TypeKey(var_CurrentTypeDefinition);

                    // Is System.Object so return here.
                    if (var_CurrentTypeKey == System_Object_TypeKey)
                    {
                        return false;
                    }

                    // Check is Serializeable.
                    if (IsTypeSerializable(var_CurrentTypeDefinition))
                    {
                        return true;
                    }

                    // Has no base type.
                    if (var_CurrentTypeDefinition.BaseType == null)
                    {
                        return false;
                    }

                    // Cannot resolve base type!
                    if (!Mono.Member.Helper.MemberReferenceHelper.TryToResolve(var_CurrentTypeDefinition.BaseType, out var_CurrentTypeDefinition))
                    {
                        // Log
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Cannot resolve base: " + Mono.Member.Helper.MemberReferenceHelper.GetExtendedFullName(var_CurrentTypeDefinition.BaseType));

                        // Cannot resolve!!
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Exception: " + e.ToString());
            }

            return false;
        }

        /// <summary>
        /// True: _TypeDefinition has System.SerializableAttribute.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private static bool TypeHasSerializableAttribute(TypeDefinition _TypeDefinition)
        {
            return Mono.Member.Attribute.Helper.AttributeHelper.HasCustomAttribute(_TypeDefinition, "SerializableAttribute");
        }

        #endregion

        // Inherit
        #region Inherit

        /// <summary>
        /// Returns if the type _Type inherits from type with original _TypeKey.
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <param name="_TypeKey"></param>
        /// <returns></returns>
        public static bool InheritsFrom(TypeDefinition _Type, TypeCache _TypeCache, TypeKey _TypeKey)
        {
            return _TypeCache.InheritesFrom(_Type, _TypeKey);
        }

        /// <summary>
        /// Returns if the type _Type inherits from type with original fullname _FullName.
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <param name="_FullName"></param>
        /// <returns></returns>
        public static bool InheritsFrom(TypeDefinition _Type, TypeCache _TypeCache, String _FullName)
        {
            return _TypeCache.InheritesFrom(_Type, _FullName);
        }

        #endregion

        // Constructor
        #region Constructor

        /// <summary>
        /// Returns if the _TypeDefinition has an empty constructor.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool HasEmptyConstructor(TypeDefinition _TypeDefinition)
        {
            return GetEmptyConstructor(_TypeDefinition) != null;
        }

        /// <summary>
        /// Find the default constructor of a TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static MethodDefinition GetEmptyConstructor(TypeDefinition _TypeDefinition)
        {
            foreach (MethodDefinition var_Method in _TypeDefinition.Methods)
            {
                if (var_Method.Name == ".ctor" && !var_Method.HasParameters)
                {
                    return var_Method;
                }
            }
            return null;
        }

        /// <summary>
        /// Add an empty constructor to _TypeDefinition which exists / or will be added in / to _ModuleDefinition.
        /// </summary>
        /// <param name="_ModuleDefinition"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_BaseEmptyConstructor"></param>
        /// <returns></returns>
        public static MethodDefinition AddEmptyConstructor(ModuleDefinition _ModuleDefinition, TypeDefinition _TypeDefinition, MethodReference _BaseEmptyConstructor)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".ctor", methodAttributes, _ModuleDefinition.TypeSystem.Void);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, _BaseEmptyConstructor));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            _TypeDefinition.Methods.Add(method);

            return method;
        }

        /// <summary>
        /// Returns all constructor, but the static constructor.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static List<MethodDefinition> GetAllConstructor(TypeDefinition _TypeDefinition)
        {
            List<MethodDefinition> var_Result = new List<MethodDefinition>();

            foreach (MethodDefinition var_Method in _TypeDefinition.Methods)
            {
                if (var_Method.Name == ".ctor")
                {
                    var_Result.Add(var_Method);
                }
            }
            return var_Result;
        }

        /// <summary>
        /// Returns the staticc constructor.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static MethodDefinition GetStaticConstructor(TypeDefinition _TypeDefinition)
        {
            foreach (MethodDefinition var_Method in _TypeDefinition.Methods)
            {
                if (var_Method.Name == ".cctor")
                {
                    return var_Method;
                }
            }
            return null;
        }

        /// <summary>
        /// Create an empty static constructor for _TypeDefinition.
        /// </summary>
        /// <param name="_ModuleDefinition"></param>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static MethodDefinition AddEmptyStaticConstructor(ModuleDefinition _ModuleDefinition, TypeDefinition _TypeDefinition)
        {
            var methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".cctor", methodAttributes, _ModuleDefinition.TypeSystem.Void);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            _TypeDefinition.Methods.Add(method);

            return method;
        }

        #endregion

        // Member
        #region Member

        /// <summary>
        ///  Returns a list of all members of type TMemberDefinition in _TypeDefinition, sharing the name _OriginalName searching base types.
        /// </summary>
        /// <typeparam name="TMemberDefinition"></typeparam>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_Cache"></param>
        /// <param name="_OriginalName"></param>
        /// <returns></returns>
        public static List<TMemberDefinition> FindMember<TMemberDefinition>(TypeDefinition _TypeDefinition, IMemberCache _Cache, String _OriginalName) where TMemberDefinition : IMemberDefinition
        {
            List<TMemberDefinition> var_Result = new List<TMemberDefinition>();

            // Search and add local members.
            if (typeof(TMemberDefinition) == typeof(MethodDefinition))
            {
                var_Result.AddRange(FindMember<TMemberDefinition>(_TypeDefinition.Methods as OPS.Mono.Collections.Generic.Collection<TMemberDefinition>, _Cache, _OriginalName));
            }
            else if (typeof(TMemberDefinition) == typeof(FieldDefinition))
            {
                var_Result.AddRange(FindMember<TMemberDefinition>(_TypeDefinition.Fields as OPS.Mono.Collections.Generic.Collection<TMemberDefinition>, _Cache, _OriginalName));
            }
            else if (typeof(TMemberDefinition) == typeof(PropertyDefinition))
            {
                var_Result.AddRange(FindMember<TMemberDefinition>(_TypeDefinition.Properties as OPS.Mono.Collections.Generic.Collection<TMemberDefinition>, _Cache, _OriginalName));
            }
            else if (typeof(TMemberDefinition) == typeof(EventDefinition))
            {
                var_Result.AddRange(FindMember<TMemberDefinition>(_TypeDefinition.Events as OPS.Mono.Collections.Generic.Collection<TMemberDefinition>, _Cache, _OriginalName));
            }

            // Search Base Type
            if (_TypeDefinition.BaseType == null)
            {
            }
            else
            {
                if (MemberReferenceHelper.TryToResolve(_TypeDefinition.BaseType, out TypeDefinition var_BaseTypeDefinition))
                {
                    var_Result.AddRange(FindMember<TMemberDefinition>(var_BaseTypeDefinition, _Cache, _OriginalName));
                }
            }

            return var_Result;
        }

        /// <summary>
        /// Returns a list of all members of type TMemberDefinition sharing the name _OriginalName.
        /// </summary>
        /// <typeparam name="TMemberDefinition"></typeparam>
        /// <param name="_MemberCollection"></param>
        /// <param name="_Cache"></param>
        /// <param name="_OriginalName"></param>
        /// <returns></returns>
        public static List<TMemberDefinition> FindMember<TMemberDefinition>(OPS.Mono.Collections.Generic.Collection<TMemberDefinition> _MemberCollection, IMemberCache _Cache, String _OriginalName) where TMemberDefinition : IMemberDefinition
        {
            List<TMemberDefinition> var_Result = new List<TMemberDefinition>();

            foreach (TMemberDefinition var_Member in _MemberCollection)
            {
                var var_Key = _Cache.GetOriginalMemberKeyByMemberDefinition(var_Member);

                if (var_Key == null)
                {
                    continue;
                }

                if (var_Key.Name.Equals(_OriginalName))
                {
                    var_Result.Add(var_Member);
                }
            }
            return var_Result;
        }

        /// <summary>
        /// Returns the required members in _TypeDefinition.
        /// TMemberDefinition as TypeDefinition will return all nested types.
        /// TMemberDefinition as MethodDefinition will return all methods.
        /// TMemberDefinition as FieldDefinition will return all fields.
        /// TMemberDefinition as PropertyDefinition will return all properties.
        /// TMemberDefinition as EventDefinition will return all events.
        /// Else will return null!
        /// </summary>
        /// <typeparam name="TMemberDefinition"></typeparam>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static IEnumerable<TMemberDefinition> GetMembers<TMemberDefinition>(TypeDefinition _TypeDefinition)
            where TMemberDefinition : IMemberDefinition
        {
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition");
            }

            if (typeof(TMemberDefinition) == typeof(TypeDefinition))
            {
                return _TypeDefinition.NestedTypes.Cast<TMemberDefinition>();
            }
            if (typeof(TMemberDefinition) == typeof(MethodDefinition))
            {
                return _TypeDefinition.Methods.Cast<TMemberDefinition>();
            }
            if (typeof(TMemberDefinition) == typeof(FieldDefinition))
            {
                return _TypeDefinition.Fields.Cast<TMemberDefinition>();
            }
            if (typeof(TMemberDefinition) == typeof(PropertyDefinition))
            {
                return _TypeDefinition.Properties.Cast<TMemberDefinition>();
            }
            if (typeof(TMemberDefinition) == typeof(EventDefinition))
            {
                return _TypeDefinition.Events.Cast<TMemberDefinition>();
            }

            return null;
        }

        /// <summary>
        /// Removes the member name and following parameters (if there are some, else returns _Name).
        /// Example: System.String MyNamespace.MyType::MyMethod(...), returns System.String MyNamespace.MyType.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String RemoveMemberName(String _Name)
        {
            if (TypeDefinitionHelper.TryGetMemberNameStartIndex(_Name, out int var_MemberIndex))
            {
                _Name = _Name.Substring(0, var_MemberIndex - 2);
            }

            return _Name;
        }

        /// <summary>
        /// Returns true if _Name contains a member (method/field/property/event).
        /// _MemberIndex represents the index in _Name, where the member name after "::" starts.
        /// Example: System.String MyNamespace.MyType::MyMethod(...), returns true and _MemberIndex as 34.
        /// </summary>
        /// <param name="_Name"></param>
        /// <param name="_MemberIndex"></param>
        /// <returns></returns>
        public static bool TryGetMemberNameStartIndex(String _Name, out int _MemberIndex)
        {
            int var_LastIndex = _Name.LastIndexOf("::");

            if (var_LastIndex != -1)
            {
                _MemberIndex = var_LastIndex + 2;
                return true;
            }

            _MemberIndex = -1;
            return false;
        }

        #endregion

        // Method
        #region Method

        #endregion

        // Field
        #region Field

        /// <summary>
        /// Returns the FieldDefinition with the original name _OriginalFieldName.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_FieldCache"></param>
        /// <param name="_OriginalFieldName"></param>
        /// <returns></returns>
        public static FieldDefinition FindField(TypeDefinition _TypeDefinition, FieldCache _FieldCache, String _OriginalFieldName)
        {
            FieldDefinition var_FieldDefinition = FindField(_TypeDefinition.Fields, _FieldCache, _OriginalFieldName);

            if (var_FieldDefinition == null)
            {
                if (_TypeDefinition.BaseType == null)
                {
                    return null;
                }

                if (!MemberReferenceHelper.TryToResolve(_TypeDefinition.BaseType, out TypeDefinition var_BaseTypeDefinition))
                {
                    return null;
                }

                return FindField(var_BaseTypeDefinition, _FieldCache, _OriginalFieldName);
            }

            return var_FieldDefinition;
        }

        /// <summary>
        /// Returns the FieldDefinition with the original name _OriginalFieldName.
        /// </summary>
        /// <param name="_Fields"></param>
        /// <param name="_FieldCache"></param>
        /// <param name="_OriginalFieldName"></param>
        /// <returns></returns>
        public static FieldDefinition FindField(OPS.Mono.Collections.Generic.Collection<FieldDefinition> _Fields, FieldCache _FieldCache, String _OriginalFieldName)
        {
            foreach (FieldDefinition var_FieldDefinition in _Fields)
            {
                var var_Key = _FieldCache.GetOriginalMemberKeyByMemberDefinition(var_FieldDefinition);

                if (var_Key == null)
                {
                    continue;
                }

                if (var_Key.Name.Equals(_OriginalFieldName))
                {
                    return var_FieldDefinition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the FieldDefinition to _PropertyDefinition if there is some.
        /// Else returns null.
        /// </summary>
        /// <param name="_FieldCache"></param>
        /// <param name="_PropertyDefinition"></param>
        /// <returns></returns>
        public static FieldDefinition GetFieldBelongingToProperty(FieldCache _FieldCache, PropertyDefinition _PropertyDefinition)
        {
            // Example: <FirstName>k__BackingField
            String var_FieldName = "<" + _PropertyDefinition.Name + ">" + "k__BackingField";

            if (_PropertyDefinition.DeclaringType != null
                && _PropertyDefinition.DeclaringType.HasFields)
            {
                FieldDefinition var_FieldDefinition = FindField(_PropertyDefinition.DeclaringType, _FieldCache, var_FieldName);
                if (var_FieldDefinition != null)
                {
                    return var_FieldDefinition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the FieldDefinition to _EventDefinition if there is some.
        /// Else returns null.
        /// </summary>
        /// <param name="_FieldCache"></param>
        /// <param name="_EventDefinition"></param>
        /// <returns></returns>
        public static FieldDefinition GetFieldBelongingToEvent(FieldCache _FieldCache, EventDefinition _EventDefinition)
        {
            // Example: MyEvent
            String var_FieldName = _EventDefinition.Name;

            if (_EventDefinition.DeclaringType != null
                && _EventDefinition.DeclaringType.HasFields)
            {
                FieldDefinition var_FieldDefinition = FindField(_EventDefinition.DeclaringType, _FieldCache, var_FieldName);
                if (var_FieldDefinition != null)
                {
                    return var_FieldDefinition;
                }
            }
            return null;
        }

        #endregion

        // Property
        #region Property

        /// <summary>
        /// Returns the PropertyDefinition with the original name _OriginalPropertyName.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_PropertyCache"></param>
        /// <param name="_OriginalPropertyName"></param>
        /// <returns></returns>
        public static PropertyDefinition FindProperty(TypeDefinition _TypeDefinition, PropertyCache _PropertyCache, String _OriginalPropertyName)
        {
            PropertyDefinition var_PropertyDefinition = FindProperty(_TypeDefinition.Properties, _PropertyCache, _OriginalPropertyName);

            if (var_PropertyDefinition == null)
            {
                if (_TypeDefinition.BaseType == null)
                {
                    return null;
                }

                if (!MemberReferenceHelper.TryToResolve(_TypeDefinition.BaseType, out TypeDefinition var_BaseTypeDefinition))
                {
                    return null;
                }

                return FindProperty(var_BaseTypeDefinition, _PropertyCache, _OriginalPropertyName);
            }

            return var_PropertyDefinition;
        }

        /// <summary>
        /// Returns the PropertyDefinition with the original name _OriginalPropertyName.
        /// </summary>
        /// <param name="_Properties"></param>
        /// <param name="_PropertyCache"></param>
        /// <param name="_OriginalPropertyName"></param>
        /// <returns></returns>
        public static PropertyDefinition FindProperty(OPS.Mono.Collections.Generic.Collection<PropertyDefinition> _Properties, PropertyCache _PropertyCache, String _OriginalPropertyName)
        {
            foreach (PropertyDefinition var_PropertyDefinition in _Properties)
            {
                var var_Key = _PropertyCache.GetOriginalMemberKeyByMemberDefinition(var_PropertyDefinition);

                if (var_Key == null)
                {
                    continue;
                }

                if (var_Key.Name.Equals(_OriginalPropertyName))
                {
                    return var_PropertyDefinition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the PropertyDefinition belonging to _FieldDefinition if _FieldDefinition is created through a Property.
        /// If there is none returns null.
        /// </summary>
        /// <param name="_PropertyCache"></param>
        /// <param name="_FieldDefinition"></param>
        /// <returns></returns>
        public static PropertyDefinition GetPropertyBelongingToField(PropertyCache _PropertyCache, FieldDefinition _FieldDefinition)
        {
            if (_FieldDefinition.Name.Contains("k__BackingField"))
            {
                int var_Index = _FieldDefinition.Name.IndexOf('>');
                if (var_Index != -1 && var_Index >= 1)
                {
                    string var_PropertyName = _FieldDefinition.Name.Substring(1, var_Index - 1);
                    if (_FieldDefinition.DeclaringType != null
                        && _FieldDefinition.DeclaringType.HasProperties)
                    {
                        PropertyDefinition propertyDefinition = FindProperty(_FieldDefinition.DeclaringType, _PropertyCache, var_PropertyName);
                        if (propertyDefinition != null)
                        {
                            return propertyDefinition;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        // Event
        #region Event

        public static EventDefinition FindEvent(TypeDefinition _TypeDefinition, EventCache _EventCache, String _OriginalEventName)
        {
            EventDefinition var_EventDefinition = FindEvent(_TypeDefinition.Events, _EventCache, _OriginalEventName);

            if (var_EventDefinition == null)
            {
                if (_TypeDefinition.BaseType == null)
                {
                    return null;
                }

                if (!MemberReferenceHelper.TryToResolve(_TypeDefinition.BaseType, out TypeDefinition var_BaseTypeDefinition))
                {
                    return null;
                }

                return FindEvent(var_BaseTypeDefinition, _EventCache, _OriginalEventName);
            }

            return var_EventDefinition;
        }

        public static EventDefinition FindEvent(OPS.Mono.Collections.Generic.Collection<EventDefinition> _Events, EventCache _EventCache, String _OriginalEventName)
        {
            foreach (EventDefinition var_EventDefinition in _Events)
            {
                var var_Key = _EventCache.GetOriginalMemberKeyByMemberDefinition(var_EventDefinition);

                if (var_Key == null)
                {
                    continue;
                }

                if (var_Key.Name.Equals(_OriginalEventName))
                {
                    return var_EventDefinition;
                }
            }
            return null;
        }

        public static EventDefinition GetEventBelongingToField(EventCache _EventCache, FieldDefinition _FieldDefinition)
        {
            String var_EventName = _FieldDefinition.Name;
            if (_FieldDefinition.DeclaringType != null
                && _FieldDefinition.DeclaringType.HasProperties)
            {
                EventDefinition var_EventDefinition = FindEvent(_FieldDefinition.DeclaringType, _EventCache, var_EventName);
                if (var_EventDefinition != null)
                {
                    return var_EventDefinition;
                }
            }
            return null;
        }

        #endregion
    }
}
