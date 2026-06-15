using System.Collections.Generic;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbShop;
using Data.DbUser.Currency;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.FieldMain.MainLeft
{
    public class UI_MainAdBuff_Item: UI_Base
    {
        private TextMeshProUGUI _timeText;

        private DbUserAdBuff _adBuff;

        private CoroutineHandle _timeRoutine;
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            _timeText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Time", true);
            
            return true;
        }
        
        public void Set(DbUserAdBuff adBuff)
        {
            if (!_isInit) Init();

            transform.GetComponent<Image>().sprite = Manager.Resource.Load<Sprite>(DbAdBuff.Get(adBuff.AdBuffType).MainResource);
            _adBuff = adBuff;
            WhenUsingChanged();
            
            _adBuff.IsUsing.ValueChanged += (_, _) => WhenUsingChanged();
        }

        private void WhenUsingChanged()
        {
            if (_adBuff.IsUsing.Value)
            {
                gameObject.SetActive(true);
                Timing.KillCoroutines(_timeRoutine);
                if (CurrencyController.I.Have(CurrencyType.AdSkip))
                {
                    _timeText.text = string.Empty;
                }
                else
                {
                    _timeRoutine = Timing.RunCoroutine(_AdBuffTimeRoutine());
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator<float> _AdBuffTimeRoutine()
        {
            var time = _adBuff.LeftTime;
            while (time > 0)
            {
                var leftSeconds = time % 60;
                _timeText.text = (time/60) + "m";
                while (leftSeconds-- >= 0)
                {
                    yield return Timing.WaitForSeconds(1);
                    time--;
                    _adBuff.LeftTime--;
                }
            }
            _adBuff.IsUsing.Value = false;
            TotalStatController.I.Apply(_adBuff.BuffType);
        }
    }
}