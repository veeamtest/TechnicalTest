using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Decompression
{
    /// <summary>
    ///     Class implementing the decompression algorithm
    /// </summary>
    internal class TestDecompression : CompressionBase
    {
        public TestDecompression(IBlockDistributorFactory blockDistributorFactory, IBlockProcessorFactory blockProcessorFactory) : base(blockDistributorFactory, blockProcessorFactory) { }
        protected override CompressionMode CompressionMode => CompressionMode.Decompress;

        /// <summary>
        ///     Reads block count from the stream
        /// </summary>
        protected override int GetBlockCount(Stream stream)
        {
            Contract.IsNotNull(stream);
            using var binaryReader = new BinaryReader(stream, Encoding.UTF8, true);
            return binaryReader.ReadInt32();
        }

        /// <summary>
        ///     Checks the source file, if it's compressed file
        /// </summary>
        protected override void CheckSourceFile(Stream stream)
        {
            Contract.IsNotNull(stream);
            using var binaryReader = new BinaryReader(stream, Encoding.UTF8, true);
            try
            {
                //let's check the string at the beginning...
                string archiveFileHeaderCheckString = binaryReader.ReadString();
                if (archiveFileHeaderCheckString != ArchiveFileHeaderCheckString)
                {
                    throw new UserException("Source file is not archive or is corrupted");
                }
            }
            catch (Exception)
            {
                throw new UserException("Source file is not archive or is corrupted");
            }
        }
    }
}