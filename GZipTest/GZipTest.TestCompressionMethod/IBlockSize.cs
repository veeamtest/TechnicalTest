namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Class able to provide the block size implements this interface
    /// </summary>
    internal interface IBlockSize
    {
        long BlockSize { get; }
    }
}