using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.IO.Version
{
    /// <summary>
    /// Make a class version able and allow to load from a different version.
    /// </summary>
    public interface IVersionAble
    {
        /// <summary>
        /// The version of this object.
        /// </summary>
        String Version { get; }

        /// <summary>
        /// Load current object from a different old version. 
        /// </summary>
        /// <typeparam name="TVersionAble"></typeparam>
        /// <param name="_VersionAble"></param>
        /// <returns></returns>
        bool LoadFromVersion<TVersionAble>(TVersionAble _VersionAble)
            where TVersionAble : IVersionAble;
    }
}
