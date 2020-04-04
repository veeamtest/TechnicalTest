using System;

namespace GZipTest.TestCompressionMethod
{
    /// <summary>
    ///     Interface for block processors
    ///     Block processor's responsibility is to process blocks of data, one by one
    ///     It wraps processing by one thread
    /// </summary>
    internal interface IBlockProcessor
    {
        BlockProcessor.BlockProcessorState State { get; }
        event EventHandler<BlockProcessor.BlockProcessorExceptionEventArgs> ExceptionOccured;
        event EventHandler Finished;
        event EventHandler<BlockProcessor.BlockProcessorResultEventArgs> NewResult;
        void Start();
    }
}