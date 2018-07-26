using System;
using System.IO;
using CommandLine;

namespace cq
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        // TODO: make help pretty
        // TODO: write CsvWriterTests
        // TODO: write CsvReaderTests
        // TODO: read from file
        // TODO: write to file
        
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(Cq.Run, _ => 1);
        }
    }
}