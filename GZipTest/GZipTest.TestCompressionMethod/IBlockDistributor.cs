using System.IO;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Interface for block distributor implementations
    ///     Block distributor's responsibility is to provide blocks from source stream
    /// </summary>
    internal interface IBlockDistributor
    {
        /// <summary>
        ///     Returns next block
        /// </summary>
        (int, Stream) GetNextBlock();
    }
}