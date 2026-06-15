// System
using System;

// OPS - IO
using OPS.Editor.IO.File;

namespace OPS.Obfuscator.Editor.Assets.Unity
{
    /// <summary>
    /// Represents a unity asset file reference.
    /// </summary>
    public class UnityAssetReference : IFileReference 
    {
        /// <summary>
        /// Returns the relative path including name and extension. For example Assets/[Directory]/[Asset][Extension].
        /// </summary>
        public String RelativeFilePath { get; private set; }

        /// <summary>
        /// Returns the RelativeFilePath.
        /// </summary>
        String IFileReference.FilePath
        {
            get
            {
                return this.RelativeFilePath;
            }
        }

        /// <summary>
        /// Name of the file, without extension.
        /// </summary>
        public String FileName { get; private set; }

        /// <summary>
        /// Extension only, contains the dot.
        /// </summary>
        public String FileExtension { get; private set; }

        /// <summary>
        /// Represents a unity asset in text form at _RelativeFilePath.
        /// </summary>
        /// <param name="_RelativeFilePath"></param>
        public UnityAssetReference(String _RelativeFilePath)
        {
            this.RelativeFilePath = _RelativeFilePath;

            this.FileName = System.IO.Path.GetFileNameWithoutExtension(_RelativeFilePath);
            this.FileExtension = System.IO.Path.GetExtension(_RelativeFilePath);
        }
    }
}
