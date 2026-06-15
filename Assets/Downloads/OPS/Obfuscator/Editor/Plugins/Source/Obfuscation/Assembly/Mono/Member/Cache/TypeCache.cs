using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Mdb;
using OPS.Mono.Cecil.Pdb;

// OPS - Obfuscator - Assemby
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assemby - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Extension
using OPS.Obfuscator.Editor.Extension;

// OPS - Obfuscator - Project
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    /// <summary>
    /// A cache of all types in the obfuscated assemblies.
    /// </summary>
    public class TypeCache : AMemberCache<TypeDefinition, TypeKey>
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the TypeCache using the assemblies being obfuscated from _Step. Only builds the cache for the types and its base types in the obfuscated assemblies.
        /// </summary>
        /// <param name="_Step"></param>
        public TypeCache(PostAssemblyBuildStep _Step)
            : base(_Step)
        {
            // Iterate all types in the obfuscated assemblies and build up the cache.
            foreach (AssemblyInfo var_AssemblyInfo in this.Step.DataContainer.ObfuscateAssemblyList)
            {
                foreach (TypeDefinition var_TypeDefinition in var_AssemblyInfo.GetAllTypeDefinitions())
                {
                    this.Add_MemberDefinition(var_TypeDefinition);
                }
            }
        }

        #endregion

        // Member
        #region Member

        /// <summary>
        /// Maps the original full name of a type ([Namespace].[TypeName]) to a list of type keys that share the same full name but could be in different assemblies.
        /// </summary>
        private readonly Dictionary<String, List<TypeKey>> fullNameToTypeKeyListDictionary = new Dictionary<string, List<TypeKey>>();

        /// <summary>
        /// Build up the cache for the type and its base types and interfaces.
        /// </summary>
        /// <param name="_TypeDefinition">The type to add and build up the cache for.</param>
        /// <returns>True if the type got added to the cache; otherwise false.</returns>
        protected override bool On_AddTo_MemberKey_And_MemberDefinition_Cache(TypeDefinition _TypeDefinition)
        {
            // Add the type to the full name look up dictionary.
            this.AddTypeToNameLookUp(_TypeDefinition);

            // Build up the hierarchy for the type.
            this.BuildBaseHierarchy(_TypeDefinition);

            // Build up the interfaces for the type.
            this.BuildInterfaceHierarchy(_TypeDefinition);

            return true;
        }

        /// <summary>
        /// Add the _TypeDefinition to the FullName to TypeKey List look up dictionary.
        /// </summary>
        /// <param name="_TypeDefinition">The type to add.</param>
        private void AddTypeToNameLookUp(TypeDefinition _TypeDefinition)
        {
            // Add to FullName to TypeKey List
            if (this.fullNameToTypeKeyListDictionary.TryGetValue(_TypeDefinition.FullName, out List<TypeKey> var_TypeKeyList))
            {
                // Add to existing list.
                var_TypeKeyList.Add(new TypeKey(_TypeDefinition));
            }
            else
            {
                // Add to new list.
                this.fullNameToTypeKeyListDictionary.Add(_TypeDefinition.FullName, new List<TypeKey>() { new TypeKey(_TypeDefinition) });
            }
        }

        /// <summary>
        /// Build up the base type hierarchy for the type.
        /// </summary>
        /// <param name="_TypeDefinition">The type to build up the hierarchy for.</param>
        private void BuildBaseHierarchy(TypeDefinition _TypeDefinition)
        {
            // Get the type key for the type.
            TypeKey var_TypeKey = new TypeKey(_TypeDefinition);

            // Check if the base type hierarchy is already build up for the type.
            if (this.BaseTypeDictionary.ContainsKey(var_TypeKey))
            {
                // Already added base hierachie. Return here.
                return;
            }

            // Check if the type has a base type.   
            if (_TypeDefinition.BaseType == null)
            {
                // Has no base type. Return here.
                return;
            }

            // Try to resolve the base type and build up the hierarchy.
            try
            {
                // Try to resolve base.
                TypeDefinition var_BaseTypeDefinition = this.Step.Resolve_TypeDefinition(_TypeDefinition.BaseType);
                if (var_BaseTypeDefinition == null)
                {
                    // Base cannot be resolved!
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Cannot resolve type '{0}' while building type hierarchy.", _TypeDefinition.BaseType.ToString()));

                    return;
                }

                // Add the type to the full name look up dictionary.
                this.AddTypeToNameLookUp(var_BaseTypeDefinition);

                // Add the type to its base type mapping.
                this.BaseTypeDictionary.Add(var_TypeKey, new TypeKey(var_BaseTypeDefinition));

                // Build up the interfaces for the base type.
                this.BuildInterfaceHierarchy(var_BaseTypeDefinition);

                // Recursively build up the base hierarchy for the base type.
                this.BuildBaseHierarchy(var_BaseTypeDefinition);
            }
            catch (Exception e)
            {
                // Base cannot be resolved!
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Cannot resolve type '{0}' from assembly '{1}' while building type hierarchy. Exception: {2}", _TypeDefinition.BaseType.FullName, _TypeDefinition.BaseType.GetScopeName(), e.ToString()));
            }
        }

        /// <summary>
        /// Build up the interfaces for the type.
        /// </summary>
        /// <param name="_TypeDefinition">The type to build up the interfaces for.</param>
        private void BuildInterfaceHierarchy(TypeDefinition _TypeDefinition)
        {
            // Get the type key for the type.
            TypeKey var_TypeKey = new TypeKey(_TypeDefinition);

            // Check if the interfaces are already added for the type.
            if (this.InterfacesDictionary.ContainsKey(var_TypeKey))
            {
                // Already added interfaces. Return here.
                return;
            }

            // Check if the type has interfaces.
            if (!_TypeDefinition.HasInterfaces)
            {
                // Has no interfaces. Return here.
                return;
            }

            // The interfaces for the type as type key list.
            List<TypeKey> var_InterfaceTypeKeyList = new List<TypeKey>();

            // Iterate all interfaces and add them to the list.
            foreach (InterfaceImplementation var_InterfaceImplementation in _TypeDefinition.Interfaces)
            {
                try
                {
                    // Try to resolve interface.
                    TypeDefinition var_InterfaceTypeDefinition = this.Step.Resolve_TypeDefinition(var_InterfaceImplementation.InterfaceType);
                    if (var_InterfaceTypeDefinition == null)
                    {
                        // Interface cannot be resolved!
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Cannot resolve type '{0}' while building type hierarchy.", var_InterfaceImplementation.InterfaceType.ToString()));

                        continue;
                    }

                    // Add to the full name look up dictionary.
                    this.AddTypeToNameLookUp(var_InterfaceTypeDefinition);

                    // Add to interface list.
                    var_InterfaceTypeKeyList.Add(new TypeKey(var_InterfaceTypeDefinition));

                    // Iterate the interfaces for the interface.
                    this.BuildInterfaceHierarchy(var_InterfaceTypeDefinition);
                }
                catch (Exception e)
                {
                    // Interface cannot be resolved!
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Cannot resolve type '{0}' from assembly '{1}' while building type hierarchy. Exception: {2}", var_InterfaceImplementation.InterfaceType.FullName, var_InterfaceImplementation.InterfaceType.GetScopeName(), e.ToString()));
                }
            }

            // Add the interfaces to the dictionary.
            this.InterfacesDictionary.Add(var_TypeKey, var_InterfaceTypeKeyList);
        }

        #endregion

        // Base Types
        #region Base Types

        /// <summary>
        /// Extendend Type Full Name, Extendend Base Type Full Name..
        /// </summary>
        public Dictionary<TypeKey, TypeKey> BaseTypeDictionary { get; private set; } = new Dictionary<TypeKey, TypeKey>();

        /// <summary>
        /// Extendend Type Full Name, Extendend Interface Full Name List.
        /// </summary>
        public Dictionary<TypeKey, List<TypeKey>> InterfacesDictionary { get; private set; } = new Dictionary<TypeKey, List<TypeKey>>();

        /// <summary>
        /// Search if the _TypeDefinition implements the _Interface.
        /// </summary>
        /// <param name="_TypeDefinition">The type to search for the interface.</param>
        /// <param name="_Interface">The interface to search for.</param>
        /// <returns>True if the type implements the interface; otherwise false.</returns>
        public bool HasInterface(TypeDefinition _TypeDefinition, TypeKey _Interface)
        {
            // Precheck
            if (_TypeDefinition == null)
            {
                return false;
            }

            // Get the original type key.
            TypeKey var_OriginalTypeKey = this.GetOriginalMemberKeyByMemberDefinition(_TypeDefinition);

            // If the original type key is null, the type is not part of the obfuscation context.
            if (var_OriginalTypeKey == null)
            {
                // Is not part of the obfuscation context. 

                // Create a new key for it.
                var_OriginalTypeKey = new TypeKey(_TypeDefinition);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Type '{0}' is not part of the obfuscation context!", var_OriginalTypeKey));
            }

            // Check if the type has the searched interface.
            return this.HasInterface(var_OriginalTypeKey, _Interface, new HashSet<TypeKey>());
        }

        /// <summary>
        /// Search if the _Type implements the _Interface.
        /// </summary>
        /// <param name="_Type">The type to search for the interface.</param>
        /// <param name="_Interface">The interface to search for.</param>
        /// <param name="_AlreadySearchedTypes">A hash set of already searched types to prevent cyclic dependencies.</param>
        /// <returns>True if the type implements the interface; otherwise false.</returns>
        private bool HasInterface(TypeKey _Type, TypeKey _Interface, HashSet<TypeKey> _AlreadySearchedTypes)
        {
            // Check if the type is already searched. Caused by cyclic dependencies. Should normally not happen.
            if (_AlreadySearchedTypes.Contains(_Type))
            {
                // Cyclic dependency!!!
                return false;
            }

            // Check if one of the interfaces is the searched one.
            if (this.InterfacesDictionary.TryGetValue(_Type, out List<TypeKey> var_Interfaces))
            {
                for (int i = 0; i < var_Interfaces.Count; i++)
                {
                    // Check if the iterated interface is the searched one.
                    if (var_Interfaces[i].Equals(_Interface))
                    {
                        return true;
                    }

                    // Check if the iterated interface has the searched interface.
                    if (this.HasInterface(var_Interfaces[i], _Interface, _AlreadySearchedTypes))
                    {
                        return true;
                    }
                }
            }

            // Add to already searched.
            _AlreadySearchedTypes.Add(_Type);

            return false;
        }

        /// <summary>
        /// Check here if a _TypeDefinition inherits from _FromType (Base type or interfaces). The assembly, namespace and type name will be compared.
        /// </summary>
        /// <param name="_TypeDefinition">The type to check if it inherits from _FromType.</param>
        /// <param name="_FromType">The type to check if it is inherited from.</param>
        /// <param name="_CheckInterfaces">True if the interfaces should be checked; otherwise false.</param>
        /// <returns>True if the type inherits from the _FromType; otherwise false.</returns>
        public bool InheritesFrom(TypeDefinition _TypeDefinition, TypeKey _FromType, bool _CheckInterfaces = true)
        {
            // Precheck
            if (_TypeDefinition == null)
            {
                return _FromType == null;
            }

            // Types full name.
            TypeKey var_OriginalTypeKey = this.GetOriginalMemberKeyByMemberDefinition(_TypeDefinition);

            if (var_OriginalTypeKey == null)
            {
                // Is not part of the obfuscation context. 

                // Create a new key for it.
                var_OriginalTypeKey = new TypeKey(_TypeDefinition);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Type " + var_OriginalTypeKey + " is not part of the obfuscation context!");
            }

            // Build target type.
            TypeKey var_OriginalTargetTypeKey = _FromType;

            // Check if the _TypeDefinition is already the searched one. 
            if (var_OriginalTypeKey.Equals(var_OriginalTargetTypeKey))
            {
                return true;
            }

            // Do not iterate types twice!
            HashSet<TypeKey> var_AlreadySearchedHashSet = new HashSet<TypeKey>
            {
                var_OriginalTypeKey
            };

            // Iterate until base found.
            TypeKey var_OriginalBaseTypeKey = var_OriginalTypeKey;

            while (this.BaseTypeDictionary.TryGetValue(var_OriginalBaseTypeKey, out var_OriginalBaseTypeKey))
            {
                // Check if the _TypeDefinition is the searched one. 
                if (var_OriginalBaseTypeKey.Equals(var_OriginalTargetTypeKey))
                {
                    return true;
                }

                // Check it the type is already searched. Caused by cyclic dependencies. Should normally not happen.
                if (var_AlreadySearchedHashSet.Contains(var_OriginalBaseTypeKey))
                {
                    // Cyclic dependency!!!
                    return false;
                }

                // Add to already searched.
                var_AlreadySearchedHashSet.Add(var_OriginalBaseTypeKey);
            }

            // If interfaces should be checked, check if the type has the searched interface.
            if (_CheckInterfaces)
            {
                // Check if the type has the searched interface.
                if (this.HasInterface(var_OriginalTypeKey, var_OriginalTargetTypeKey, new HashSet<TypeKey>()))
                {
                    return true;
                }
            }

            // TODO: Add fail safe, if _TypeDefinition is not added, so iterate bases still! and check if they are in BaseTypeDictionary.
            return false;
        }

        /// <summary>
        /// Check here if a _TypeDefinition inherits from type with original _FullName. Only the namespace and type name will be compared.
        /// </summary>
        /// <param name="_TypeDefinition">The type to check if it inherits from _FullName.</param>
        /// <param name="_FullName">The full name of the type to check if it is inherited from.</param>
        /// <param name="_CheckInterfaces">True if the interfaces should be checked; otherwise false.</param>
        /// <returns>Ttrue if the type inherits from the _FullName; otherwise false.</returns>
        public bool InheritesFrom(TypeDefinition _TypeDefinition, String _FullName, bool _CheckInterfaces = true)
        {
            // Precheck
            if (_TypeDefinition == null)
            {
                return String.IsNullOrEmpty(_FullName);
            }

            // Iterate all TypeKey sharing the same _FullName.
            if (this.fullNameToTypeKeyListDictionary.TryGetValue(_FullName, out List<TypeKey> var_TypeKeyList))
            {
                for (int k = 0; k < var_TypeKeyList.Count; k++)
                {
                    // Check if _TypeDefinition inherits from some of those TypeKeys.
                    if (this.InheritesFrom(_TypeDefinition, var_TypeKeyList[k], _CheckInterfaces))
                    {
                        return true;
                    }
                }
            }

            // TODO: Add fail safe, if _TypeDefinition is not added, so iterate bases still! and check if they are in BaseTypeDictionary.
            return false;
        }

        #endregion
    }
}
