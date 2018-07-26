using System;
using System.IO;

namespace cq
{
    // TODO: write tests for this class
    
    internal static class Cq
    {
        public static int Run(Options options)
        {
            var script = ScriptTranscriber.TranscribeSlice(options.Script);
            var filter = new Filter(script);

            using (var writer = new CsvWriter(new StreamWriter(Console.OpenStandardOutput())))
            {
                var sr = new StreamReader(Console.OpenStandardInput(), options.Encoding);
                using (var csvReader = new CsvReader(sr, header => { }))
                {
                    var lineNumber = 0;
                    
                    try
                    {
                        foreach (var row in csvReader.ReadAllLines())
                        {
                            lineNumber++;
                            writer.WriteLine(filter.Apply(row));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"error at line {lineNumber}: {e.Message}");
                        return 1;
                    }
                }
            }

            return 0;
        }
    }
}