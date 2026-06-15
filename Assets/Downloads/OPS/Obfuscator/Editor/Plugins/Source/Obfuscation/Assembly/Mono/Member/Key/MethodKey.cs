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
    /// Used as key for methods.
    /// </summary>
    public class MethodKey : IMemberKey
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private MethodKey()
        {

        }

        public MethodKey(MethodReference _MethodReference)
        {
            if(_MethodReference == null)
            {
                throw new ArgumentNullException("_MethodReference");
            }

            try
            {
                MethodDefinition var_MethodDefinition = _MethodReference.Resolve();

                // Type
                this.Type = new TypeKey(var_MethodDefinition.DeclaringType);

                // Method
                this.FullName = var_MethodDefinition.FullName;
                this.Name = _MethodReference.Name;

                // GenericParameter
                this.GenericParameterCount = var_MethodDefinition.GenericParameters.Count;
            }
            catch(Exception e)
            {
                throw new Exception("Could not resolve: " + _MethodReference.FullName + " in " + _MethodReference.DeclaringType.Scope.Name + ". Please add the assemblies directory as dependency. Error: " + e.ToString());
            }
        }

        public MethodKey(MethodDefinition _MethodDefinition)
        {
            if (_MethodDefinition == null)
            {
                throw new ArgumentNullException("_MethodDefinition");
            }

            // Type
            this.Type = new TypeKey(_MethodDefinition.DeclaringType);

            // Method
            this.FullName = _MethodDefinition.FullName;
            this.Name = _MethodDefinition.Name;

            // GenericParameter
            this.GenericParameterCount = _MethodDefinition.GenericParameters.Count;
        }

        public MethodKey(String _Assembly, String _FullName)
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

            this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Event, _FullName);
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

        // GenericParameter
        #region GenericParameter

        /// <summary>
        /// Returns the count of GenericParameters the Method has.
        /// </summary>
        public int GenericParameterCount { get; private set; }

        #endregion

        // Equals
        #region Equals

        public override bool Equals(object _Object)
        {
            return this.Equals(_Object as MethodKey);
        }

        public bool Equals(MethodKey _MethodKey)
        {
            return _MethodKey != null && this.Assembly.Equals(_MethodKey.Assembly) && this.Serialization_FullName.Equals(_MethodKey.Serialization_FullName);
        }

        public override int GetHashCode()
        {
            return this.Assembly.GetHashCode() ^ this.Serialization_FullName.GetHashCode();
        }

        #endregion

        // Operator
        #region Operator

        public static bool operator ==(MethodKey _A, MethodKey _B)
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

        public static bool operator !=(MethodKey _A, MethodKey _B)
        {
            return !(_A == _B);
        }

        #endregion

        // Serialization
        #region Serialization

        private String serialization_FullName;

        /// <summary>
        /// Get: Creates Property Serialization_FullName and assigns to Field serialization_FullName.
        /// Set: Extracts and assigns to Field FullName, Name and GenericParameterCount.
        /// </summary>
        private String Serialization_FullName
        {
            get
            {
                if (this.serialization_FullName == null)
                {
                    if (this.GenericParameterCount == 0)
                    {
                        // No generic parameter, so serialization fullname is normal fullname.
                        this.serialization_FullName = this.FullName;
                    }
                    else
                    {
                        // Has generic parameter, add between name and parameter.

                        // Extract method parameter.
                        String var_Parameter = "";
                        String var_Serialization_FullName_Without_Parameter = "";
                        if (MethodDefinitionHelper.TryGetMethodParameterStartIndex(this.FullName, out int var_ParameterIndex))
                        {
                            var_Parameter = this.FullName.Substring(var_ParameterIndex - 1);

                            var_Serialization_FullName_Without_Parameter = this.FullName.Substring(0, var_ParameterIndex - 1);
                        }

                        // Add generic parameter.
                        String var_GenericParameter = this.GenericParameterCount == 0 ? "" : "`" + this.GenericParameterCount;
                        var_Serialization_FullName_Without_Parameter += var_GenericParameter;

                        // Add method parameter again.
                        this.serialization_FullName = var_Serialization_FullName_Without_Parameter + var_Parameter;
                    }
                }

                return this.serialization_FullName;
            }
            set
            {
                // Assign Serialization FullName
                this.serialization_FullName = value;

                // Extract method parameter.
                String var_Parameter = "";
                String var_FullName_Without_Parameter = "";
                if (MethodDefinitionHelper.TryGetMethodParameterStartIndex(this.serialization_FullName, out int var_ParameterIndex))
                {
                    var_Parameter = this.serialization_FullName.Substring(var_ParameterIndex - 1);

                    var_FullName_Without_Parameter = this.serialization_FullName.Substring(0, var_ParameterIndex - 1);
                }

                // Extract generic parameter.
                if(IMemberDefinitionHelper.TryGetGenericParameter(var_FullName_Without_Parameter, out int var_GenericParameterIndex))
                {
                    this.GenericParameterCount = var_GenericParameterIndex;
                }

                // Remove generic parameter.
                String var_FullName_Without_Parameter_Without_GenericParameter = IMemberDefinitionHelper.RemoveGenericParameter(var_FullName_Without_Parameter);

                // Create FullName.
                this.FullName = var_FullName_Without_Parameter_Without_GenericParameter + var_Parameter;

                // Extract Name.
                this.Name = IMemberDefinitionHelper.GetMemberName(EMemberType.Method, this.FullName);
            }
        }

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
            var_Serializer.Append(this.Serialization_FullName);

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
            String var_Serialization_FullName = var_Pipe[1];

            // Assign Type.
            this.Type = new TypeKey(var_Assembly, TypeDefinitionHelper.RemoveMemberName(var_Serialization_FullName));

            // Assign Method.
            this.Serialization_FullName = var_Serialization_FullName;

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
            var_Builder.Append(this.Serialization_FullName);

            return var_Builder.ToString();
        }

        #endregion
    }
}
