using Controller;
using Controller.Play;
using Features;
using SkillEffects;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

namespace Managers.Base
{
    public class ResourceManager
    {
        public T Load<T>(string path, bool isUI = true) where T : Object
        {
            if (typeof(T) == typeof(GameObject))
            {
                var name = path;
                var index = name.LastIndexOf('/');
                if (index >= 0)
                {
                    name = name.Substring(index + 1);
                }
     
                var go = Manager.Pool.GetOriginal(name);
                if (go != null)
                {
                    return go as T;
                }
            }

            if (typeof(T) == typeof(Sprite))
            {
                return Resources.Load<SpriteAtlas>(isUI ? "UI" : "Characters").GetSprite(path) as T;
            }
             
            return Resources.Load<T>(path);
        }

        public GameObject Instantiate(GameObject original)
        {
            return Instantiate("/" + original.name);
        }

        public GameObject InstantiateParticle(string path, int count = 1, Transform parent = null)
        {
            if (PlayController.I.isPowerSave) return null;
            return Instantiate(path, count, parent);
        }
        
        public GameObject Instantiate(string path, int count = 1, Transform parent = null)
        {
            var original = Load<GameObject>($"Prefabs/{path}");
            if (original == null)
            {
                //Debug.Log($"Failed to load prefab: {path}");
                return null;
            }
     
            if (original.GetComponent<Poolable>() != null)
            {
                return Manager.Pool.Pop(original, count, parent).gameObject;
            }
             
            var go = Object.Instantiate(original, parent);
            go.name = original.name;
            return go;
        }
     
        public void Destroy(GameObject go)
        {
            if (!go.IsValid()) return;
            
            var poolable = go.GetComponent<Poolable>();
            if (poolable != null)
            {
                Manager.Pool.Push(poolable);
                return;
            }
             
            Object.Destroy(go);
        }
    }
}