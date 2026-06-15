using System.Collections.Generic;
using Controller.Currency;
using Data;
using Data.DbUser.Currency;
using Managers;
using UIBases;
using UIs.AdBuff;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;

namespace UIs.FieldMain.MainLeft
{
    public class UI_MainAdBuff: UI_Base
    {
        private List<DbUserAdBuff> adBuff = new();

        private Sprite[] _adBuffSprites;
        private Transform _adBuffImg;
        private GameObject _offObj;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            gameObject.BindEvent(Functions.TrueCondition, _ => OpenAdBuff(), UIEffectType.Bounce);

            _adBuffImg = Util.FindChild<Transform>(gameObject, "IMG_AdBuff", true);
            adBuff.Add(CurrencyController.I.GetAdBuff(AdBuffType.GoldBuff));
            adBuff.Add(CurrencyController.I.GetAdBuff(AdBuffType.GrowthStoneBuff));
            Util.FindChild(gameObject, "GoldBuff", true).GetOrAddComponent<UI_MainAdBuff_Item>()
                .Set(adBuff[0]);
            Util.FindChild(gameObject, "GrowthStoneBuff", true).GetOrAddComponent<UI_MainAdBuff_Item>()
                .Set(adBuff[1]);

            _adBuffSprites = new[] {Manager.Resource.Load<Sprite>("Icon_AdBuff_Off"), Manager.Resource.Load<Sprite>("Icon_AdBuff")};
            adBuff[0].IsUsing.ValueChanged += (_, _) => WhenAdUsingChanged();
            adBuff[1].IsUsing.ValueChanged += (_, _) => WhenAdUsingChanged();

            _offObj = Util.FindChild(gameObject, "IMG_AdIcon");
            
            WhenAdUsingChanged();
            
            return true;
        }

        private Vector3 _rotate = new Vector3(0, 0, 360);
        private void WhenAdUsingChanged()
        {
            var isUsing = adBuff[0].IsUsing.Value || adBuff[1].IsUsing.Value;
            _offObj.SetActive(!isUsing);
            _adBuffImg.GetComponent<Image>().sprite = _adBuffSprites[isUsing ? 1: 0];
            if (isUsing)
            {
                _adBuffImg.DOLocalRotate(_rotate, 3, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            }
            else
            {
                _adBuffImg.DOKill();
            }
        }

        private void OpenAdBuff()
        {
            Manager.UI.ShowPopupUI<UI_AdBuff>();
        }

    }
}