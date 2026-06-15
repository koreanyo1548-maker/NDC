using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Key
{
    /// <summary>
    /// Used as key for types.
    /// </summary>
    public class TypeKey : IMemberKey
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private TypeKey()
        {

        }

        public TypeKey(TypeReference _TypeReference)
        {
            if (_TypeReference == null)
            {
                throw new ArgumentNullException("_TypeReference");
            }

            try
            {
                TypeDefinition var_TypeDefinition = _TypeReference.Resolve();

                // Assembly
                this.Assembly = TypeReferenceHelper.GetScopeName(var_TypeDefinition);

                // Type
                this.FullName = var_TypeDefinition.FullName;
                this.Namespace = var_TypeDefinition.Namespace;
                this.Name = var_TypeDefinition.Name;

                // GenericParameter
                if (IMemberDefinitionHelper.TryGetGenericParameter(this.Name, out int var_GenericParameterCount))
                {
                    this.GenericParameterCount = var_GenericParameterCount;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not resolve: " + _TypeReference.FullName + " in " + _TypeReference.Scope.Name + ". Please add the assemblies directory as dependency. Error: " + e.ToString());
            }
        }

        public TypeKey(TypeDefinition _TypeDefinition)
        {
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition");
            }

            // Assembly
            this.Assembly = TypeReferenceHelper.GetScopeName(_TypeDefinition);

            //Type
            this.FullName = _TypeDefinition.FullName;
            this.Namespace = _TypeDefinition.Namespace;
            this.Name = _TypeDefinition.Name;

            // GenericParameter
            if (IMemberDefinitionHelper.TryGetGenericParameter(this.Name, out int var_GenericParameterCount))
            {
                this.GenericParameterCount = var_GenericParameterCount;
            }
        }

        public TypeKey(Type _Type)
        {
            if (_Type == null)
            {
                throw new ArgumentNullException("_Type");
            }

            // Assembly
            this.Assembly = _Type.Assembly.GetName().Name;

            // Type
            // dotNet uses a + sign instead of a / sign like mono for nested class path.
            this.FullName = _Type.FullName.Replace("+", "/");
            this.Namespace = _Type.Namespace;
            this.Name = _Type.Name;

            // GenericParameter - Valid for Type also! It uses the "`" for generic parameter too.
            if (IMemberDefinitionHelper.TryGetGenericParameter(_Type.Name, out int var_GenericParameterCount))
            {
                this.GenericParameterCount = var_GenericParameterCount;
            }
        }

        public TypeKey(String _Assembly, String _FullName)
        {
            if (String.IsNullOrEmpty(_Assembly))
            {
                throw new ArgumentNullException("_Assembly");
            }
            if (String.IsNullOrEmpty(_FullName))
            {
                throw new ArgumentNullException("_FullName");
            }

            // Assembly
            this.Assembly = _Assembly;

            // Type
            // Remove attached member and return type if they are there.
            this.FullName = IMemberDefinitionHelper.RemoveReturnType(TypeDefinitionHelper.RemoveMemberName(_FullName));

            // Split Namespace and Name.
            this.Namespace = IMemberDefinitionHelper.GetMemberName(EMemberType.Namespace, this.FullName);
            this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Type, this.FullName);

            // GenericParameter
            if (IMemberDefinitionHelper.TryGetGenericParameter(this.Name, out int var_GenericParameterCount))
            {
                this.GenericParameterCount = var_GenericParameterCount;
            }
        }

        public TypeKey(String _Assembly, String _Namespace, String _Name)
        {
            if (String.IsNullOrEmpty(_Assembly))
            {
                throw new ArgumentNullException("_Assembly");
            }
            if (String.IsNullOrEmpty(_Name))
            {
                throw new ArgumentNullException("_Name");
            }

            // Assembly
            this.Assembly = _Assembly;

            // FullName
            this.FullName = String.IsNullOrEmpty(_Namespace) ? _Name : _Namespace + "." + _Name;

            // Split Namespace and Name.
            this.Namespace = _Namespace;
            this.Name = _Name;

            // GenericParameter
            if (IMemberDefinitionHelper.TryGetGenericParameter(this.Name, out int var_GenericParameterCount))
            {
                this.GenericParameterCount = var_GenericParameterCount;
            }
        }

        #endregion

        // Assembly
        #region Assembly

        /// <summary>
        /// Simplified name of the assembly this Type is in.
        /// </summary>
        public String Assembly { get; private set; }

        #endregion

        // Name
        #region Name

        /// <summary>
        /// Fullname of this type.
        /// </summary>
        public String FullName { get; private set; }

        /// <summary>
        /// Namespace only of the type. (Nested have NONE!)
        /// </summary>
        public String Namespace { get; private set; }

        /// <summary>
        /// Name only of the type.
        /// </summary>
        public String Name { get; private set; }

        #endregion

        // GenericParameter
        #region GenericParameter

        /// <summary>
        /// Returns the count of GenericParameters the Type has.
        /// </summary>
        public int GenericParameterCount { get; private set; }

        #endregion

        // Equals
        #region Equals

        public override bool Equals(object _Object)
        {
            return this.Equals(_Object as TypeKey);
        }

        public bool Equals(TypeKey _TypeKey)
        {
            return _TypeKey != null && this.Assembly.Equals(_TypeKey.Assembly) && this.FullName.Equals(_TypeKey.FullName);
        }

        public override int GetHashCode()
        {
            return this.Assembly.GetHashCode() ^ this.FullName.GetHashCode();
        }

        #endregion

        // Operator
        #region Operator

        public static bool operator ==(TypeKey _A, TypeKey _B)
        {
            if (ReferenceEquals(_A, _B))
            {
                return true;
            }
            if (ReferenceEquals(_A, null))
            {
                return false;
            }
            if (ReferenceEquals(_B, null))
            {
                return false;
            }

            return _A.Equals(_B);
        }

        public static bool operator !=(TypeKey _A, TypeKey _B)
        {
            return !(_A == _B);
        }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// Serialize to _Value.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        public bool Serialize(out string _Value)
        {
            StringBuilder var_Serializer = new StringBuilder();

            var_Serializer.Append(this.Assembly);
            var_Serializer.Append("|");
            var_Serializer.Append(this.FullName);

            _Value = var_Serializer.ToString();

            return true;
        }

        /// <summary>
        /// Deserialize from _Value.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        public bool Deserialize(string _Value)
        {
            if (_Value == null)
            {
                throw new ArgumentNullException("_Value");
            }

            // Seperator Pipe
            String[] var_Pipe = _Value.Split('|');

            if (var_Pipe.Length != 2)
            {
                return false;
            }

            // Get the Assembly and FullName.
            String var_Assembly = var_Pipe[0];
            String var_FullName = var_Pipe[1];

            // Assign fields.
            this.Assembly = var_Assembly;

            this.FullName = var_FullName;

            this.Namespace = IMemberDefinitionHelper.GetMemberName(EMemberType.Namespace, var_FullName);
            this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Type, var_FullName);

            return true;
        }

        #endregion

        // ToString
        #region ToString

        public override string ToString()
        {
            StringBuilder var_Builder = new StringBuilder();

            var_Builder.Append("[");
            var_Builder.Append(this.Assembly);
            var_Builder.Append("]");
            var_Builder.Append(" ");
            var_Builder.Append(this.FullName);

            return var_Builder.ToString();
        }

        #endregion
    }
}
