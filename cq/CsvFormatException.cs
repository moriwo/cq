using System;

namespace cq
{
    public class CsvFormatException : Exception
    {
        private const string MessageFormat =
            "illegal format found at row #{0} column #{1}, near '{2}'";

        public int Row { get; }
        public int Column { get; }
        public string Near { get; }

        public CsvFormatException(int row, int column, string near)
            : base(string.Format(MessageFormat, row, column, near))
        {
            Row = row;
            Column = column;
            Near = near;
        }
    }
}