using System.IO.Compression;
using System.Threading;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Factory for block processors
    /// </summary>
    internal interface IBlockProcessorFactory
    {
        IBlockProcessor Create(CompressionMode compressionMode, IBlockDistributor blockDistributor, CancellationToken cancellationToken);
    }
}