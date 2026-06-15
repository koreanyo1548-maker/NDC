using System;
using Data;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.StageResult
{
    public class UI_BossStage: UI_Scene
    {
        private bool _isPromotion;
        enum Texts
        {
            T_Boss
        }
        private void Start()
        {
            Init();
            transform.GetComponent<Canvas>().sortingOrder = 10;
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            
            return true;
        }

        public void SetToast(int textId)
        {
            if (!_isInit) Init();
            Manager.Sound.PlaySFX(SFXType.Boss);
            _isPromotion = textId == 210241;
            Get<TextMeshProUGUI>((int)Texts.T_Boss).text = LocalString.Get(textId);
        }

        public override bool NeedRaycast()
        {
            return false;
        }

        public void WhenAnimationEnd()
        {
            Manager.UI.CloseSingleUI(this);
            if (!_isPromotion && Manager.Field.CurField.Value == FieldType.Stage || _isPromotion && Manager.Field.CurField.Value == FieldType.Promotion) Manager.Field.SpawnGame(false, false);
        }
    }
}