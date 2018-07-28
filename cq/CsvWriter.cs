using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cq
{
    public class CsvWriter : IDisposable
    {
        private readonly TextWriter _writer;

        public CsvWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            _writer.Flush();
            _writer.Close();
        }

        public void WriteLine(IEnumerable<string> data)
        {
            _writer.Write(string.Join(",", data.Select(EscapeString)) + "\r\n");
        }

        /// <summary>
        /// Escapes string for csv with following three rules:
        /// 1. quote with " if str contains one or more comma(s) or \n.
        /// 2. quote with " if str starts or ends with a whitespace.
        /// 3. replace " with "" and quote with " if string contains on or more doublequote(s).
        /// </summary>
        /// <param name="str"></param>
        /// <returns>escaped string</returns>
        public static string EscapeString(string str)
        {
            if (str == null)
                return "";

            var containsDoubleQuote = str.Contains("\"");
            var startsWithWhiteSpace = str.StartsWith(" ");
            var endsWithWhiteSpace = str.EndsWith(" ");
            var containsComma = str.Contains(",");
            var containsLineFeed = str.Contains("\n");
            
            var mustBeQuoted = startsWithWhiteSpace || endsWithWhiteSpace || containsComma || containsLineFeed || containsDoubleQuote;

            if (containsDoubleQuote)
                str = str.Replace("\"", "\"\"");

            return mustBeQuoted ? $"\"{str}\"" : str;
        }
    }
}