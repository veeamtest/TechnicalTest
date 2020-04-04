using System;
using System.IO;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Result of one block processing
    /// </summary>
    internal class BlockResult : IDisposable
    {
        public BlockResult(Stream blockStream, int blockIndex)
        {
            Contract.IsNotNull(blockStream);
            Contract.Requires(blockIndex >= 0);
            this.BlockStream = blockStream;
            this.BlockIndex = blockIndex;
        }

        /// <summary>
        ///     Index of block
        /// </summary>
        public int BlockIndex { get; }

        /// <summary>
        ///     Result stream of processing
        /// </summary>
        public Stream BlockStream { get; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.BlockStream?.Dispose();
            }
        }

        ~BlockResult() => this.Dispose(false);
    }
}