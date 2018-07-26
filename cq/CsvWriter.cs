using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            _writer.WriteLine(string.Join(",", data.Select(EscapeString)));
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
            var mustBeQuoted = false;

            if (str == null)
                return "";

            mustBeQuoted = str.StartsWith(" ") || str.EndsWith(" ") || str.Contains(",") || str.Contains("\n");

            if (str.Contains("\""))
            {
                mustBeQuoted = true;
                str = str.Replace("\"", "\"\"");
            }

            return mustBeQuoted ? $"\"{str}\"" : str;
        }
    }
}