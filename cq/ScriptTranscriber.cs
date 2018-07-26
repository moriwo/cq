using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace cq
{
    public static class ScriptTranscriber
    {
        private static readonly Regex SlicePattern = new Regex("r([0-9]+)-([0-9]+)");

        /// <summary>
        /// rewrite array slicing syntax (like 'r3-5') to a list of variables.
        /// ex. 'r3-5' -> 'r3, r4, r5'
        /// </summary>
        /// <param name="script">script to rewrite</param>
        /// <returns>rewritten script</returns>
        public static string TranscribeSlice(string script)
        {
            script = SlicePattern.Replace(script, m =>
            {
                var fromIndex = int.Parse(m.Groups[1].Value);
                var toIndex = int.Parse(m.Groups[2].Value);

                if(fromIndex > toIndex)
                {
                    throw new ArgumentOutOfRangeException($"illegal slicing syntax: '{m.Groups[0].Value}'");
                }
                // TODO: if from > to then throw ArgumentOutOfRangeException

                return string.Join(",",
                    Enumerable.Range(fromIndex, toIndex - fromIndex + 1).Select(i => $"r{i}"));
            });
            return script;
        }
    }
}