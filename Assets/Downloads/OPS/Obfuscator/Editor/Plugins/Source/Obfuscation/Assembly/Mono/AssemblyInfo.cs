using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Rocks;

namespace OPS.Obfuscator.Editor.Assembly
{
    /// <summary>
    /// Contains the AssemblyDefinition and infos over the TypeDefinitions.
    /// </summary>
    public class AssemblyInfo
    {
        //Load Info
        #region Load Info

        /// <summary>
        /// Belonging AssemblyLoadInfo. Contains informations about how to process the assembly.
        /// </summary>
        public AssemblyLoadInfo AssemblyLoadInfo { get; internal set; }

        #endregion

        //Assembly
        #region Assembly

        /// <summary>
        /// Loaded AssemblyDefinition.
        /// </summary>
        public AssemblyDefinition AssemblyDefinition { get; internal set; }

        #endregion

        //Types
        #region Types

        /// <summary>
        /// Returns an ordered list of TypeDefinitions.
        /// The list is ordered in a way, that first the nested TypeDefinitions are added, than the TypeDefinition itself.
        /// Helps for the obfuscation.
        /// </summary>
        /// <param name="_UnsortedList"></param>
        /// <returns></returns>
        private List<TypeDefinition> SortTypes(IEnumerable<TypeDefinition> _UnsortedList)
        {
            List<TypeDefinition> var_TypeList = new List<TypeDefinition>();
            foreach (var var_Type in _UnsortedList)
            {
                this.RecursiveSortTypes(var_Type, var_TypeList);
            }
            return var_TypeList;
        }

        /// <summary>
        /// Adds the _CurrentType to the _SortedList.
        /// The list is ordered in a way, that first the nested TypeDefinitions are added, than the TypeDefinition itself.
        /// Helps for the obfuscation.
        /// </summary>
        /// <param name="_CurrentType"></param>
        /// <param name="_SortedList"></param>
        private void RecursiveSortTypes(TypeDefinition _CurrentType, List<TypeDefinition> _SortedList)
        {
            if(_CurrentType.FullName == "<Module>")
            {
                return;
            }

            if (_CurrentType.FullName.Contains("<PrivateImplementationDetails>"))
            {
                return;
            }

            if (_CurrentType.HasNestedTypes)
            {
                foreach (var var_NestedType in _CurrentType.NestedTypes)
                {
                    this.RecursiveSortTypes(var_NestedType, _SortedList);
                }
            }

            if (_SortedList.Contains(_CurrentType))
            {
                return;
            }

            _SortedList.Add(_CurrentType);
        }

        /// <summary>
        /// Cached and sorted types in this AssemblyDefinition.
        /// </summary>
        private IEnumerable<TypeDefinition> cachedTypes;

        /// <summary>
        /// Get all types in the AssemblyDefinition, sort them an assign them to cachedTypes.
        /// </summary>
        private void ResortTypes()
        {
            var result = Editor.Assembly.Mono.Helper.AssemblyHelper.GetAllTypes(this.AssemblyDefinition); // this.AssemblyDefinition.MainModule.GetAllTypes();

            this.cachedTypes = this.SortTypes(result);
        }

        /// <summary>
        /// Returns all TypeDefinition in this Assembly sorted.
        /// This is cached!
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TypeDefinition> GetAllTypeDefinitions()
        {
            if (this.cachedTypes != null)
            {
                return this.cachedTypes;
            }

            try
            {
                //Get all types and sort them.
                this.ResortTypes();

                //Return the sorted types stored in cachedTypes.
                return this.cachedTypes;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to get type definitions for {0}", this.AssemblyDefinition.Name), e);
            }
        }

        /// <summary>
        /// Returns true if the AssemblyDefinition contains _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public bool HasTypeDefinition(TypeDefinition _TypeDefinition)
        {
            return this.HasTypeDefinition(_TypeDefinition.Name, _TypeDefinition.Name);
        }

        /// <summary>
        /// Returns true if the AssemblyDefinition contains a TypeDefinition with _Namespace and _Name.
        /// </summary>
        /// <param name="_Namespace"></param>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public bool HasTypeDefinition(String _Namespace, String _Name)
        {
            foreach (TypeDefinition var_TypeDefinition in this.GetAllTypeDefinitions())
            {
                if (var_TypeDefinition.Namespace == _Namespace && var_TypeDefinition.Name == _Name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the _TypeDefinition only to the AssemblyDefinitions _ModuleDefinition and then resorts this assembly type cache!
        /// </summary>
        /// <param name="_ModuleDefinition"></param>
        /// <param name="_TypeDefinition"></param>
        internal void AddTypeDefinition(ModuleDefinition _ModuleDefinition, TypeDefinition _TypeDefinition)
        {
            //Add to _ModuleDefinition.
            _ModuleDefinition.Types.Add(_TypeDefinition);

            //Resort cache.
            this.ResortTypes();
        }

        #endregion

        //Name
        #region Name

        /// <summary>
        /// Returns the Name of the AssemblyDefinition.
        /// </summary>
        public String Name
        {
            get
            {
                return this.AssemblyDefinition.Name.Name;
            }
        }

        #endregion

        //Compare
        #region Compare

        public bool Equals(AssemblyInfo _Other)
        {
            return _Other != null &&
                   this.Name == _Other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is AssemblyInfo ? Equals((AssemblyInfo)obj) : false;
        }

        public static bool operator ==(AssemblyInfo a, AssemblyInfo b)
        {
            if ((object)a == null)
                return (object)b == null;
            else if ((object)b == null)
                return false;
            else
                return a.Equals(b);
        }

        public static bool operator !=(AssemblyInfo a, AssemblyInfo b)
        {
            if ((object)a == null)
                return (object)b != null;
            else if ((object)b == null)
                return true;
            else
                return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }
}
