using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using GZipTest.Common;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Base class both for compression and decompression algorithms
    /// </summary>
    internal abstract class CompressionBase
    {
        /// <summary>
        ///     This string is used for check that the file is an archive. It is written to the beginning of the archive file
        /// </summary>
        protected const string ArchiveFileHeaderCheckString = "TestCompressionMethod";

        private readonly IBlockProcessorFactory _blockProcessorFactory;

        /// <summary>
        ///     We are using the cancellation token for halt when there is any error within any block processor
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        ///     AutoResetEvent for thread which processes the results provided by block processors
        /// </summary>
        private readonly AutoResetEvent _newBlockReadyOrBlockProcessorFinishedOrErrorAutoResetEvent = new AutoResetEvent(false);

        protected readonly IBlockDistributorFactory BlockDistributorFactory;

        /// <summary>
        ///     Stores results acquired from block processors
        /// </summary>
        private List<BlockResult> _blockResults;

        /// <summary>
        ///     Stores exception if there is any
        /// </summary>
        private Exception _exception;

        protected CompressionBase(IBlockDistributorFactory blockDistributorFactory, IBlockProcessorFactory blockProcessorFactory)
        {
            this.BlockDistributorFactory = blockDistributorFactory;
            this._blockProcessorFactory = blockProcessorFactory;
        }

        protected abstract CompressionMode CompressionMode { get; }

        /// <summary>
        ///     Main processing method
        ///     Checks source file, creates block distributor, writes header of the archive file (for compression), generates block
        ///     processors,
        ///     collects the results of block processing and writes the results to the target stream
        /// </summary>
        public void Process(Stream sourceStream, Stream destinationStream)
        {
            Contract.IsNotNull(sourceStream);
            Contract.IsNotNull(destinationStream);
            var blockIndex = 0;
            this._blockResults = new List<BlockResult>();
            var source = new CancellationTokenSource();
            if (this.CompressionMode == CompressionMode.Decompress)
            {
                this.CheckSourceFile(sourceStream); //we want to check the file is our archive
            }

            IBlockDistributor blockDistributor = this.BlockDistributorFactory.Create(sourceStream);
            using var destinationBinaryWriter = new BinaryWriter(destinationStream);
            if (this.CompressionMode == CompressionMode.Compress)
            {
                this.WriteEmptyHeaderOfStream(destinationBinaryWriter); //write empty space to the beginning, so we can use it at the end
            }

            //generate and run the block processors
            List<IBlockProcessor> blockProcessors = this.RunBlockProcessors(blockDistributor, this.GetBlockCount(sourceStream), this._cancellationTokenSource.Token);

            //process the block results
            blockIndex = this.ProcessBlockResults(destinationStream, blockProcessors, blockIndex, destinationBinaryWriter);

            (blockDistributor as IDisposable)?.Dispose();
            if (this._exception != null)
            {
                throw this._exception; //if there was an exception, we want to throw it
            }

            if (this.CompressionMode == CompressionMode.Compress)
            {
                this.WriteHeader(blockIndex, destinationBinaryWriter); //write the archive file header for compression
            }
        }

        /// <summary>
        ///     Returns count of blocks for the stream
        /// </summary>
        protected abstract int GetBlockCount(Stream stream);

        /// <summary>
        ///     Checks source file. Is used for check that file is archive file
        /// </summary>
        protected virtual void CheckSourceFile(Stream stream) { }

        /// <summary>
        ///     Writes empty space to the result file
        /// </summary>
        protected virtual void WriteEmptyHeaderOfStream(BinaryWriter binaryWriter) { }

        /// <summary>
        ///     Writes header to the header section
        /// </summary>
        protected virtual void WriteHeader(int blockCount, BinaryWriter binaryWriter) { }

        /// <summary>
        ///     Writes header of one block
        /// </summary>
        protected virtual void WriteHeaderOfBlock(BinaryWriter binaryWriter, BlockResult blockResult) { }

        private void BlockProcessor_ExceptionOccured(object sender, BlockProcessor.BlockProcessorExceptionEventArgs ea)
        {
            lock (this)
            {
                if (this._cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                this._cancellationTokenSource.Cancel(); //cancel the processing of all other threads
                this._exception = ea.Exception;
                this._newBlockReadyOrBlockProcessorFinishedOrErrorAutoResetEvent.Set(); //wake up the processing thread
            }
        }

        private void BlockProcessor_Finished(object sender, EventArgs e)
        {
            var blockProcessor = (BlockProcessor) sender;
            blockProcessor.NewResult -= this.BlockProcessor_NewResult;
            blockProcessor.Finished -= this.BlockProcessor_Finished;
            blockProcessor.ExceptionOccured -= this.BlockProcessor_ExceptionOccured;
            this._newBlockReadyOrBlockProcessorFinishedOrErrorAutoResetEvent.Set(); //wake up the processing thread
        }

        private void BlockProcessor_NewResult(object sender, BlockProcessor.BlockProcessorResultEventArgs e)
        { //new block result
            lock (this._blockResults)
            {
                this._blockResults.Add(e.BlockResult); //add it to the block results
            }

            this._newBlockReadyOrBlockProcessorFinishedOrErrorAutoResetEvent.Set(); //wake up the processing thread
        }

        /// <summary>
        /// Waits for the block results and writes them to the destination stream with correct order
        /// </summary>
        private int ProcessBlockResults(Stream destinationStream, List<IBlockProcessor> blockProcessors, int currentBlockIndex, BinaryWriter destinationBinaryWriter)
        {
            while ((blockProcessors.Any(p => p.State == BlockProcessor.BlockProcessorState.Started) || this._blockResults.Count > 0) && !this._cancellationTokenSource.IsCancellationRequested)
            { //there is at least one running block processor or at least one block result and cancellation was not requested
                if (this._blockResults.Count > 0)
                { //there is a block result
                    BlockResult blockResult;
                    lock (this._blockResults)
                    {
                        blockResult = this._blockResults.FirstOrDefault(r => r.BlockIndex == currentBlockIndex); //let's try to select block result with current index
                    }

                    while (blockResult != null)
                    { //we have block result with current index
                        lock (this._blockResults)
                        {
                            this._blockResults.Remove(blockResult);
                        }

                        if (this.CompressionMode == CompressionMode.Compress)
                        {
                            this.WriteHeaderOfBlock(destinationBinaryWriter, blockResult); //write block header for compression
                        }

                        blockResult.BlockStream.CopyTo(destinationStream); //copy block result to the destination stream
                        blockResult.Dispose();
                        currentBlockIndex++;
                        lock (this._blockResults)
                        {
                            blockResult = this._blockResults.FirstOrDefault(r => r.BlockIndex == currentBlockIndex); //let's try to select block result with current index
                        }
                    }
                }

                if (blockProcessors.Any(p => p.State == BlockProcessor.BlockProcessorState.Started))
                { //there is at least one started processor, we want to wait for next block result
                    this._newBlockReadyOrBlockProcessorFinishedOrErrorAutoResetEvent.WaitOne();
                }
            }

            return currentBlockIndex;
        }

        /// <summary>
        ///     Determines count of block processors, creates and returns them
        /// </summary>
        private List<IBlockProcessor> RunBlockProcessors(IBlockDistributor blockDistributor, decimal totalBlocks, CancellationToken cancellationToken)
        {
            Contract.IsNotNull(blockDistributor);
            Contract.Requires(totalBlocks > -1);
            Contract.IsNotNull(cancellationToken);
            var blockProcessors = new List<IBlockProcessor>();
            decimal blockProcessorCount = Math.Min(Environment.ProcessorCount, totalBlocks); //we want to employ as many threads as possible, up to the system logical cores count

            for (var i = 0; i < blockProcessorCount; i++)
            {
                IBlockProcessor blockProcessor = this._blockProcessorFactory.Create(this.CompressionMode, blockDistributor, cancellationToken);
                blockProcessor.NewResult += this.BlockProcessor_NewResult;
                blockProcessor.Finished += this.BlockProcessor_Finished;
                blockProcessor.ExceptionOccured += this.BlockProcessor_ExceptionOccured;
                blockProcessors.Add(blockProcessor);
                blockProcessor.Start();
            }

            return blockProcessors;
        }
    }
}