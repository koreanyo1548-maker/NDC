using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Key
{
    /// <summary>
    /// Used as key for fields.
    /// </summary>
    public class FieldKey : IMemberKey
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private FieldKey()
        {

        }

        public FieldKey(FieldReference _FieldReference)
        {
            if(_FieldReference == null)
            {
                throw new ArgumentNullException("_FieldReference");
            }

            try
            {
                FieldDefinition var_FieldDefinition = _FieldReference.Resolve();

                this.Type = new TypeKey(var_FieldDefinition.DeclaringType);

                this.FullName = var_FieldDefinition.FullName;
                this.Name = _FieldReference.Name;
            }
            catch(Exception e)
            {
                throw new Exception("Could not resolve: " + _FieldReference.FullName + " in " + _FieldReference.DeclaringType.Scope.Name + ". Please add the assemblies directory as dependency. Error: " + e.ToString());
            }
        }

        public FieldKey(FieldDefinition _FieldDefinition)
        {
            if (_FieldDefinition == null)
            {
                throw new ArgumentNullException("_FieldDefinition");
            }

            this.Type = new TypeKey(_FieldDefinition.DeclaringType);

            this.FullName = _FieldDefinition.FullName;
            this.Name = _FieldDefinition.Name;
        }

        public FieldKey(String _Assembly, String _FullName)
        {
            if (_Assembly == null)
            {
                throw new ArgumentNullException("_Assembly");
            }
            if (_FullName == null)
            {
                throw new ArgumentNullException("_FullName");
            }

            this.Type = new TypeKey(_Assembly, _FullName);

            this.FullName = _FullName;

            this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Field, _FullName);
        }

        #endregion

        // Type
        #region Type

        /// <summary>
        /// The Type the method belongs too.
        /// </summary>
        public TypeKey Type { get; private set; }

        #endregion

        // Assembly
        #region Assembly

        /// <summary>
        /// Assembly this member is in.
        /// </summary>
        public String Assembly
        {
            get
            {
                return this.Type.Assembly;
            }
        }

        #endregion

        // Name
        #region Name

        /// <summary>
        /// Fullname of this method.
        /// </summary>
        public String FullName { get; private set; }

        /// <summary>
        /// Name only of the method.
        /// </summary>
        public String Name { get; private set; }

        #endregion

        // Equals
        #region Equals

        public override bool Equals(object _Object)
        {
            return this.Equals(_Object as FieldKey);
        }

        public bool Equals(FieldKey _FieldKey)
        {
            return _FieldKey != null && this.Assembly.Equals(_FieldKey.Assembly) && this.FullName.Equals(_FieldKey.FullName);
        }

        public override int GetHashCode()
        {
            return this.Assembly.GetHashCode() ^ this.FullName.GetHashCode();
        }

        #endregion

        // Operator
        #region Operator

        public static bool operator ==(FieldKey _A, FieldKey _B)
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

        public static bool operator !=(FieldKey _A, FieldKey _B)
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
            this.Type = new TypeKey(var_Assembly, TypeDefinitionHelper.RemoveMemberName(var_FullName));

            this.FullName = var_FullName;

            this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Field, var_FullName);

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
