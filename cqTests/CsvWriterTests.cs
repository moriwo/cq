using System.IO;
using System.Linq;
using cq;
using NUnit.Framework;

namespace cqTests
{
    [TestFixture]
    public class CsvWriterTests
    {
        [TestCase("a", "a")]
        [TestCase("1,2", "\"1,2\"")]
        [TestCase("1\n2", "\"1\n2\"")]
        [TestCase(" a", "\" a\"")]
        [TestCase("a ", "\"a \"")]
        [TestCase("1\"2", "\"1\"\"2\"")]
        [TestCase(null, "")]
        public void EscapeStringEscapesStringWell(string input, string expected)
        {
            CsvWriter.EscapeString(input).Is(expected);
        }

        [Test]
        public void CanWriteToTextWriterAndReadByCsvReader()
        {
            var testData = new[]
            {
                new [] {"1", "abc", "1,2,3", "1\n\"a\" "},
                new [] {"@:;[.", ",,,,,,", "a\na\n\n\n\n", "\"\"\"","","","",",","EOL"},
                new [] {""},
                new [] {"this", "is", "a", "csv", "file"}
            };

            var temporaryFilePath = Path.GetTempFileName();
            
            using (var cw = new CsvWriter(new StreamWriter(temporaryFilePath)))
            {
                foreach (var row in testData)
                {
                    cw.WriteLine(row);
                }
            }
            
            // throw new Exception(temporaryFilePath);

            var result = new CsvReader(new StreamReader(temporaryFilePath)).ReadAllLines().ToArray();
            result.IsNotNull();
            result.Is(testData);
        }
    }
}