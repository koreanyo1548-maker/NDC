using System;
using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Data.Utils;
using DG.Tweening;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Attend
{
    public class UI_Attend_Item: UI_Base
    {
        private int _day;
        
        public void Set(int day)
        {
            _day = day;
            transform.Find("T_Days").GetComponent<TextMeshProUGUI>().text = string.Format(LocalString.Get(210342), day);

            var rewards = DbAttendReward.Get(_day).Items;

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

            for (var idx = rewards.Count; idx < 2; ++idx)
            {
                rewardUIs[idx].gameObject.SetActive(false);
            }

            // var reward = DbAttendEventReward.Get(rewardId);
            // transform.Find("IMG_Item").GetComponent<Image>().sprite =
            //     Manager.Resource.Load<Sprite>(DbCurrency.Get(reward.Items[0].currencyType).GetResource());
            // transform.Find("T_Count").GetComponent<TextMeshProUGUI>().text = reward.Items[0].count.ToString("N0");
            
            SetStatus(day < AttendController.data.NextDay.Value);
        }

        public void SetStatus(bool isRewarded, bool useAnimation = false)
        {
            transform.Find("G_Clear").gameObject.SetActive(isRewarded);

            if (useAnimation)
            {
                var check = Util.FindChild<Transform>(gameObject, "IMG_Check", true);
                check.localScale = Define.Ten;
                check.DOScale(Define.One, 0.5f).SetEase(Ease.OutBounce);
            }
        }
    }
}