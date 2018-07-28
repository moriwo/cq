using System.Linq;
using Jint;

namespace cq
{
    public class Filter
    {
        private readonly string _script;
        private readonly Engine _engine;

        public Filter(string script)
        {
            _script = $"[{script}]";
            _engine = new Engine();
        }

        /// <summary>
        /// apply script to every string in row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string[] Apply(string[] row)
        {
            // set variables
            _engine.SetValue("r", row);
            for (var i = 0; i < row.Length; i++)
                _engine.SetValue($"r{i}", row[i]);
            
            var values = _engine.Execute(_script).GetCompletionValue();

            foreach (var elem in values.AsArray().GetOwnProperties())
            {
                if (elem.Value.Value.IsUndefined())
                    throw new UndefinedValueErrorException(elem.Key);
            }

            var objects = (object[]) values.ToObject();
            return objects.Select(obj => (obj ?? "null").ToString()).ToArray();
        }
    }
}
