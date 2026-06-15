using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping
{
    public class MemberReferenceMapping<TMemberReference, TMemberKey> : IMemberReferenceMapping<TMemberReference, TMemberKey>
        where TMemberReference : MemberReference
        where TMemberKey : IMemberKey
    {
        /// <summary>
        /// MemberReference, (Original) Member Key
        /// </summary>
        public List<KeyValuePair<TMemberReference, TMemberKey>> ReferenceList { get; } = new List<KeyValuePair<TMemberReference, TMemberKey>>();

        /// <summary>
        /// Add a _Reference, and its resolved definitions original member key.
        /// </summary>
        /// <param name="_Reference"></param>
        /// <param name="_OriginalMemberKey"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        public bool Add(TMemberReference _Reference, TMemberKey _OriginalMemberKey, bool _Override = false)
        {
            if(_Override)
            {
                throw new NotImplementedException();
            }

            this.ReferenceList.Add(new KeyValuePair<TMemberReference, TMemberKey>(_Reference, _OriginalMemberKey));

            return true;
        }
    }
}
