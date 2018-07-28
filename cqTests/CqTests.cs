using NUnit.Framework;
using cq;
using System.IO;
using System.Text;

namespace cqTests
{
    [TestFixture]
    public class CqTests
    {
        [Test]
        public void CanRun()
        {
            var stringBuilder = new StringBuilder();

            var options = new Options
            {
                Script = "r0",
                Reader = new StringReader("あ,1\r\nい,2\r\n"),
                Writer = new StringWriter(stringBuilder),
                SkipHeader = false
            };

            // returns zero if no error
            Cq.Run(options).Is(0);

            stringBuilder.ToString().Is("あ\r\nい\r\n");
        }

        [Test]
        public void CanSkipHeader()
        {
            var stringBuilder = new StringBuilder();

            var options = new Options
            {
                Script = "r1",
                Reader = new StringReader("Hiragana,Number\r\nあ,1\r\nい,2\r\n"),
                Writer = new StringWriter(stringBuilder),
                SkipHeader = true
            };

            // returns zero if no error
            Cq.Run(options).Is(0);

            stringBuilder.ToString().Is("1\r\n2\r\n");
        }
    }
}