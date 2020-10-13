using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class MiscTools
    {
        public static List<T> DictToList<K, T>(Dictionary<K, T> dict)
        {
            List<T> list = new List<T>();
            foreach (var e in dict)
                list.Add(e.Value);
            return list;
        }
    }
}
