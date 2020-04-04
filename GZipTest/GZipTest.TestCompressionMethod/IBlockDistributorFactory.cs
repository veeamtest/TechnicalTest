using System.IO;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Factory for block distributors
    /// </summary>
    internal interface IBlockDistributorFactory
    {
        IBlockDistributor Create(Stream stream);
    }
}