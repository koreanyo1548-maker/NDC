using System;
using Controller.Have;
using Data;
using Data.DbAbility;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_Rune_Item: UI_Base
    {
        private StatType _rune;
        
        enum Texts
        {
            T_Level,
            T_Description
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            return true;
        }

        public void Set(StatType rune)
        {
            if (!_isInit) Init();
            _rune = rune;
            var runeMeta = DbAbilityRune.Get(rune);
            Util.FindChild<Image>(gameObject, "IMG_Rune", true).sprite
                = Manager.Resource.Load<Sprite>(runeMeta.Resource);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(runeMeta.NameId);
            SetInfo();
        }

        private void SetInfo()
        {
            var level = AbilityController.I.runeLevel[_rune].Value;
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), level);
            Get<TextMeshProUGUI>((int) Texts.T_Description).text =
                StringMaker.GetFinalString(_rune, level == 0 ? 0 : DbAbilityRune.Get(_rune).Value[level - 1]);
        }

        private void OnEnable()
        {
            if (_isInit) SetInfo();
        }
    }
}