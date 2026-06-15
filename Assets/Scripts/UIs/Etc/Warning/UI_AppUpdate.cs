using System;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Etc.Warning
{
    public class UI_AppUpdate: MonoBehaviour
    {
        public void Set(Action toDo)
        {
            Util.FindChild(gameObject, "YesButton").BindEvent(Functions.TrueCondition, _ => toDo());
        }
    }
}



