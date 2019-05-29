using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace xnano.Extensions
{
    public static class DictionaryExtentions
    {
        public static NameValueCollection ToNameValueCollection(this Dictionary<string,string> dict)
        {
            return dict.Aggregate(new NameValueCollection(),
                (seed, current) => {
                    seed.Add(current.Key, current.Value);
                return seed;
            });
        }
    }
}
