using System.IO;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Decompression
{
    /// <summary>
    ///     Factory for decompression block distributor
    /// </summary>
    internal class DecompressionBlockDistributorFactory : IBlockDistributorFactory
    {
        public IBlockDistributor Create(Stream stream)
        {
            Contract.IsNotNull(stream);
            return new DecompressionBlockDistributor(stream);
        }
    }
}