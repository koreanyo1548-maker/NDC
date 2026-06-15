using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly
{
    /// <summary>
    /// Contains informations about to load assemblies.
    /// </summary>
    public class AssemblyLoadInfo
    {
        // Constructor
        #region Constructor

        public AssemblyLoadInfo()
        {

        }

        #endregion

        //File
        #region File

        /// <summary>
        /// Full file path to the assembly.
        /// </summary>
        public String FilePath { get; set; }

        #endregion

        //Assembly
        #region Assembly

        /// <summary>
        /// True: Is a assembly build through unity. Those assemblies dont need a a backup.
        /// </summary>
        public bool IsUnityAssembly { get; set; }

        /// <summary>
        /// True: Is a assembly imported to unity. Those assemblies need a a backup.
        /// </summary>
        public bool IsThirdPartyAssembly
        {
            get { return !this.IsUnityAssembly; }
        }

        /// <summary>
        /// True: A assembly that wont get obfuscated, but gets referenced and may be used by obfuscation components.
        /// </summary>
        public bool IsHelperAssembly { get; set; }

        #endregion

        //Obfuscation
        #region Obfuscation

        /// <summary>
        /// True: Gets obfuscated.
        /// </summary>
        public bool Obfuscate { get; set; }

        #endregion
    }
}
