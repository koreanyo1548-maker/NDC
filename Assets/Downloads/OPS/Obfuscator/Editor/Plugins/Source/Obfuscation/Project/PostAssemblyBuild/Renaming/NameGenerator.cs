using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming
{
    /// <summary>
    /// Generator used to generate a obfuscated name.
    /// </summary>
    public class NameGenerator
    {
        public NameGenerator()
        {
            // Init Charset
            UseCharSet(new Charset.DefaultCharset());

            // Init Indexes
            this.groupedRenamingIndexDictionary.Add(EMemberType.Namespace, 0);
            this.groupedRenamingIndexDictionary.Add(EMemberType.Type, 0);
            this.groupedRenamingIndexDictionary.Add(EMemberType.Method, 0);
            this.groupedRenamingIndexDictionary.Add(EMemberType.Field, 0);
            this.groupedRenamingIndexDictionary.Add(EMemberType.Property, 0);
            this.groupedRenamingIndexDictionary.Add(EMemberType.Event, 0);
        }

        //Charset
        #region Charset

        /// <summary>
        /// All unique charaters in this charset.
        /// </summary>
        private string uniqueChars;

        /// <summary>
        /// The length of charset.
        /// </summary>
        private uint numUniqueChars;

        /// <summary>
        /// Assign a charset used for obfuscation.
        /// </summary>
        /// <param name="_Charset"></param>
        public void UseCharSet(Charset.ACharset _Charset)
        {
            if (_Charset == null)
            {
                throw new ArgumentNullException("_Charset");
            }

            uniqueChars = _Charset.Characters;

            numUniqueChars = (uint)uniqueChars.Length;
        }

        #endregion

        //Name Generation
        #region Name Generation

        /// <summary>
        /// True: No obfuscated member will share the same name.
        /// False: Obfuscated member of different member types may share the same name.
        /// </summary>
        public bool UseGlobalIndexer { get; private set; } = true;

        /// <summary>
        /// Global index for obfuscation.
        /// </summary>
        private uint globalRenamingIndex = 0;

        /// <summary>
        /// Dictionary between member and its index.
        /// </summary>
        private Dictionary<EMemberType, uint> groupedRenamingIndexDictionary = new Dictionary<EMemberType, uint>();

        /// <summary>
        /// Returns the next index.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <returns></returns>
        private uint GetNextIndex(EMemberType _MemberType)
        {
            if(this.UseGlobalIndexer)
            {
                return this.globalRenamingIndex++;
            }

            return this.groupedRenamingIndexDictionary[_MemberType]++;
        }

        /// <summary>
        /// Generate a next name for a member using a predefined pattern.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public String GetNextName(EMemberType _MemberType, IMemberDefinition _MemberDefinition)
        {
            // Next index for the name.
            uint var_NextIndex = this.GetNextIndex(_MemberType);

            // Create new name.
            String var_UniqueName = this.GetNextName(var_NextIndex);

            // Is Type, check if generic, add generic parameter.
            if(_MemberType == EMemberType.Type && _MemberDefinition is TypeDefinition)
            {
                TypeDefinition var_TypeDefinition = (TypeDefinition) _MemberDefinition;

                int var_GenericParameters = var_TypeDefinition.GenericParameters.Count - (var_TypeDefinition.DeclaringType != null ? var_TypeDefinition.DeclaringType.GenericParameters.Count : 0);

                if (var_GenericParameters > 0)
                {
                    var_UniqueName += '`' + var_GenericParameters.ToString();
                }
            }

            return var_UniqueName;
        }

        /// <summary>
        /// Generate a next name under a specific index using a predefined pattern.
        /// Use with caution! Watch out for same names in the same type.
        /// </summary>
        /// <param name="_Index"></param>
        /// <returns></returns>
        public string GetNextName(uint _Index)
        {
            return GetNextName(_Index, null);
        }

        /// <summary>
        /// Generate a new name under a specific index using a predefined pattern.
        /// After each character a _Seperator can/will be added.
        /// Use with caution! Watch out for same names in the same type.
        /// </summary>
        /// <param name="_Index"></param>
        /// <param name="_Seperator"></param>
        /// <returns></returns>
        private string GetNextName(uint _Index, string _Seperator)
        {
            // Throw exception if no characters!
            if (this.numUniqueChars == 0)
            {
                throw new Exception("The renaming pattern contains no characters!");
            }

            // Optimization for simple case
            if (_Index < this.numUniqueChars)
            {
                return this.uniqueChars[(int)_Index].ToString();
            }

            Stack<char> stack = new Stack<char>();

            do
            {
                // Add next character.
                stack.Push(this.uniqueChars[(int)(_Index % this.numUniqueChars)]);

                // Is last, so break.
                if (_Index < this.numUniqueChars)
                {
                    break;
                }

                // Special case, if numUniqueChars == 1, modulo or devision does not work!
                // Plus 1 is needed, because _Index = 1, are 2 characters!
                if (this.numUniqueChars == 1
                    && _Index + 1 == stack.Count)
                {
                    break;
                }

                // Remove modulo part by devision.
                _Index /= this.numUniqueChars;
            } while (true);

            // Build single string.
            StringBuilder builder = new StringBuilder();
            builder.Append(stack.Pop());
            while (stack.Count > 0)
            {
                if (_Seperator != null)
                {
                    builder.Append(_Seperator);
                }
                builder.Append(stack.Pop());
            }

            return builder.ToString();
        }

        #endregion
    }
}
