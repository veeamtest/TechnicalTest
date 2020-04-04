using System;
using System.IO;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Compression
{
    /// <summary>
    ///     Implementation of block distributor for compression
    /// </summary>
    internal class CompressionBlockDistributor : IBlockDistributor
    {
        private readonly long _blockSize;
        private readonly Stream _stream;
        private int _currentBlockIndex;

        public CompressionBlockDistributor(Stream stream, long blockSize)
        {
            Contract.IsNotNull(stream);
            Contract.Requires(stream.CanRead);
            Contract.Requires(blockSize > 0);
            this._stream = stream;
            this._blockSize = blockSize;
            this._currentBlockIndex = 0;
        }

        public (int, Stream) GetNextBlock()
        {
            //Creates fixed size blocks from source stream, puts the block to the stream and returns blocks one by one, including index
            lock (this)
            {
                long realBlockSize = Math.Min(this._stream.Length - this._stream.Position, this._blockSize);
                if (realBlockSize < 1)
                {
                    //nothing else to read
                    return (-1, null);
                }

                var buffer = new byte[realBlockSize];
                this._stream.Read(buffer, 0, (int) realBlockSize);
                var resultStream = new MemoryStream(buffer);
                return (this._currentBlockIndex++, resultStream);
            }
        }
    }
}