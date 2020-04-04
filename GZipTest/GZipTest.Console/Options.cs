using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace GZipTest.Console
{
    internal class Options
    {
        public enum OptionsAction
        {
            easteregg,
            compress,
            decompress
        }

        [Usage(ApplicationAlias = "gziptest")]
        public static IEnumerable<Example> Examples =>
            new List<Example>
            {
                new Example("Example of compression", UnParserSettings.WithUseEqualTokenOnly(), new Options {Action = OptionsAction.compress, SourceFileName = "OriginalFile", DestinationFileName = "ArchiveFile"}),
                new Example("Example of decompression", UnParserSettings.WithUseEqualTokenOnly(), new Options {Action = OptionsAction.decompress, SourceFileName = "ArchiveFile", DestinationFileName = "DecompressedFile"})
            };

        [Value(0, MetaName = "Action", Required = true, HelpText = "Action - compress or decompress")]
        public OptionsAction Action { get; set; }

        [Value(2, MetaName = "DestinationFileName", Required = true, HelpText = "Archive file name")]
        public string DestinationFileName { get; set; }

        [Option('d', "Debug", Required = false, HelpText = "Output debug information")]
        public bool Debug { get; set; }

        [Value(1, MetaName = "SourceFileName", Required = true, HelpText = "Original file name")]
        public string SourceFileName { get; set; }
    }
}