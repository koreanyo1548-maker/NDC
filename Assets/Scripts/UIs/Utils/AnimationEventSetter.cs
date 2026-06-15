using System;
using UnityEngine;

namespace UIs.Utils
{
    public class AnimationEventSetter: MonoBehaviour
    {
        private Action _action;

        public void SetAction(Action action)
        {
            _action = action;
        }

        private void DoAction()
        {
            _action();
        }
    }
}