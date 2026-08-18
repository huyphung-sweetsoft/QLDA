using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Utils
{
    public static class DictionaryExtensions
    {
        public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> other)
        {
            foreach (var kv in other)
            {
                if (!dict.ContainsKey(kv.Key))
                    dict.Add(kv.Key, kv.Value);
            }
        }
    }
}
