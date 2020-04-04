using System.IO.Compression;
using System.Threading;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Factory for block processor
    /// </summary>
    internal class BlockProcessorFactory : IBlockProcessorFactory
    {
        public IBlockProcessor Create(CompressionMode compressionMode, IBlockDistributor blockDistributor, CancellationToken cancellationToken) => new BlockProcessor(compressionMode, blockDistributor, cancellationToken);
    }
}