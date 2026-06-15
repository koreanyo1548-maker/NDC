using System;
using Controller;
using Controller.Infos;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Data.Utils;
using DG.Tweening;
using Managers;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.EventAttend
{
    public class UI_Attend7Days_Item: UI_Base, ILanguageSet
    {
        private int _rewardId;
        
        public void Set(int day, int rewardId)
        {
            _rewardId = rewardId;
            transform.Find("T_Days").GetComponent<TextMeshProUGUI>().text = string.Format(LocalString.Get(210342), day);

            var rewards = DbAttendEventReward.Get(rewardId).Items;

            var packageEquip = Util.FindChild(gameObject, "UI_PackageEquip_Item", true);
            var rewardUIs = new[]{Util.FindChild<Image>(gameObject, "IMG_Reward (1)", true), Util.FindChild<Image>(gameObject, "IMG_Reward (2)", true)};
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                var reward = rewards[idx];
                if (reward.currencyType is CurrencyType.Weapon or CurrencyType.Accessory or CurrencyType.Skill or CurrencyType.Costume)
                {
                    if (day != 7) Util.FindChild(gameObject, "G_RewardGroup", true).SetActive(false);
                    
                    var grade = reward.currencyType == CurrencyType.Costume ? DbCostume.Get(reward.id).Grade : DbSelector.GetEquipment(reward.currencyType, reward.id).GetGrade();
                    packageEquip.transform.GetComponent<UI_PackageEquip_Item>()
                        .Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id), reward.count, grade);
                }
                else
                {
                    packageEquip.SetActive(false);
                    rewardUIs[idx].sprite = DbCurrency.Get(reward.currencyType).GetResource(reward.id);
                    Util.FindChild<TextMeshProUGUI>(gameObject, $"T_Count ({idx + 1})", true).text =
                        reward.count.ToString("N0");
                }
            }

            if (rewards.Count == 1 && day != 7)
            {
                rewardUIs[1].gameObject.SetActive(false);
            }
            if (day == 7)
            {
                LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
                SetDay7Reward();
            }

            // var reward = DbAttendEventReward.Get(rewardId);
            // transform.Find("IMG_Item").GetComponent<Image>().sprite =
            //     Manager.Resource.Load<Sprite>(DbCurrency.Get(reward.Items[0].currencyType).GetResource());
            // transform.Find("T_Count").GetComponent<TextMeshProUGUI>().text = reward.Items[0].count.ToString("N0");
            
            SetStatus(EventAttendController.data.RewardedCount.Value >= day);
        }

        public void SetStatus(bool isRewarded, bool useAnimation = false, Action rewardPop = null)
        {
            transform.Find("G_Clear").gameObject.SetActive(isRewarded);

            if (useAnimation)
            {
                var check = Util.FindChild<Transform>(gameObject, "IMG_Check", true);
                check.localScale = Define.Ten;
                check.DOScale(Define.One, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    if (rewardPop != null) rewardPop();
                });
            }
        }
        
        private void SetDay7Reward()
        {
            var costume = DbCostume.Get(DbAttendEventReward.Get(_rewardId).Items[0].id);
            transform.Find("T_CostumeName").GetComponent<TextMeshProUGUI>().text = LocalString.Get(costume.NameId);
            transform.Find("T_CostumeEx").GetComponent<TextMeshProUGUI>().text =
                StringMaker.GetCostumeOptionString(costume);
        }

        public void OnLanguageChanged(Locale locale)
        {
            SetDay7Reward();
        }
    }
}