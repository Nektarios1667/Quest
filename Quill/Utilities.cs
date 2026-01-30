using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Quill;
public static class Utilities
{
    public static string DictToQict<K, V>(Dictionary<K, V> dict)
        where K : notnull
        where V : notnull
    => string.Join('/', dict.Select(kv => $"{kv.Key}:{kv.Value}"));

    public static Dictionary<string, string> QictToDict(string qict)
    {
        Dictionary<string, string> dict = [];
        foreach (string pair in qict.Split('/'))
        {
            // Get key-value
            string[] kv = pair.Split(':', 2);
            if (kv.Length != 2)
                continue;

            dict[kv[0]] = kv[1];
        }

        return dict;
    }
}
