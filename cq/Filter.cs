using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Native.Array;

namespace cq
{
    public class Filter
    {
        private readonly string _script;
        private readonly Engine _engine;

        public Filter(string script)
        {
            this._script = $"[{script}]";
            this._engine = new Engine();
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
