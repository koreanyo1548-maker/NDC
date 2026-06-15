using System;
using Controller.Currency;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Toast
{
    public class UI_RewardToast_Item : UI_Base
    {
        enum Texts
        {
            T_Count,
            T_EquipCount,
            T_Grade
        }

        enum Images
        {
            IMG_Reward,
            IMG_Grade,
            IMG_Equip
        }

        enum Particles
        {
            P_MagicDust
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<ParticleSystem>(typeof(Particles));
            
            return true;
        }

        public void Set(DbReward reward)
        {
            if (!_isInit) Init();

            var isEquip = IsEquip(reward.currencyType);

            Get<TextMeshProUGUI>((int) Texts.T_EquipCount).text = string.Empty;

            if (reward.currencyType is CurrencyType.OfflineExp)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.CalculateOfflineReward(
                    reward.count * 60, CurrencyController.OfflineRewardType.Exp)[0].count, 3, 2);
            }
            else if (reward.currencyType is CurrencyType.OfflineGold)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.CalculateOfflineReward(
                    reward.count * 60, CurrencyController.OfflineRewardType.Gold)[0].count, 3, 2);
            }
            else if (reward.currencyType is CurrencyType.OfflineAccessoryGrowthStone)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.CalculateOfflineReward(
                    reward.count * 60, CurrencyController.OfflineRewardType.AccessoryGrowthStone)[0].count, 3, 2);
            }
            else if (reward.currencyType is CurrencyType.OfflineWeaponGrowthStone)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.CalculateOfflineReward(
                    reward.count * 60, CurrencyController.OfflineRewardType.WeaponGrowthStone)[0].count, 3, 2);
            }
            else if (reward.currencyType == CurrencyType.OfflineReward)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Count).text = StringMaker.GetDayTimeString(new TimeSpan(0, 0, (int)reward.count, 0));
            }
            else
            {
                Get<TextMeshProUGUI>((int)(isEquip ? Texts.T_EquipCount : Texts.T_Count)).text = Define.AddUnit(reward.count, 3, 2);
                Get<TextMeshProUGUI>((int)(isEquip ? Texts.T_Count : Texts.T_EquipCount)).text = string.Empty;
            }
            var currency = DbCurrency.Get(reward.currencyType);
            Get<Image>((int)(isEquip ? Images.IMG_Equip : Images.IMG_Reward)).sprite = currency.GetResource(reward.id);
            Get<Image>((int)(isEquip ? Images.IMG_Reward : Images.IMG_Equip)).sprite = Manager.Resource.Load<Sprite>(Define.EmptySprite);

            var grade = isEquip ? DbGrade.Get(DbSelector.GetGrade(currency.Id, reward.id)) : null;
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = isEquip ? LocalString.Get(grade.NameId) : string.Empty;
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(isEquip ? grade.Id.ToString() : Define.EmptySprite);
            
            gameObject.SetActive(true);
        }

        private bool IsEquip(CurrencyType currency)
        {
            if (currency == CurrencyType.Weapon || currency == CurrencyType.Accessory ||
                currency == CurrencyType.Skill || currency == CurrencyType.Pet || currency == CurrencyType.Necklace)
            {
                return true;
            }

            return false;
        }

        public void Set(DbRewardBig reward)
        {
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(reward.count, 3, 2);
            Get<Image>((int)Images.IMG_Reward).sprite = DbCurrency.Get(reward.currencyType).GetResource(reward.id);
        
            gameObject.SetActive(true);
        }

        public void PlayParticles()
        {
            var particle = Get<ParticleSystem>((int)Particles.P_MagicDust);
            if (particle.gameObject.activeSelf)
            {
                particle.Simulate( 0.0f, true, true );
                particle.Play();
            }
        }
    }
}