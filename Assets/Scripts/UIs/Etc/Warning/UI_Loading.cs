using UIBases;
using UnityEngine;

namespace UIs.Etc.Warning
{
    public class UI_Loading: UI_Scene
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            transform.GetComponent<Canvas>().sortingOrder = 200;
            return true;
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}