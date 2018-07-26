using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandLine;

namespace cq
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Options
    {
        [Option('c', "encode", Default = "Shift_JIS", HelpText = "Input/output encoding.")]
        private string EncodeName { set; get; }

        public Encoding Encoding => Encoding.GetEncoding(EncodeName);

        [Value(0, Required = true, HelpText = "Script for parsing a row from csv.")]
        public string Script { set; get; }
    }
}