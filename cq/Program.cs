using System;
using CommandLine;

namespace cq
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        // TODO: make help pretty
        // TODO: read from file
        // TODO: write to file
        
        private static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<Options>(args)
                    .MapResult(Cq.Run, _ => 1);

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}