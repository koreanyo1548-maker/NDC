using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIs.Utils
{
    public class AnimationMultipleEventSetter: MonoBehaviour
    {
        private List<Action> _actions = new();

        public void SetAction(Action action)
        {
            _actions.Add(action);
        }

        private void DoAction(int idx)
        {
            _actions[idx]();
        }
    }
}