using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Core class for block processing
    ///     Every instance has its own thread and can ask for next block and process it
    ///     It is used both for compression and decompression
    ///     It communicates with outside world by events
    /// </summary>
    internal class BlockProcessor : IBlockProcessor
    {
        public enum BlockProcessorState
        {
            Unstarted,
            Started,
            Finished,
            Error
        }

        private readonly IBlockDistributor _blockDistributor;

        /// <summary>
        ///     Cancellation token is used for halt when something goes wrong
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        private readonly CompressionMode _compressionMode;

        public BlockProcessor(CompressionMode compressionMode, IBlockDistributor blockDistributor, CancellationToken cancellationToken)
        {
            Contract.IsNotNull(blockDistributor);
            Contract.IsNotNull(cancellationToken);
            this._compressionMode = compressionMode;
            this._blockDistributor = blockDistributor;
            this._cancellationToken = cancellationToken;
        }

        /// <summary>
        ///     Fires when error occures
        /// </summary>
        public event EventHandler<BlockProcessorExceptionEventArgs> ExceptionOccured;

        /// <summary>
        ///     Fires when there are not any other blocks for processing
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        ///     Fires when block has been processed and is ready
        /// </summary>
        public event EventHandler<BlockProcessorResultEventArgs> NewResult;

        /// <summary>
        ///     Starts processing
        /// </summary>
        public void Start()
        {
            this.State = BlockProcessorState.Started;
            new Thread(this.RunThread).Start();
        }

        public BlockProcessorState State { get; private set; } = BlockProcessorState.Unstarted;

        protected virtual void OnExceptionOccured(BlockProcessorExceptionEventArgs e) => this.ExceptionOccured?.Invoke(this, e);

        protected virtual void OnFinished() => this.Finished?.Invoke(this, EventArgs.Empty);

        protected virtual void OnNewResult(BlockProcessorResultEventArgs e) => this.NewResult?.Invoke(this, e);

        /// <summary>
        ///     Processes one block
        ///     Includes both compression and decompression logic
        ///     The logic might be refactored and injected to the block processor, but was not for simplicity
        /// </summary>
        private Stream ProcessBlock(Stream blockStream)
        {
            Contract.IsNotNull(blockStream);
            if (this._compressionMode == CompressionMode.Compress)
            {
                var resultStream = new MemoryStream();
                using var compressionStream = new GZipStream(resultStream, this._compressionMode, true);
                blockStream.CopyTo(compressionStream);
                return resultStream;
            }
            else
            {
                var resultStream = new MemoryStream();
                using var decompressionStream = new GZipStream(blockStream, this._compressionMode, true);
                decompressionStream.CopyTo(resultStream);
                return resultStream;
            }
        }

        /// <summary>
        ///     Thread entry method
        /// </summary>
        private void RunThread()
        {
            try
            {
                (int blockIndex, Stream blockStream) = this._blockDistributor.GetNextBlock(); //we get new block
                while (blockStream != null)
                {
                    //there is a block
                    if (this._cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    Stream processedStream = this.ProcessBlock(blockStream); //we process the block
                    if (this._cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    processedStream.Position = 0;
                    var blockResult = new BlockResult(processedStream, blockIndex);
                    this.OnNewResult(new BlockProcessorResultEventArgs(blockResult)); //push it out
                    (blockIndex, blockStream) = this._blockDistributor.GetNextBlock(); //we get new block
                }

                this.State = BlockProcessorState.Finished;

                this.OnFinished();
            }
            catch (Exception e)
            {
                this.State = BlockProcessorState.Error;
                if (this._cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                this.OnExceptionOccured(new BlockProcessorExceptionEventArgs(e));
            }
        }

        public class BlockProcessorExceptionEventArgs : EventArgs
        {
            public BlockProcessorExceptionEventArgs(Exception exception) => this.Exception = exception;

            public Exception Exception { get; }
        }

        public class BlockProcessorResultEventArgs : EventArgs
        {
            public BlockProcessorResultEventArgs(BlockResult blockResult) => this.BlockResult = blockResult;

            public BlockResult BlockResult { get; }
        }
    }
}