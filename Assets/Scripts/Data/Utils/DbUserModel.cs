using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Utils
{
    public abstract class DbUserModel
    {
        public event EventHandler<DbEventArgs> AnyValueChanged;

        public void OnAnyValueChanged(DbEventArgs id)
        {
            var raiseEvent = AnyValueChanged;

            if (raiseEvent != null)
            {
                raiseEvent(this, id);
            }
        }
    }
    [Serializable]
    public abstract class DbUserModel<T, U>: DbUserModel where T: DbUserModel<T, U>, new() 
    {
        [NonSerialized] private static Dictionary<U, T> Data;
        public static int EntityCount => Data.Count;

        public static object Saves => Data.Values.ToList();

        [DataMember] public U Id { get; protected set; }

        public abstract void Set(List<T> obj);

        public static void Remove(U remove)
        {
            Data.Remove(remove);
        }
        
        protected static void Init([CanBeNull] List<T> obj)
        {
            if (obj == null) obj = new T().GetInitials();
            else obj = new T().AdjustDataModification(obj);
            
            Data = new DbUserParser<T, U>().GetDataAsDictionary(obj);
            DbLoadChecker.I.Check();
        }

        public static T Get(U key) => Data.ContainsKey(key) ? Data[key] : null;

        public static bool HaveAny(Predicate<T> predicate)
        {
            foreach (var meta in Data)
            {
                if (predicate(meta.Value)) return true;
            }

            return false;
        }

        protected abstract List<T> GetInitials();
        public abstract List<T> AdjustDataModification(List<T> obj);
        public static void ForEach(Action<T> toDo)
        {
            foreach (var meta in Data)
            {
                toDo(meta.Value);
            }
        }
        public static void ForEach(Predicate<T> predicate, Action<T> toDo)
        {
            foreach (var meta in Data)
            {
                if (predicate(meta.Value)) toDo(meta.Value);
            }
        }
        
        public static List<T> GetAll(Predicate<T> predicate)
        {
            var match = new List<T>();
            foreach (var meta in Data)
            {
                if (predicate(meta.Value)) match.Add(meta.Value);
            }

            return match;
        }
        
        public static T Get(Predicate<T> predicate)
        {
            foreach (var meta in Data)
            {
                if (predicate(meta.Value)) return meta.Value;
            }

            return null;
        }
    }
}