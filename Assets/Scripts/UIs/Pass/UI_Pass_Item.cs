using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbShop;

using Data.DbUser.Currency;
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
    public class UI_Pass_Item : DynamicScrollObject<PassItem>
    {
        private Image _normalRewardImage;
        private Image _normalEquipmentRewardImage;
        private Image _normalGradeImage;
        private Image _premiumRewardImage;
        private Image _premiumEquipmentRewardImage;
        private Image _premiumGradeImage;
        private Image _progressImage;
        private Image _passedImage;
        private TextMeshProUGUI _indexText;
        private TextMeshProUGUI _normalCountText;
        private TextMeshProUGUI _premiumCountText;
        private TextMeshProUGUI _normalGradeText;
        private TextMeshProUGUI _premiumGradeText;
        private GameObject _normalDisabled;
        private GameObject _normalReceived;
        private GameObject _premiumDisabled;
        private GameObject _premiumReceived;
        private GameObject _premiumLocked1;
        private GameObject _premiumLocked2;
        private GameObject _passedStatus;
        private GameObject _nextStatus;

        private PassItem _pass;
        private DbUserPass _passInfo;
        
        private IDbPass _passMeta;

        private UI_Pass _passUI;

        private Sprite[] _passedSprites;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _passUI = Manager.UI.GetPopupUI<UI_Pass>();
            
            _normalRewardImage = Util.FindChild<Image>(gameObject, "IMG_NormalReward", true);
            _normalEquipmentRewardImage = Util.FindChild<Image>(gameObject, "IMG_NormalEquipmentReward", true);
            _normalGradeImage = Util.FindChild<Image>(gameObject, "IMG_NormalGrade", true);
            _premiumRewardImage = Util.FindChild<Image>(gameObject, "IMG_PremiumReward", true);
            _premiumEquipmentRewardImage = Util.FindChild<Image>(gameObject, "IMG_PremiumEquipmentReward", true);
            _premiumGradeImage = Util.FindChild<Image>(gameObject, "IMG_PremiumGrade", true);
            _progressImage = Util.FindChild<Image>(gameObject, "IMG_GetLine", true);
            _passedImage = Util.FindChild<Image>(gameObject, "IMG_ReceivedBG", true);
            _indexText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_PassNum", true);
            _normalCountText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_NormalCount", true);
            _premiumCountText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_PremiumCount", true);
            _normalGradeText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_NormalGrade", true);
            _premiumGradeText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_PremiumGrade", true);
            _normalDisabled = Util.FindChild(gameObject, "IMG_NormalDisable", true);
            _normalReceived = Util.FindChild(gameObject, "IMG_NormalReceived", true);
            _premiumDisabled = Util.FindChild(gameObject, "IMG_PremiumDisable", true);
            _premiumReceived = Util.FindChild(gameObject, "IMG_PremiumReceived", true);
            _premiumLocked1 = Util.FindChild(gameObject, "IMG_LockIconBG", true);
            _premiumLocked2 = Util.FindChild(gameObject, "IMG_LockIcon", true);
            _passedStatus = Util.FindChild(gameObject, "IMG_GetCircleBG", true);
            _nextStatus = Util.FindChild(gameObject, "IMG_PremiumBG", true);

            _passedSprites = new[]
            {
                Manager.Resource.Load<Sprite>("ItemFrame05_Demo_Bg_f"),
                Manager.Resource.Load<Sprite>("ItemFrame01_Demo_Gray")
            };
            
            Util.FindChild(gameObject, "B_Normal", true).BindEvent(NormalGetCondition, GetNormalReward, UIEffectType.Bounce, false);
            Util.FindChild(gameObject, "B_Premium", true).BindEvent(PremiumGetCondition, GetPremiumReward, UIEffectType.Bounce, false);
        }

        private bool NormalGetCondition()
        {
            return _pass.passId == _passInfo.LastFreeRewarded + 1 &&
                   _passInfo.Progress >= _passMeta.GetGoal();
        }

        private bool PremiumGetCondition()
        {
            return CurrencyController.I.Have(_pass.specificPassType)
                   && _pass.passId == _passInfo.LastPremiumRewarded[_passMeta.GetSpecificPassTypeIdx()] + 1 &&
                   _passInfo.Progress >= _passMeta.GetGoal();
        }

        private void GetNormalReward(PointerEventData eventData)
        {
            CurrencyController.I.GetReward(_passInfo.PassType, _passMeta, _pass.passId, true);
            _passUI.UpdateBasicPass();
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbReward>();
                rewardsForToast.Add(
                    new DbReward(_passMeta.GetFreeRewardType() , _passMeta.GetFreeRewardCounts(), _passMeta.GetFreeRewardValue()));
            toast.SetReward(210243, rewardsForToast);
        }

        private void GetPremiumReward(PointerEventData eventData)
        {
            CurrencyController.I.GetReward(_passInfo.PassType, _passMeta, _pass.passId, false);
            _passUI.UpdateBasicPass();
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbReward>();
            rewardsForToast.Add(
                new DbReward(_passMeta.GetPremiumRewardType(), _passMeta.GetPremiumRewardCounts(), _passMeta.GetPremiumRewardValue()));
            toast.SetReward(210243, rewardsForToast);
        }

        public override void UpdateScrollObject(PassItem pass, int index)
        {
            base.UpdateScrollObject(pass, index);

            _pass = pass;
            _passInfo = CurrencyController.I.GetPassInfo(pass.passType);
            _passMeta = DbSelector.GetPass(_pass.passType, _pass.passId);
            _indexText.text = (pass.index).ToString();
            
            var normalReward = DbCurrency.Get(_passMeta.GetFreeRewardType());
            var premiumReward = DbCurrency.Get(_passMeta.GetPremiumRewardType());

            var isNormalEquipment = normalReward.Id is CurrencyType.Accessory or CurrencyType.Weapon;
            SetReward(true, isNormalEquipment,
                normalReward.GetResource(_passMeta.GetFreeRewardValue()), _passMeta.GetFreeRewardCounts(),
                isNormalEquipment ? DbSelector.GetEquipment(normalReward.Id, _passMeta.GetFreeRewardValue()).GetGrade() : GradeType.Normal);
            
            var isPremiumEquipment = premiumReward.Id is CurrencyType.Accessory or CurrencyType.Weapon;
            SetReward(false, isPremiumEquipment, 
                premiumReward.GetResource(_passMeta.GetPremiumRewardValue()), _passMeta.GetPremiumRewardCounts(), 
                isPremiumEquipment ? DbSelector.GetEquipment(premiumReward.Id, _passMeta.GetPremiumRewardValue()).GetGrade() : GradeType.Normal);
            
            SetStatus();

            void SetReward(bool isNormal, bool isEquipment, Sprite reward, int count, GradeType grade = GradeType.Normal)
            {
                (isNormal ? _normalRewardImage : _premiumRewardImage).sprite = isEquipment ? Manager.Resource.Load<Sprite>(Define.EmptySprite) : reward;
                (isNormal ? _normalEquipmentRewardImage : _premiumEquipmentRewardImage).sprite = isEquipment ? reward : Manager.Resource.Load<Sprite>(Define.EmptySprite);
                (isNormal ? _normalGradeImage : _premiumGradeImage).sprite = Manager.Resource.Load<Sprite>(isEquipment ? grade.ToString() : Define.EmptySprite);
                (isNormal ? _normalGradeText : _premiumGradeText).text = isEquipment ? LocalString.Get(DbGrade.Get(grade).NameId) : string.Empty;
                (isNormal ? _normalCountText : _premiumCountText).text = Define.AddUnit(count, 7, 0);
            }
        }

        public void SetStatus()
        {
            var normalReceived = _passInfo.LastFreeRewarded >= _pass.passId;
            var premiumReceived = _passInfo.LastPremiumRewarded[_passMeta.GetSpecificPassTypeIdx()] >= _pass.passId; 
            _normalReceived.SetActive(normalReceived);
            _premiumReceived.SetActive(premiumReceived);

            var canGetNormal = NormalGetCondition();
            var isNormalCurrent = canGetNormal || (_passInfo.LastFreeRewarded == _pass.passId && !NextCanGetReward()) || 
                                  (_pass.index == 0 && _passInfo.LastFreeRewarded == DbSelector.GetFirstId(_passMeta.GetPassType(), _passMeta.GetSpecificPassType()) -1);
            var canPremium = CurrencyController.I.Have(_pass.specificPassType);
            _normalDisabled.SetActive(!canGetNormal);
            _premiumDisabled.SetActive(!PremiumGetCondition());
            
            _premiumLocked1.SetActive(!canPremium);
            _premiumLocked2.SetActive(!canPremium);

            _progressImage.fillAmount = isNormalCurrent ? 0.5f : normalReceived ? 1 :  0;
            _passedStatus.SetActive(normalReceived || isNormalCurrent);
            _passedImage.color = isNormalCurrent ? Define.ColorE7CA93 : normalReceived ? Define.Color767682 :  Define.ColorTransparent;
            _passedImage.sprite = _passedSprites[isNormalCurrent ? 0 : 1];
            _nextStatus.SetActive(!normalReceived && !isNormalCurrent);
            
            bool NextCanGetReward()
            {
                var nextMeta = DbSelector.GetPass(_passMeta.GetPassType(), _pass.passId + 1);
                return nextMeta != null && nextMeta.GetSpecificPassType() == _passMeta.GetSpecificPassType() 
                                        && _passInfo.Progress >= nextMeta.GetGoal();
            }
        }
    }

    public class PassItem
    {
        public int index;
        public PassType passType;
        public CurrencyType specificPassType;
        public int passId;
    }
}
