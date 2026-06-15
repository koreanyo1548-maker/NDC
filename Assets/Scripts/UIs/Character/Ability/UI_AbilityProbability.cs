using System;
using Data;
using Data.DbAbility;
using Data.Utils;
using TMPro;
using UIBases;
using UIs.Utils;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_AbilityProbability: UI_Popup
    { 
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            for (var idx = 0; idx <= 6; ++idx)
            {
                Util.FindChild<TextMeshProUGUI>(gameObject, "T_Probability"+idx, true).text = 
                    DbAbilityOptionSummon.Get(GradeType.Normal+idx).Probability / 10000f + "%";
            }
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            
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