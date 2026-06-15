using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Utils
{
    [Serializable]
    public abstract class DbModel<T, U> where T: DbModel<T, U>, new()
    {
        [NonSerialized] protected static string fileName;
        [NonSerialized] protected static Dictionary<U, T> Meta;

        public U Id;
        
        public abstract void Load();
        protected void Init()
        {
            Meta = new DbParser<T, U>().GetDataAsDictionary(fileName);
            DbLoadChecker.I.Check();
        }
        public static T Get(U index) => Meta.ContainsKey(index) ? Meta[index] : null;
        public static int Count => Meta.Count;
        public static bool Have(U index) => Meta.ContainsKey(index);

        
        public static void ForEach(Action<T> toDo)
        {
            foreach (var meta in Meta)
            {
                toDo(meta.Value);
            }
        }

        public static void ForEach(Func<KeyValuePair<U, T>, bool> order, Action<T> toDo)
        {
            var sorted = Meta.OrderBy(order);
            foreach (var meta in sorted)
            {
                toDo(meta.Value);
            }
        }

        public static void ForEach(Func<KeyValuePair<U, T>, int> order, Action<T> toDo)
        {
            var sorted = Meta.OrderBy(order);
            foreach (var meta in sorted)
            {
                toDo(meta.Value);
            }
        }

        public static void ForEach(Predicate<T> predicate, Action<T> toDo)
        {
            foreach (var meta in Meta)
            {
                if (predicate(meta.Value)) toDo(meta.Value);
            }
        }
        public static List<T> GetAll(Predicate<T> predicate)
        {
            var match = new List<T>();
            foreach (var meta in Meta)
            {
                if (predicate(meta.Value)) match.Add(meta.Value);
            }

            return match;
        }
        public static T Get(Predicate<T> predicate)
        {
            foreach (var meta in Meta)
            {
                if (predicate(meta.Value)) return meta.Value;
            }

            return null;
        }
        public static List<T> ToList()
        {
            return Meta.Values.ToList();
        }
    }
}