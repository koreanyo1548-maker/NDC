using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro;
using Features;
using MEC;
using TMPro;
using UnityEngine;

namespace Managers.Base
{
    public class PoolManager
    {
        private Dictionary<string, Pool> _pool = new();
        private Transform _root;
        public DamageNumber damage;

        public void Init()
        {
            if (_root == null)
            {
                _root = new GameObject {name = "@Pool_Root"}.transform;
                Object.DontDestroyOnLoad(_root);
                damage = Resources.Load<DamageNumber>("Prefabs/Fonts/DamageText");
            }
            //Timing.RunCoroutine(_DamageTest());
        }

        /*
        IEnumerator<float> _DamageTest()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(0.1f);
                damage.Spawn(new Vector3(Random.Range(15, 19), Random.Range(-7, 0)), Random.Range(10, 9999999));
            }
        }
        */

        public void Push(Poolable poolable)
        {
            var name = poolable.gameObject.name;
            if (!_pool.ContainsKey(name))
            {
                GameObject.Destroy(poolable.gameObject);
                return;
            }
            
            _pool[name].Push(poolable);
        }

        public void CreatePool(GameObject original, int count)
        {
            var pool = new Pool();
            pool.Init(original, count);
            pool.Root.parent = _root;
            
            _pool.Add(original.name, pool);
        }

        public Poolable Pop(GameObject original, int count, Transform parent = null)
        {
            if (!_pool.ContainsKey(original.name))
            {
                CreatePool(original, count);
            }

            return _pool[original.name].Pop(parent);
        }

        public GameObject GetOriginal(string name)
        {
            if (!_pool.ContainsKey(name))
            {
                return null;
            }

            return _pool[name].Original;
        }

        public void Clear()
        {
            foreach (Transform child in _root)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            _pool.Clear();
        }
    }
}