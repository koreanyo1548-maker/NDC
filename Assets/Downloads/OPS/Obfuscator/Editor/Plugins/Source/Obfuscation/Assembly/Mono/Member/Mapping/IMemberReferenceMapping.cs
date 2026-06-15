using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping
{
    /// <summary>
    /// Maps a member reference to a value object.
    /// </summary>
    /// <typeparam name="TMemberReference"></typeparam>
    /// <typeparam name="TObject"></typeparam>
    public interface IMemberReferenceMapping<TMemberReference, TObject>
        where TMemberReference : MemberReference
    {
        /// <summary>
        /// Add or override for _Reference the _Value.
        /// </summary>
        /// <param name="_Reference"></param>
        /// <param name="_Value"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        bool Add(TMemberReference _Reference, TObject _Value, bool _Override = false);
    }
}
