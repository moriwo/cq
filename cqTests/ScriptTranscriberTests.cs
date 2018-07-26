using cq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace cq.Tests
{
    public class ScriptTranscriberTests
    {
        [Test]
        public void TranscribeSliceCanRewriteProperly()
        {
            ScriptTranscriber.TranscribeSlice("r8-10").Is("r8,r9,r10");
        }

        [Test]
        public void TranscribeSliceThrowsExceptionIfFromIndexGreaterThanToIndex()
        {
            Assert.That(() =>
            {
                ScriptTranscriber.TranscribeSlice("r8-4");
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}