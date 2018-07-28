using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using cq;
using NUnit.Framework;

namespace cqTests
{
    [TestFixture]
    public class CsvReaderTests
    {
        // this is not a unittest. it is only for checking performancess.
        [Test]
        [Ignore("run this when you want to measure performances")]
        public void CanReadFast()
        {
            var testData = new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"},
                new[] {"a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,\",a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,a,\","}
            };

            var temporaryFilePath = Path.GetTempFileName();

            using (var cw = new CsvWriter(new StreamWriter(temporaryFilePath)))
            {
                for (var i = 0; i < 200000; i++)
                {
                    foreach (var row in testData)
                    {
                        cw.WriteLine(row);
                    }
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            foreach (var _ in new CsvReader(new StreamReader(temporaryFilePath)).ReadAllLines())
            {
                
            }

            stopwatch.Stop();

            Console.WriteLine($"{stopwatch.Elapsed.TotalMilliseconds} msec elapsed.");
        }

        private static void AssertCanReadCorrectly(string testData, string[][] expected,
            Action<string[]> headerHandler = null)
        {
            using (var reader = new CsvReader(new StringReader(testData), headerHandler))
            {
                var actual = reader.ReadAllLines();
                actual.Is(expected);
            }
        }

        [Test]
        public void CanReadSimpleOneLineCsv()
        {
            AssertCanReadCorrectly("1,a,@", new[] {new[] {"1", "a", "@"}});
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCell()
        {
            AssertCanReadCorrectly("1,\"a,b\",@", new[] {new[] {"1", "a,b", "@"}});
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCellContaingQuoteMark()
        {
            AssertCanReadCorrectly("1,\"a,\"\"b\",@", new[] {new[] {"1", "a,\"b", "@"}});
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithQuotedCellContainingNewLine()
        {
            AssertCanReadCorrectly("1,\"a,\n\n\r\nb\",@", new[] {new[] {"1", "a,\n\n\r\nb", "@"}});
        }

        [Test]
        public void CanReadSimpleTwoLineCsvWithEmptyCell()
        {
            AssertCanReadCorrectly("1,a,@\r\n2,b,", new[] {new[] {"1", "a", "@"}, new[] {"2", "b", ""}});
        }

        [Test]
        public void CanReadSimpleOneLineCsvWithContinuousEmptyCells()
        {
            AssertCanReadCorrectly(",,1,a,,,@,,", new[] {new[] {"", "", "1", "a", "", "", "@", "", ""}});
        }

        [Test]
        public void CanReadWellFormattedFileWithoutHeader()
        {
            AssertCanReadCorrectly("1,abc,\"1,2,3\",\"1\n" +
                                   "\"\"a\"\" \"\r\n" +
                                   "@:;[.,\",,,,,,\",\"a\n" +
                                   "a\n" +
                                   "\n" +
                                   "\r\n" +
                                   "\n" +
                                   "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                   "\r\n" +
                                   "this,is,a,csv,file\r\n", new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            });
        }

        [Test]
        public void CanReadWellFormattedFileWithHeader()
        {
            AssertCanReadCorrectly("c1,c2,c3,c4\r\n" +
                                   "1,abc,\"1,2,3\",\"1\n" +
                                   "\"\"a\"\" \"\r\n" +
                                   "@:;[.,\",,,,,,\",\"a\n" +
                                   "a\n" +
                                   "\n" +
                                   "\r\n" +
                                   "\n" +
                                   "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                   "\r\n" +
                                   "this,is,a,csv,file\r\n", new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            }, header => header.Is(new[] {"c1", "c2", "c3", "c4"}));
        }

        [Test]
        public void LastRowMayNotHaveTrailingCrlf()
        {
            AssertCanReadCorrectly("1,abc,\"1,2,3\",\"1\n" +
                                   "\"\"a\"\" \"\r\n" +
                                   "@:;[.,\",,,,,,\",\"a\n" +
                                   "a\n" +
                                   "\n" +
                                   "\r\n" +
                                   "\n" +
                                   "\",\"\"\"\"\"\"\"\",,,,\",\",EOL\r\n" +
                                   "\r\n" +
                                   "this,is,a,csv,file", new[]
            {
                new[] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new[] {"@:;[.", ",,,,,,", "a\na\n\n\r\n\n", "\"\"\"", "", "", "", ",", "EOL"},
                new[] {""},
                new[] {"this", "is", "a", "csv", "file"}
            });
        }

        [Test]
        public void ThrowsCsvFormatExceptionIfEofFoundWhileQuoted()
        {
            // I do not use Assert.That - Throws here.
            // This is to prevent code inspection alerts (can be private) for Row, Column, Near of CsvFormatException.
            try
            {
                const string testData = "1,abc,\"a,w,3\n";
                var readAllLines = new CsvReader(new StringReader(testData)).ReadAllLines();
                throw new AssertionException(readAllLines.Last().Last());
            }
            catch (CsvFormatException e)
            {
                e.Row.Is(1);
                e.Column.Is(3);
                e.Near.Is("a,w,3\n");
            }
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