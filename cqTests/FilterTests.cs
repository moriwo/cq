using cq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace cq.Tests
{
    public class FilterTests
    {
        /// <summary>
        /// common test data for Filter.Apply
        /// </summary>
        private readonly string[] _commonInput = new[] { "cell0", "cell1", "cell2", "cell3", "43", "cell5" };

        [Test]
        public void FilterCanConvertEveryStringInArray()
        {
            new Filter("r0, r2, r3+r5, +r4+10, r1.length").Apply(_commonInput)
                .Is(new[] { "cell0", "cell2", "cell3cell5", "53", "5" });
        }

        [Test]
        public void FilterThrowsExceptionIfUndefinedValueFound()
        {
            try
            {
                new Filter("r0,r1,r2,r3.lengt").Apply(_commonInput);
                Assert.Fail();
            }
            catch (UndefinedValueErrorException e)
            {
                e.Index.Is("3");
            }
        }
        
        [Test]
        public void FilterThrowsUndefinedValueExceptionForFirstUndefinedValueFound()
        {
            try
            {
                new Filter("r0,r1.lenrth,r2,r3.lengt").Apply(_commonInput);
                Assert.Fail();
            }
            catch (UndefinedValueErrorException e)
            {
                e.Index.Is("1");
            }
        }
        
        [Test]
        public void FilterThrowsExceptionIfScriptHasSyntaxError()
        {
            Assert.That(() =>
            {
                new Filter("r0,+-,r1").Apply(_commonInput);
            }, Throws.TypeOf<Jint.Parser.ParserException>());
        }

        [Test]
        public void FilterThrowsExceptionIfScriptUsesUndefinedVariable()
        {
            Assert.That(() =>
            {
                new Filter("r0,++c,r1").Apply(_commonInput);
            }, Throws.TypeOf<Jint.Runtime.JavaScriptException>());
        }
    }
}