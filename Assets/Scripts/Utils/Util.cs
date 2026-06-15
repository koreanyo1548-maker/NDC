using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    public static class Util
    {
        public static T FindChild<T>(GameObject go, string name = null, bool isRecursive = false) where T : Object
        {
            if (go == null)
            {
                return null;
            }

            var returnAny = string.IsNullOrEmpty(name);
            if (isRecursive)
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (returnAny || component.name.Equals(name))
                        return component;
                }
            }
            else
            {
                var goT = go.transform;
                var childCount = goT.childCount;
                for (var idx = 0; idx < childCount; ++idx)
                {
                    var transform = goT.GetChild(idx);
                    if (returnAny || transform.name.Equals(name))
                    {
                        T component = transform.GetComponent<T>();
                        if (component != null)
                            return component;
                    }
                }
            }

            return null;
        }

        public static GameObject FindChild(GameObject go, string name = null, bool isRecursive = false)
        {
            var transform = FindChild<Transform>(go, name, isRecursive);
            if (transform == null)
                return null;

            return transform.gameObject;
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component == null)
                component = go.AddComponent<T>();
            return component;
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, list);
            stream.Position = 0;
            return (List<T>) formatter.Deserialize(stream);
        }
    }
}