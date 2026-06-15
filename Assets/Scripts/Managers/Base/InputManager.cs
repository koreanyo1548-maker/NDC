using System;
using UIs.Etc;
using UIs.Etc.Warning;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Managers.Base
{
    public class InputManager
    {
        public bool blockBackKey;
        

        public void OnUpdate()
        {
            if (!blockBackKey && Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Manager.UI.ClosePopupUI())
                {
                    Manager.UI.ShowPopupUI<UI_Quit>();
                }
                else
                {
                    Manager.Sound.PlaySFX(SFXType.UI_Close);
                }
            }
        }
    }
}