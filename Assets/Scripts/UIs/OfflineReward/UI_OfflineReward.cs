using System;
using System.Collections.Generic;
using System.Numerics;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbRecord;
using Data.DbStage;
using Data.Stores;
using Managers;
using Managers.Base;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Attend;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace UIs.OfflineReward
{
    public class UI_OfflineReward: UI_Popup, ILanguageSet
    {
        private List<DbRewardBig> _rewards = new(16);
        
        private int _prevPlayed = 0;
        private TimeSpan _rewardTime;

        private List<UI_OfflineReward_Item> _rewardItems = new();
        enum Texts
        {
            T_Time,
            T_Gold,
            T_Exp,
            T_WeaponStoneCount,
            T_AccessoryStoneCount,
            T_BeadsOreCount
        }
        
        enum Particles
        {
            GrPower_Impact_Light_01_1 = 0,
            DustMotesLively = 1,
            P_MagicDust0 = 2,
            P_MagicDust1 = 3,
            P_MagicDust2 = 4,
            P_MagicDust3 = 5,
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<ParticleSystem>(typeof(Particles));
            
            Util.FindChild(gameObject, "B_GetBonus", true).BindEvent(Functions.TrueCondition, TryGetBonus);
            Util.FindChild(gameObject, "B_GetNormal", true).BindEvent(Functions.TrueCondition, _ => Close());
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_GetBonus", true).text = string.Format(
                LocalString.Get(210167), DbPlay.Get(PlayType.OfflineReward).Value * 100);

            transform.GetComponent<Canvas>().sortingOrder = 100;
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            return true;
        }

        public void Set(TimeSpan time)
        {
            _rewardItems.Clear();
            Manager.Sound.PlaySFX(SFXType.Reward);
            Init();
            _prevPlayed = 0;
            if (time.TotalSeconds > 86400) time = new TimeSpan(24, 0, 0);
            _rewardTime = time;
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = time.TotalHours >= 24 ? "24:00:00" : time.ToString(@"hh\:mm\:ss");

            _rewards = CurrencyController.I.CalculateOfflineReward((long)time.TotalSeconds, CurrencyController.OfflineRewardType.All);
                
            Get<TextMeshProUGUI>((int) Texts.T_Gold).text = Define.AddUnit(_rewards.Find(r => r.currencyType == CurrencyType.Gold).count, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_Exp).text = Define.AddUnit(_rewards.Find(r => r.currencyType == CurrencyType.Exp).count, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_WeaponStoneCount).text = Define.AddUnit(_rewards.Find(r => r.currencyType == CurrencyType.WeaponGrowthStone).count, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_AccessoryStoneCount).text = Define.AddUnit(_rewards.Find(r => r.currencyType == CurrencyType.AccessoryGrowthStone).count, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_BeadsOreCount).text = Define.AddUnit(_rewards.Find(r => r.currencyType == CurrencyType.BeadsOre).count, 3, 2);
            
            var rewardParent = Util.FindChild<Transform>(gameObject, "Rewards", true);
            while (rewardParent.childCount > 8)
            {
                Manager.Resource.Destroy(rewardParent.GetChild(8).gameObject);
            }
            rewardParent.GetChild(7).gameObject.SetActive(_rewards.Find(r => r.currencyType == CurrencyType.BeadsOre).count > 0);
            
            var weapons = _rewards.FindAll(r => r.currencyType == CurrencyType.Weapon);
            for (var idx = 0; idx < weapons.Count; ++idx)
            {
                var item = Manager.UI.MakeSubItem<UI_OfflineReward_Item>(rewardParent);
                item.Set(DbWeapon.Get(weapons[idx].id), weapons[idx].count);
                _rewardItems.Add(item);
            } 

            var accessories = _rewards.FindAll(r => r.currencyType == CurrencyType.Weapon);
            for (var idx = 0; idx < accessories.Count; ++idx)
            {
                var item = Manager.UI.MakeSubItem<UI_OfflineReward_Item>(rewardParent);
                item.Set(DbAccessory.Get(accessories[idx].id), accessories[idx].count);
                _rewardItems.Add(item);
            }

            // 이벤트 시작 전 시간은 배제시키도록 확인
            var dropEventReward = DropEventController.I.TryGetOfflineReward((long)time.TotalSeconds);
            if (dropEventReward != null)
            {
                var item = Manager.UI.MakeSubItem<UI_RewardToast_Item>(rewardParent);
                item.Set(dropEventReward);
                _rewards.Add(dropEventReward);
            }
            
            GetReward(false);
        }

        private void TryGetBonus(PointerEventData eventData)
        {
            Manager.Ad.ShowAd(eAdType.OfflineReward, () => Timing.CallDelayed(0.1f, GetBonusAndClose));
        }

        private bool _isLast = false;
        private void PlayParticles(int max)
        {
            for (;_prevPlayed <= max; ++_prevPlayed)
            {
                var particle = Get<ParticleSystem>(_prevPlayed);
                if (particle.gameObject.activeSelf)
                {
                    particle.Simulate( 0.0f, true, true );
                    particle.Play();
                }
            }

            if (_isLast)
            {
                foreach (var item in _rewardItems) item.PlayParticle();
            }
            _isLast = true;
        }

        private void GetBonusAndClose()
        {
            GetReward(true);
            Close();
        }

        private void GetReward(bool isBonus)
        {
            foreach (var reward in _rewards)
            {
                if (isBonus && reward.currencyType == CurrencyType.DropEventMoney) continue;
                CurrencyController.I.GetReward(reward.currencyType, reward.count, reward.id);
            }
        }

        private void Close()
        {
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(200019);
            ClosePopupUI();
            
            if (AttendController.I.CanRewarded.Value)
            {
                Manager.UI.ShowPopupUI<UI_Attend>().SetSpawnGameWhenClose();
            }
            else
            {
                Manager.Field.SpawnGame();
            }
            UserInfo.saved.log.Add((int)_rewardTime.TotalSeconds, _rewards);
        }
            
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_GetBonus", true).text = string.Format(
                LocalString.Get(210167), DbPlay.Get(PlayType.OfflineReward).Value * 100);
        }
    }
}