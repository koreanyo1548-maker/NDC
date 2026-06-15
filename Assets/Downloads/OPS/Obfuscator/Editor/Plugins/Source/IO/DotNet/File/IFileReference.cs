// System
using System;

namespace OPS.Editor.IO.File
{
    /// <summary>
    /// Reference a file in the filesystem.
    /// </summary>
    public interface IFileReference
    {
        // Path
        #region Path

        /// <summary>
        /// Path to the file.
        /// </summary>
        String FilePath { get; }

        #endregion
    }
}
