using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Data.DbUser.Currency;
using Data.Stores;
using Data.Utils;
using dynamicscroll;
using Managers;
using TMPro;
using UIs.Toast;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Pass
{
    public class UI_SeasonPassReward_Item : DynamicScrollObject<SeasonPassRewardItem>
    {
        private EventsManager _pointEventManager;
        private EventsManager _diaEventManager;
        
        private Image _rewardImage;
        private Image _equipmentRewardImage;
        private Image _gradeImage;
        private Image _progressImage;
        private Image _passedImage;
        private Image _levelUpBtnImage;
        private TextMeshProUGUI _indexText;
        private TextMeshProUGUI _countText;
        private TextMeshProUGUI _gradeText;
        private GameObject _levelUpBtn;
        private GameObject _freeBgObj;
        private GameObject _premiumBgObj;
        private GameObject _disabled;
        private GameObject _received;
        private GameObject _premiumUnlocked;
        private GameObject _premiumLocked;
        private GameObject _freeUnlocked;
        private GameObject _passedStatus;
        private GameObject _premiumParticle;

        private SeasonPassRewardItem _reward;
        private DbSeasonPassReward _rewardMeta;

        private UI_Pass _passUI;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _passUI = Manager.UI.GetPopupUI<UI_Pass>();
            
            _rewardImage = Util.FindChild<Image>(gameObject, "IMG_Reward", true);
            _equipmentRewardImage = Util.FindChild<Image>(gameObject, "IMG_EquipmentReward", true);
            _gradeImage = Util.FindChild<Image>(gameObject, "IMG_Grade", true);
            _progressImage = Util.FindChild<Image>(gameObject, "IMG_GetLine", true);
            _passedImage = Util.FindChild<Image>(gameObject, "IMG_ReceivedBG", true);
            _levelUpBtn = Util.FindChild(gameObject, "B_LevelUp", true);
            _levelUpBtnImage = _levelUpBtn.GetComponent<Image>();
            _indexText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_PassNum", true);
            _countText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Count", true);
            _gradeText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade", true);
            _freeBgObj = Util.FindChild(gameObject, "IMG_FreeBg", true);
            _premiumBgObj = Util.FindChild(gameObject, "IMG_PremiumBg", true);
            _disabled = Util.FindChild(gameObject, "IMG_Disable", true);
            _received = Util.FindChild(gameObject, "IMG_Received", true);
            _premiumLocked = Util.FindChild(gameObject, "IMG_LockIcon", true);
            _premiumUnlocked = Util.FindChild(gameObject, "IMG_UnlockIcon", true);
            _premiumParticle = Util.FindChild(gameObject, "PremiumParticle", true);
            _freeUnlocked = Util.FindChild(gameObject, "IMG_Free", true);
            _passedStatus = Util.FindChild(gameObject, "IMG_PassNumBgGetReward", true);
            
            Util.FindChild(gameObject, "B_Reward", true).BindEvent(
                () => GetRewardCondition() || IsCostume(), GetReward, UIEffectType.Bounce, false);
            _levelUpBtn.BindEvent(LevelUpCondition, LevelUp, UIEffectType.Bounce);
            _pointEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetStatus,
                updatedController = new[] {SeasonPassController.I.CanGetReward, SeasonPassController.I.CanGetPoint}
            });
            _diaEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetLevelUpButtonEnable,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });
        }

        private bool GetRewardCondition()
        {
            var nextRewardId = SeasonPassController.I.NextRewarded.Value;
            if (_rewardMeta.Id > nextRewardId) return false;
            return SeasonPassController.data.Point.Value >= _rewardMeta.NeedPoint 
                   && !SeasonPassController.data.Rewarded.Value.Contains(_reward.rewardId);
        }

        private bool IsCostume()
        {
            return _rewardMeta.RewardType == CurrencyType.Costume;
        }

        private void GetReward(PointerEventData eventData)
        {
            if (!GetRewardCondition())
            {
                OpenCostumeRewardInfo();
                return;
            }
            if (!_rewardMeta.IsFree && !SeasonPassController.I.IsSeasonPassPurchased())
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200061);
                return;
            }
            
            SeasonPassController.I.GetReward(_reward.rewardId);
            _passUI.UpdateSeasonPass();
        }

        private bool LevelUpCondition()
        {
            return (_rewardMeta.Id == SeasonPassController.I.GetLastRewardId() + 1 
                    || _rewardMeta.Id == SeasonPassController.I.NextRewarded.Value) &&
                   SeasonPassController.data.Point.Value < _rewardMeta.NeedPoint;
        }

        private void LevelUp(PointerEventData eventData)
        {
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            if (CurrencyController.I.TryUse(CurrencyType.Dia, 20000))
            {
                SeasonPassController.I.LevelUpTo(_reward.rewardId);
                _passUI.UpdateSeasonPass();
                CurrencyController.I.SetDiaLog($"시즌패스 레벨업 {_reward.rewardId} 구매", -20000, prev);
            }
        }

        private void OpenCostumeRewardInfo()
        {
            Manager.UI.ShowPopupUI<UI_CostumeInfo>().Set(DbCostume.Get(_rewardMeta.RewardId),
                _rewardImage.transform.position, Define.PivotMiddle2);
        }

        public override void UpdateScrollObject(SeasonPassRewardItem reward, int index)
        {
            base.UpdateScrollObject(reward, index);

            _reward = reward;
            _rewardMeta = DbSeasonPassReward.Get(reward.rewardId);
            _indexText.text = (reward.rewardId).ToString();

            _freeBgObj.SetActive(_rewardMeta.IsFree);
            _premiumBgObj.SetActive(!_rewardMeta.IsFree);
            var rewardCurrency = DbCurrency.Get(_rewardMeta.RewardType);

            var isEquipment = rewardCurrency.Id is CurrencyType.Accessory or CurrencyType.Weapon or CurrencyType.Costume;
            var grade = isEquipment ? rewardCurrency.Id == CurrencyType.Costume ? DbCostume.Get(_rewardMeta.RewardId).Grade :
                DbSelector.GetEquipment(rewardCurrency.Id, _rewardMeta.RewardId).GetGrade() : GradeType.Normal;
            _rewardImage.sprite = isEquipment ? Manager.Resource.Load<Sprite>(Define.EmptySprite) : rewardCurrency.GetResource(_rewardMeta.RewardId);
            _equipmentRewardImage.sprite = isEquipment ? rewardCurrency.GetResource(_rewardMeta.RewardId) : Manager.Resource.Load<Sprite>(Define.EmptySprite);
            _gradeImage.sprite = Manager.Resource.Load<Sprite>(isEquipment ? grade.ToString() : Define.EmptySprite);
            _gradeText.text = isEquipment ? LocalString.Get(DbGrade.Get(grade).NameId) : string.Empty;
            _countText.text = Define.AddUnit(_rewardMeta.RewardCount, 7, 0);
            
            SetStatus();
        }

        public void SetStatus()
        {
            _received.SetActive(SeasonPassController.data.Rewarded.Value.Contains(_reward.rewardId));

            var nextId = SeasonPassController.I.NextRewarded.Value;
            var isPrev = _reward.rewardId < nextId; 
            var isCurrent = _reward.rewardId == nextId;
            var canPremium = SeasonPassController.I.IsSeasonPassPurchased();
            _disabled.SetActive(!GetRewardCondition() || (!_rewardMeta.IsFree && !canPremium));
            
            if (isCurrent) _pointEventManager?.Reconnect();
            else _pointEventManager?.Dispose();
            
            _premiumParticle.SetActive(!_rewardMeta.IsFree);
            _premiumUnlocked.SetActive(canPremium && !_rewardMeta.IsFree);
            _premiumLocked.SetActive(!canPremium && !_rewardMeta.IsFree);
            _freeUnlocked.SetActive(_rewardMeta.IsFree);

            _progressImage.fillAmount = isCurrent ? 0.5f : isPrev ? 1 :  0;
            _passedStatus.SetActive(isPrev || isCurrent);
            _passedImage.color = _reward.rewardId == Math.Max(1, nextId-1) ? Define.ColorF5C959 : isPrev ? Define.Color5D6682 :  Define.ColorTransparent;
            
            _levelUpBtn.SetActive(LevelUpCondition());
            if (_levelUpBtn.activeSelf)
            {
                _diaEventManager.Reconnect();
            }
            else
            {
                _diaEventManager.Dispose();
            }
        }

        private void SetLevelUpButtonEnable()
        {
            _levelUpBtnImage.material =
                Define.GetUIMaterial(CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value < 20000);
        }
    }

    public class SeasonPassRewardItem
    {
        public int rewardId;
    }
}