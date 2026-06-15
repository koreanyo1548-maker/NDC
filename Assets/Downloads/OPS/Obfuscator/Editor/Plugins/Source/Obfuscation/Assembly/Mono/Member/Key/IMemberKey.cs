using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Key
{
    public interface IMemberKey
    {
        // Assembly
        #region Assembly

        /// <summary>
        /// The Assembly the member is in.
        /// </summary>
        String Assembly { get; }

        #endregion

        // Name
        #region Name

        /// <summary>
        /// The full name of the member.
        /// </summary>
        String FullName { get; }

        /// <summary>
        /// The name of the member.
        /// </summary>
        String Name { get; }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// Serialize this key to _Value.
        /// (Needs an empty constructor!)
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        bool Serialize(out String _Value);

        /// <summary>
        /// Deserialize this key from _Value.
        /// (Needs an empty constructor!)
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        bool Deserialize(String _Value);

        #endregion
    }
}
