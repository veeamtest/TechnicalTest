using System;
using System.IO;
using System.IO.Compression;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Compression
{
    /// <summary>
    ///     Class implementing the compression algorithm
    /// </summary>
    internal class TestCompression : CompressionBase
    {
        public TestCompression(IBlockDistributorFactory blockDistributorFactory, IBlockProcessorFactory blockProcessorFactory) : base(blockDistributorFactory, blockProcessorFactory)
        {
            Contract.IsNotNull(blockDistributorFactory);
            Contract.IsInstanceOfType<IBlockSize>(blockDistributorFactory);
        }

        protected override CompressionMode CompressionMode => CompressionMode.Compress;

        private IBlockSize BlockSize => (IBlockSize) this.BlockDistributorFactory;

        protected override int GetBlockCount(Stream stream)
        {
            Contract.IsNotNull(stream);
            return (int) Math.Max(1, stream.Length / this.BlockSize.BlockSize);
        }

        /// <summary>
        ///     Writes empty header to the writer
        ///     First writes empty space for check string, then 0 (for block count)
        /// </summary>
        protected override void WriteEmptyHeaderOfStream(BinaryWriter binaryWriter)
        {
            Contract.IsNotNull(binaryWriter);
            binaryWriter.Write(new string(' ', ArchiveFileHeaderCheckString.Length));
            binaryWriter.Write(0);
        }

        /// <summary>
        ///     Writes the header to empty header
        ///     First writes the check string, then count of blocks created by compression
        /// </summary>
        protected override void WriteHeader(int blockCount, BinaryWriter binaryWriter)
        {
            Contract.Requires(blockCount > 0);
            Contract.IsNotNull(binaryWriter);
            binaryWriter.BaseStream.Position = 0;
            binaryWriter.Write(ArchiveFileHeaderCheckString);
            binaryWriter.Write(blockCount);
        }

        /// <summary>
        ///     Writes header of each block - length of the block
        /// </summary>
        protected override void WriteHeaderOfBlock(BinaryWriter binaryWriter, BlockResult blockResult)
        {
            Contract.IsNotNull(binaryWriter);
            Contract.IsNotNull(blockResult);
            binaryWriter.Write(blockResult.BlockStream.Length);
        }
    }
}