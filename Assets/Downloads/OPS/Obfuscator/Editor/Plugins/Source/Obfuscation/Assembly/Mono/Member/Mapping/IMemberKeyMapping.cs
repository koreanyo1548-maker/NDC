using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Obfuscator - Assembly - Member 
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping
{
    /// <summary>
    /// Maps a member key to a value object.
    /// </summary>
    public interface IMemberKeyMapping: IEnumerable<KeyValuePair<IMemberKey, String>>
    {
        /// <summary>
        /// Add or override for _Key the _Value.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        bool Add(IMemberKey _Key, String _Value, bool _Override = false);

        /// <summary>
        /// Get the value for _Key.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        String Get(IMemberKey _Key);

        /// <summary>
        /// Get all the member keys sharing the same _Value.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        List<IMemberKey> GetAllMemberKeys(String _Value);

        /// <summary>
        /// Get all the member keys sharing the same _Value as list of TMemberKey.
        /// </summary>
        /// <typeparam name="TMemberKey"></typeparam>
        /// <param name="_Value"></param>
        /// <returns></returns>
        List<TMemberKey> GetAllMemberKeys<TMemberKey>(String _Value);
    }
}
