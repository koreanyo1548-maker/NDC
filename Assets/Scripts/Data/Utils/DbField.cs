using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Utils
{
    [JsonConverter(typeof(DbFieldConverter))]
    public abstract class DbField
    {
        public abstract string GetValue();
        public event EventHandler<DbEventArgs> ValueChanged;

        protected void OnValueChanged(int id)
        {
            var raiseEvent = ValueChanged;

            if (raiseEvent != null)
            {
                raiseEvent(this, new DbEventArgs(id));
            }
        }
    }

    [Serializable]
    public class DbFieldWithoutParent<T> : DbField
    {
        private int Id;
        private T value;

        public override string GetValue()
        {
            return Value.ToString();
        }
        
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged(Id);
            }
        }

        public DbFieldWithoutParent<T> SetId(int id)
        {
            Id = id;
            return this;
        }

        public DbFieldWithoutParent(T value, int id)
        {
            this.value = value;
            this.Id = id;
        }

        public DbFieldWithoutParent(T value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public class DbField<T>: DbField
    {
        private int Id;
        private T value;
        private DbUserModel model;

        public override string GetValue()
        {
            return Value.ToString();
        }
        
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged(Id);
                model.OnAnyValueChanged(new DbEventArgs(Id));
            }
        }

        public DbField<T> SetId(int id, DbUserModel parent)
        {
            Id = id;
            model = parent;
            return this;
        }

        public DbField(T value, int id, DbUserModel parent)
        {
            this.value = value;
            this.Id = id;
            model = parent;
        }

        public DbField(T value)
        {
            this.value = value;
        }
    }
    
    [Serializable]
    public class DbFieldList<T>: DbField
    {
        private int Id;
        private List<DbField<T>> value;
        private DbUserModel model;
        
        public override string GetValue()
        {
            return JsonConvert.SerializeObject(Value);
        }
        
        public List<DbField<T>> Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged(Id);
                model.OnAnyValueChanged(new DbEventArgs(Id));
            }
        }

        public void SetValue(int idx, T set)
        {
            value[idx].Value = set;
            OnValueChanged(Id);
            model.OnAnyValueChanged(new DbEventArgs(Id));
        }

        public void SetValue(Predicate<T> predicate, T set)
        {
            for (var idx = 0; idx < value.Count; ++idx)
            {
                if (predicate(value[idx].Value))
                {
                    SetValue(idx, set);
                    return;
                }
            }
        }

        public void Add(DbField<T> added)
        {
            Value.Add(added);
            OnValueChanged(Id);
        }

        public void Remove(DbField<T> removed)
        {
            if (removed == null) return;
            Value.Remove(removed);
            OnValueChanged(Id);
        }

        public void ValueUpdated(int id)
        {
            OnValueChanged(id);
        }

        public void ForEach(Action<T> toDo)
        {
            var count = value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                toDo(value[idx].Value);
            }
        }

        public bool Have(Predicate<T> predicate)
        {
            for (var idx = 0; idx < value.Count; ++idx)
            {
                if (predicate(value[idx].Value)) return true;
            }

            return false;
        }

        public T Get(Predicate<T> predicate)
        {
            for (var idx = 0; idx < value.Count; ++idx)
            {
                if (predicate(value[idx].Value)) return value[idx].Value;
            }

            throw new Exception("잘못된 값을 불러오려합니다.");
        }

        public DbFieldList<T> SetId(int id, DbUserModel parent)
        {
            Id = id;
            model = parent;
            return this;
        }

        public DbFieldList(List<T> value, int id, DbUserModel parent)
        {
            if (value == null) value = new();
            var values = new List<DbField<T>>(value.Count);
            foreach (var v in value) values.Add(new DbField<T>(v, id, parent));
            this.value = values;
            this.Id = id;
            model = parent;
        }

        public List<T> ToList()
        {
            var list = new List<T>(Value.Count);
            foreach (var v in Value)
            {
                list.Add(v.Value);
            }

            return list;
        }
    }
}