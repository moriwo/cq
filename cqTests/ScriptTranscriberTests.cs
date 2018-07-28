using System;
using cq;
using NUnit.Framework;

namespace cqTests
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