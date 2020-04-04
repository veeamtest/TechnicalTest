using System.IO;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Compression
{
    /// <summary>
    ///     Factory for compression block distributor
    /// </summary>
    internal class CompressionBlockDistributorFactory : IBlockDistributorFactory, IBlockSize
    {
        public CompressionBlockDistributorFactory(long blockSize)
        {
            Contract.Requires(blockSize > 0);
            this.BlockSize = blockSize;
        }

        public IBlockDistributor Create(Stream stream)
        {
            Contract.IsNotNull(stream);
            return new CompressionBlockDistributor(stream, this.BlockSize);
        }

        public long BlockSize { get; }
    }
}