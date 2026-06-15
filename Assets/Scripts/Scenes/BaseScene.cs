using System;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Scenes
{
    public abstract class BaseScene: MonoBehaviour
    {
        public SceneType SceneType { get; protected set; } = SceneType.Unknown;

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            var obj = FindObjectOfType(typeof(EventSystem));
            if (obj == null)
                Manager.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
        }

        public abstract void Clear();
    }
}