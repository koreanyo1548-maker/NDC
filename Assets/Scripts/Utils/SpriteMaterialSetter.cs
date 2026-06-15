using System;
using Managers;
using UnityEngine;

namespace Utils
{
    public class SpriteMaterialSetter: MonoBehaviour
    {
        [SerializeField] private MaterialType material;

        private SpriteRenderer[] _sprites;
        
        private void Awake()
        {
            _sprites = transform.GetComponentsInChildren<SpriteRenderer>();
            var materialResource = GetMaterial();

            for (var idx = 0; idx < _sprites.Length; ++idx)
            {
                _sprites[idx].material = materialResource;
                _sprites[idx].color = Color.black;
            }
        }

        public void ChangeColor(Color color)
        {
            for (var idx = 0; idx < _sprites.Length; ++idx)
            {
                _sprites[idx].color = color;
            }
        }

        private Material GetMaterial()
        {
            switch (material)
            {
                case MaterialType.Screen: return Manager.Resource.Load<Material>("Materials/Sprites-Screen");
            }

            throw new Exception("존재하지 않는 메터리얼을 세팅하려 하고있습니다.");
        }

        enum MaterialType
        {
            Screen
        }
    }
}