using System.IO;
using GZipTest.Common;
using GZipTest.TestCompressionMethod.Compression;
using GZipTest.TestCompressionMethod.Decompression;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Wrapping class used as an entry point to the compression and decompression within this library
    /// </summary>
    public class TestCompressionMethod : IBlockCompressionMethod
    {
        public void Compress(Stream sourceStream, Stream destinationStream, long blockSize) => new TestCompression(new CompressionBlockDistributorFactory(blockSize), new BlockProcessorFactory()).Process(sourceStream, destinationStream);

        public void Decompress(Stream sourceStream, Stream destinationStream) => new TestDecompression(new DecompressionBlockDistributorFactory(), new BlockProcessorFactory()).Process(sourceStream, destinationStream);
    }
}