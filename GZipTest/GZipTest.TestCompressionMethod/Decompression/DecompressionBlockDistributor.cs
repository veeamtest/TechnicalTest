using System;
using System.IO;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod.Decompression
{
    /// <summary>
    ///     Implementation of block distributor for decompression
    /// </summary>
    internal class DecompressionBlockDistributor : IBlockDistributor, IDisposable
    {
        private readonly BinaryReader _binaryReader;
        private readonly Stream _stream;
        private int _currentBlockIndex;

        public DecompressionBlockDistributor(Stream stream)
        {
            Contract.IsNotNull(stream);
            Contract.Requires(stream.CanRead);
            this._stream = stream;
            this._binaryReader = new BinaryReader(this._stream);
            this._currentBlockIndex = 0;
        }

        public (int, Stream) GetNextBlock()
        {
            //reads size of block from stream, then reads the block from source stream, puts it to the new stream and returns one by one, including index
            lock (this)
            {
                if (this._binaryReader.BaseStream.Position == this._binaryReader.BaseStream.Length)
                {
                    //nothing else to read
                    return (-1, null);
                }

                long blockSize = this._binaryReader.ReadInt64();

                var buffer = new byte[blockSize];
                this._stream.Read(buffer, 0, (int) blockSize);
                var resultStream = new MemoryStream(buffer);
                return (this._currentBlockIndex++, resultStream);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._binaryReader?.Dispose();
            }
        }

        ~DecompressionBlockDistributor() => this.Dispose(false);
    }
}