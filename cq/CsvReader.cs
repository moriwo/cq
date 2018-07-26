using System;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

namespace cq
{
    public class CsvReader : IDisposable
    {
        private readonly TextFieldParser _parser;

        public CsvReader(TextReader reader, Action<string[]> headerHandler = null)
        {
            _parser = new TextFieldParser(reader);

            _parser.SetDelimiters(",");
            _parser.TrimWhiteSpace = false;

            // read and handle header if headerHandler exists.
            headerHandler?.Invoke(ReadLine());
        }

        /// <summary>
        /// read a line as string[]
        /// </summary>
        /// <returns>a row in csv file</returns>
        private string[] ReadLine()
        {
            return _parser.ReadFields();
        }

        /// <summary>
        /// read all lines and enumerate
        /// </summary>
        /// <returns>enumerator</returns>
        public IEnumerable<string[]> ReadAllLines()
        {
            string[] row;

            while ((row = ReadLine()) != null)
                yield return row;
        }

        public void Dispose()
        {
            _parser.Close();
        }
    }
}