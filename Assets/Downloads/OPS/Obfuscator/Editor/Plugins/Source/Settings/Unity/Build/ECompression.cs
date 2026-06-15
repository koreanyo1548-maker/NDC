namespace OPS.Editor.Settings.Unity.Build
{
    /// <summary>
    /// The supported compression types of unity.
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Lzma compression.
        /// </summary>
        Lzma = 1,

        /// <summary>
        /// The Lz4 compression.
        /// </summary>
        Lz4 = 2,

        /// <summary>
        /// The Lz4HC compression.
        /// </summary>
        Lz4HC = 3,

        /// <summary>
        /// The Lzham compression.
        /// </summary>
        Lzham = 4,
    }
}
