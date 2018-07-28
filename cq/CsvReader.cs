using System;
using System.IO;
using System.Collections.Generic;

namespace cq
{
    public class CsvReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly IEnumerator<string[]> _enumerator;
        
        public CsvReader(TextReader reader, Action<string[]> headerHandler = null)
        {
            _reader = reader;
            _enumerator = CsvParser.Parse(_reader.Read).GetEnumerator();
            headerHandler?.Invoke(ReadLine());
        }

        /// <summary>
        /// read a line as string[]
        /// </summary>
        /// <returns>a row in csv file, null if eof</returns>
        private string[] ReadLine()
        {
            return _enumerator.MoveNext() ? _enumerator.Current : null;
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
            _reader.Dispose();
        }
    }
}