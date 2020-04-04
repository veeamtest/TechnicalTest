using System;
using System.IO;
using CommandLine;
using GZipTest.Common;

namespace GZipTest.Console
{
    internal class Program
    {
        private static IBlockCompressionMethod TestCompressionMethod => new TestCompressionMethod.TestCompressionMethod();

        private static void Main(string[] args)
        {
            var parser = new Parser(cfg =>
                                    {
                                        cfg.CaseInsensitiveEnumValues = true;
                                        cfg.HelpWriter = Parser.Default.Settings.HelpWriter;
                                    });
            parser.ParseArguments<Options>(args).WithParsed(ProcessWithOptions);
        }

        private static void ProcessWithOptions(Options options)
        {
            try
            {
                RunApplication(options);
                Environment.ExitCode = 0;
            }
            catch (UserException e)
            {
                System.Console.WriteLine($"Error has occured : {e.Message}");
                Environment.ExitCode = 1;
            }
            catch (Exception e)
            {
                if (options.Debug)
                {
                    System.Console.WriteLine("Unexpected error has occured");
                    System.Console.WriteLine(e.ToString());
                }
                else
                {
                    if (options.Action == Options.OptionsAction.compress)
                    {
                        System.Console.WriteLine($"Unexpected error occured. {e.Message}");
                    }
                    else
                    {
                        System.Console.WriteLine($"Unexpected error occured. Archive file may be corrupted. {e.Message}");
                    }
                }

                Environment.ExitCode = 1;
            }
        }

        private static void RunApplication(Options options)
        {
            Contract.IsNotNull(options);
            var wasError = false;
            if (!File.Exists(options.SourceFileName))
            {
                throw new UserException("Original file doesn't exist");
            }

            if (File.Exists(options.DestinationFileName))
            {
                throw new UserException("Archive file already exists");
            }

            Stream originalFileStream = null, archiveFileStream = null;
            try
            {
                try
                {
                    originalFileStream = File.OpenRead(options.SourceFileName);
                }
                catch (Exception e)
                {
                    throw new UserException($"Original file can't be opened. {e.Message}");
                }

                try
                {
                    archiveFileStream = File.Create(options.DestinationFileName);
                }
                catch (Exception e)
                {
                    throw new UserException($"Archive file can't be created. {e.Message}");
                }

                switch (options.Action)
                {
                    case Options.OptionsAction.compress:
                        TestCompressionMethod.Compress(originalFileStream, archiveFileStream, 1000000);
                        System.Console.WriteLine($"Archive file created : {options.DestinationFileName}");
                        break;
                    case Options.OptionsAction.decompress:
                        TestCompressionMethod.Decompress(originalFileStream, archiveFileStream);
                        System.Console.WriteLine($"File decompressed : {options.DestinationFileName}");
                        break;
                    case Options.OptionsAction.easteregg:
                        throw new UserException("Easter egg :)");
                }
            }
            catch
            {
                wasError = true;
                throw;
            }
            finally
            {
                originalFileStream?.Close();
                originalFileStream?.Dispose();
                archiveFileStream?.Close();
                archiveFileStream?.Dispose();
                if (wasError && File.Exists(options.DestinationFileName))
                {
                    File.Delete(options.DestinationFileName);
                }
            }
        }
    }
}