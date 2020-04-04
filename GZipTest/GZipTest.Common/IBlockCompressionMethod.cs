using System.IO;

namespace GZipTest.Common
{
    /// <summary>
    ///     Interface for compression method using blocks. Other methods might be used, only one is included in our project
    /// </summary>
    public interface IBlockCompressionMethod
    {
        /// <summary>
        ///     Compresses source stream to destination stream, with defined block size
        /// </summary>
        void Compress(Stream sourceStream, Stream destinationStream, long blockSize);

        /// <summary>
        ///     Decompresses source stream to destination stream
        /// </summary>
        void Decompress(Stream sourceStream, Stream destinationStream);
    }
}