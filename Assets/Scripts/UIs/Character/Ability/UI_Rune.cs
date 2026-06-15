using System;
using Data.DbAbility;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_Rune: UI_Popup
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            var parent = Util.FindChild<Transform>(gameObject, "V_Rune", true);
            for (var idx = 0; idx < 5; ++idx)
            {
                parent.GetChild(idx).gameObject.GetOrAddComponent<UI_Rune_Item>().Set(DbAbilityRune.GetAtIndex(idx));
            }
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