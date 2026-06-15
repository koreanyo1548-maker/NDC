using System;
using Data.DbShop;
using Managers;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_ProfileSelect: UI_Popup
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            
            var parent = Util.FindChild<Transform>(gameObject, "Content", true);
            DbProfile.ForEach(p =>
            {
                Manager.UI.MakeSubItem<UI_ProfileSelect_Item>(parent).Set(p);
            });
            
            return true;
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}