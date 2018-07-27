﻿using NUnit.Framework;
using cq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cq.Tests
{
    [TestFixture()]
    public class CsvReaderTests
    {
        // this is not a unittest. it is only for checking performance.
        [Test]
        public void CanReadFast()
        {
            var testData = new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"},
                new[] {"a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,\",a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,\","},
            };

            var temporaryFilePath = Path.GetTempFileName();

            using (var cw = new CsvWriter(new StreamWriter(temporaryFilePath)))
            {
                for (int i = 0; i < 100000; i++)
                {
                    foreach (var row in testData)
                    {
                        cw.WriteLine(row);
                    }
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = new CsvReader(new StreamReader(temporaryFilePath)).ReadAllLines().ToArray();

            stopwatch.Stop();

            Console.WriteLine($"{stopwatch.Elapsed.TotalMilliseconds} msec elapsed.");
        }

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
        public void ThrowsCsvFormatExceptionIfEofFoundWhileQuoted()
        {
            Assert.That(() =>
                {
                    const string testData = "1,abc,\"a,w,3\n";
                    var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                    throw new AssertionException(readAllLines.Last().Last());
                },
                Throws.TypeOf<CsvFormatException>()
                    .With.Property("Row").EqualTo(1)
                    .With.Property("Column").EqualTo(3)
                    .With.Property("Near").EqualTo("a,w,3\n")
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
                Throws.TypeOf<CsvFormatException>()
            );
        }

        [Test]
        public void ThrowsFormatExceptionIfWhitespaceAppearsAfterEndOfQuotation()
        {
            Assert.That(() =>
                {
                    const string testData = "1,2,3,4,5\r\n" + "1,abc,\"a,w,3\" \r\n";
                    var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                    throw new AssertionException(readAllLines.Last().Last());
                },
                Throws.TypeOf<CsvFormatException>()
                    .With.Property("Row").EqualTo(2)
                    .With.Property("Column").EqualTo(3)
                    .With.Property("Near").EqualTo("a,w,3")
            );
        }
    }
}