using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIBases
{
    public class CanvasDefaultSetting: MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<CanvasScaler>().matchWidthOrHeight = Define.GetCanvasRatio();
        }
    }
}