using NUnit.Framework;
using cq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cq.Tests
{
    [TestFixture()]
    public class CsvReaderTests
    {
        [Test]
        public void CanReadSimpleOneLineCsv()
        {
            const string testData = "1,a,@";
            var expected = new[]
            {
                new[] {"1", "a", "@"}
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCell()
        {
            const string testData = "1,\"a,b\",@";
            var expected = new[]
            {
                new[] {"1", "a,b", "@"}
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCellContaingQuoteMark()
        {
            const string testData = "1,\"a,\"\"b\",@";
            var expected = new[]
            {
                new[] {"1", "a,\"b", "@"}
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCellContainingNewLine()
        {
            const string testData = "1,\"a,\n\n\r\nb\",@";
            var expected = new[]
            {
                new[] {"1", "a,\n\n\r\nb", "@"}
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleTwoLineCsvWithEmptyCell()
        {
            const string testData = "1,a,@\r\n2,b,";
            var expected = new[]
            {
                new[] {"1", "a", "@"},
                new[] {"2", "b", ""}
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithContinuousEmptyCells()
        {
            const string testData = ",,1,a,,,@,,";
            var expected = new[]
            {
                new[] {"", "", "1", "a", "", "", "@", "", ""},
            };
            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadWellFormattedFileWithoutHeader()
        {
            const string testData = "1,abc,\"1,2,3\",\"1\n" +
                                    "\"\"a\"\" \"\r\n" +
                                    "@:;[.,\",,,,,,\",\"a\n" +
                                    "a\n" +
                                    "\n" +
                                    "\r\n" +
                                    "\n" +
                                    "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                    "\r\n" +
                                    "this,is,a,csv,file\r\n";
            var expected = new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            };

            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadWellFormattedFileWithHeader()
        {
            const string testData = "c1,c2,c3,c4\r\n" +
                                    "1,abc,\"1,2,3\",\"1\n" +
                                    "\"\"a\"\" \"\r\n" +
                                    "@:;[.,\",,,,,,\",\"a\n" +
                                    "a\n" +
                                    "\n" +
                                    "\r\n" +
                                    "\n" +
                                    "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                    "\r\n" +
                                    "this,is,a,csv,file\r\n";
            var expected = new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            };
            var expectedHeader = new[] {"c1", "c2", "c3", "c4"};

            using (var reader = new CsvReader(new StringReader(testData), header => header.Is(expectedHeader)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void LastRowMayNotHaveTrailingCrlf()
        {
            const string testData = "1,abc,\"1,2,3\",\"1\n" +
                                    "\"\"a\"\" \"\r\n" +
                                    "@:;[.,\",,,,,,\",\"a\n" +
                                    "a\n" +
                                    "\n" +
                                    "\r\n" +
                                    "\n" +
                                    "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                    "\r\n" +
                                    "this,is,a,csv,file";
            var expected = new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            };

            using (var reader = new CsvReader(new StringReader(testData)))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void ThrowsFormatExceptionIfEofFoundWhileQuoted()
        {
            Assert.That(() =>
                {
                    const string testData = "1,abc,\"a,w,3\n";
                    var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                    throw new AssertionException(readAllLines.Last().Last());
                },
                Throws.TypeOf<FormatException>()
            );
        }

        [Test]
        public void ThrowsFormatExceptionIfQuoteAppearsInTheMiddleOfACell()
        {
            Assert.That(() =>
                {
                    const string testData = "1,abc,a\"a,w,3\"\r\n";
                    var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                    throw new AssertionException(readAllLines.Last().Last());
                },
                Throws.TypeOf<FormatException>()
            );
        }

        [Test]
        public void ThrowsFormatExceptionIfWhitespaceAppearsAfterEndOfQuotation()
        {
            Assert.That(() =>
                {
                    const string testData = "1,abc,\"a,w,3\" \r\n";
                    var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                    throw new AssertionException(readAllLines.Last().Last());
                },
                Throws.TypeOf<FormatException>()
            );
        }
    }
}