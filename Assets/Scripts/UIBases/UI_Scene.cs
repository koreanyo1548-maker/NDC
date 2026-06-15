using Managers;
using UnityEngine;
using Utils;

namespace UIBases
{
    public abstract class UI_Scene: UI_Base
    {

        public abstract bool NeedRaycast();
        public override bool Init()
        {
            if (!base.Init()) return false;
            Manager.UI.SetCanvas(gameObject, NeedRaycast(), false);
            return true;
        }
    }
}