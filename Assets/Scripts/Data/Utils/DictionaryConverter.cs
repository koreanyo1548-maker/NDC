using System.Collections.Generic;

namespace Data.Utils
{
    public static class DictionaryConverter
    {
        public static Dictionary<T, DbField<U>> NormalToField<T, U>(Dictionary<T, U> dic, int id, DbUserModel parent)
        {
            Dictionary<T, DbField<U>> ret = new();
            foreach (var d in dic)
            {
                ret.Add(d.Key, new DbField<U>(d.Value, id, parent));
            }

            return ret;
        }
        
        public static Dictionary<T, K> FieldToNormal<T, U, K>(Dictionary<T, U> dic) where U : DbField<K>
        {
            Dictionary<T, K> ret = new();
            foreach (var d in dic)
            {
                ret.Add(d.Key, d.Value.Value);
            }

            return ret;
        }
    }
}