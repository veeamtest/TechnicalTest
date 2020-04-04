# Project structure:
GZipTest.Console - console for user interaction

GZipTest.Common - few common classes that are used across projects or might be used by implementation of another compression method

GZipTest.TestCompressionMethod - concrete implementation of compression method

# Architecture:
Console uses an implementation of block compression method, GZipTest.TestCompressionMethod.TestCompressionMethod in our case.

Depending on the type of an action, TestCompressionMethod uses TestCompression or TestDecompression internally. Both TestCompression and TestDecompression are descendants of CompressionBase.

CompressionBase uses IBlockDistributor for preparation of blocks of data. There are two implementations of IBlockDistributor for each action : CompressionBlockDistributor and DecompressionBlockDistributor

CompressionBase uses IBlockProcessor for processing of one block of data. There is one implementation of IBlockProcessor - BlockProcessor

IBlockProcessor uses events for communication with CompressionBase. It uses BlockResult as a result of processing.

# Algorithm:
Compression: 
1. Stream is divided into blocks of data. 
2. Count of processing threads is determined based on count of cores and count of blocks. 
3. Empty archive file header is written
4. Each block is parallelly compressed and the results are written to the archive stream, with correct ordering. Each written block consists of the size and the data itself
5. At the end of the processing, when there are no more blocks waiting for processing or unwritten block results, the archive file header is written. It consists of "check string" and count of blocks

Decompression: 
1. The archive file header is read. The "check string" is checked to ensure we have an archive file. 
2. Count of blocks is read from the header
3. Count of processing threads is determined based on count of cores and count of blocks. 
4. Each block is parallelly decompressed and the results are written to the result stream, with correct ordering	

# Structure of compressed file: 

	[archive file header][block 1][block 2]...[block n]
		[archive file header] - structure : [check string][count of blocks]
			- [check string] : used for recognition of archive file
			- [count of blocks] : used for determination of count of block processors
		[block x] - structure : [block lenth][data]
			- [block length] : used by block distributor for determination of lenght of data
			- [data] : data of one block

# Comments:
Only important and nonobvious things were commented (only summary for important methods and some comments in the code) due to the time limitation, comments would be complete and everywhere in production code

Unit tests are not included due to the time limitation but code is ready for testing. Unit tests would be natural for production code

Crc check is not implemented, it would be used for production code though. It's absence may lead to corrupted result of a decompression
