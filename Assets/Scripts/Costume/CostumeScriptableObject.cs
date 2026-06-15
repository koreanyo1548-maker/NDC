using UnityEngine;
using UnityEngine.Serialization;

namespace Costume
{
    [CreateAssetMenu(fileName = "NewCostume", menuName = "ScriptableObjects/CostumeSciptableObject", order = 1)]
    public class CostumeScriptableObject: ScriptableObject
    {
        [Header("Base Sprites")]
        public Sprite head;
        public Sprite baseEyes;
        public Sprite baseFace;
        public Sprite baseHair;
        public Sprite baseMouth;
        public Sprite body;
        public Sprite cloak;

        [Header("Costume Overlays")]
        public Sprite eyes;
        public Sprite frontHead;
        public Sprite backHead;
        public Sprite face;

        [Header("Scale")]
        public float costumeScale = 1f;
        public bool isFullReplacement = false;
    }
}