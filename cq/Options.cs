using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using CommandLine;

namespace cq
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Options
    {
        [Option('c', "encode", Default = "Shift_JIS", HelpText = "Input/output encoding.")]
        public string EncodingName { set; get; }

        [Value(0, Required = true, HelpText = "Script for parsing a row from csv.")]
        public string Script { set; get; }


        [Option('h', "header", Default = false, HelpText = "Skip header")]
        public bool SkipHeader { set; get; }

        public Action<string[]> HeaderHandler
        {
            get { return SkipHeader ? (Action<string[]>) (_ => { }) : null; }
        }

        [Option('i', "input", Default = null, HelpText = "Read from file (default: console)")]
        public string InputFile { set; get; }

        private TextWriter _writer;
        /// <summary>
        /// output
        /// </summary>
        public TextWriter Writer
        {
            get { return _writer ?? new StreamWriter(Console.OpenStandardOutput(), GetEncoding()); }
            set { _writer = value; }
        }

        private Encoding GetEncoding()
        {
            return Encoding.GetEncoding(EncodingName);
        }

        private TextReader _reader;
        /// <summary>
        /// input
        /// </summary>
        public TextReader Reader
        {
            get
            {
                return _reader ??
                   (InputFile != null
                       ? new StreamReader(InputFile, GetEncoding())
                       : new StreamReader(Console.OpenStandardInput(), GetEncoding()));
            }
            set
            {
                _reader = value;
            }
        }
    }
}