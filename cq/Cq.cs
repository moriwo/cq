using System;
using System.IO;

namespace cq
{
    public static class Cq
    {
        public static int Run(Options options)
        {
            var script = ScriptTranscriber.TranscribeSlice(options.Script);
            var filter = new Filter(script);

            using (var writer = new CsvWriter(options.Writer))
            {
                using (var csvReader = new CsvReader(options.Reader, options.HeaderHandler))
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
                        throw new Exception($"error at line {lineNumber}: {e.Message}", e);
                    }
                }
            }

            return 0;
        }
    }
}