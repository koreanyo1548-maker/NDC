using Managers;
using MEC;
using UIBases;
using UnityEngine;

namespace UIs.Toast
{
    public class UI_UpdateToast: UI_Scene
    {
        private void Start()
        {
            Init();
            transform.GetComponent<Canvas>().sortingOrder = 205;
        }

        private void WhenAnimationDone()
        {
            Manager.UI.CloseSingleUI(this);
        }

        public override bool NeedRaycast()
        {
            return false;
        }
    }
}