using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utils;

namespace Features
{
    public class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        private Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;

            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (var idx = 0; idx < count; ++idx) Push(Create());
        }

        private Poolable Create()
        {
            var go = Object.Instantiate(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null) return;

            poolable.transform.SetParent(Root);
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;
            
            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
            {
                poolable = _poolStack.Pop();
            }
            else
            {
                poolable = Create();
            }
            
            if (parent == null)
            {
                poolable.transform.SetParent(Manager.EffectParent);
            }

            poolable.transform.SetParent(parent);
            poolable.IsUsing = true;
            poolable.gameObject.SetActive(true);

            return poolable;
        }
    }
}