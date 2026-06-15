using System;
using Data.DbShop;
using Managers;
using UIBases;
using UIs.Character;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.AdBuff
{
    public class UI_AdBuff: UI_Popup
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _=>ClosePopupUI(), UIEffectType.None, false);

            var parent = Util.FindChild<Transform>(gameObject, "AdBuffParent", true);
            
            DbAdBuff.ForEach(
                buff =>
                {
                    var item = Manager.UI.MakeSubItem<UI_AdBuff_Item>(parent);
                    item.SetInfo(buff);
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