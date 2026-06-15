using System;
using Data;
using Data.DbDefinition;

using Data.Utils;
using dynamicscroll;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Summon
{
    public class UI_SummonBonus_Item : DynamicScrollObject<SummonBonusItem>
    {
        private GameObject _giftIcon;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _levelText;

        private Image _gradeImg;
        private Image _equipmentRewardImg;
        private Image _rewardImg;
        private TextMeshProUGUI _countText;
        private TextMeshProUGUI _gradeText;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _giftIcon = Util.FindChild(gameObject, "IMG_GiftIcon", true);
            _titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_LevelTitle", true);
            _levelText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_LevelInfo", true);
            _countText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Count", true);
            _gradeText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade", true);
            
            _gradeImg = Util.FindChild<Image>(gameObject, "IMG_Grade", true);
            _equipmentRewardImg = Util.FindChild<Image>(gameObject, "IMG_EquipmentReward", true);
            _rewardImg = Util.FindChild<Image>(gameObject, "IMG_Reward", true);
        }

        public override void UpdateScrollObject(SummonBonusItem bonus, int index)
        {
            base.UpdateScrollObject(bonus, index);

            var isEquipment = bonus.getId != 0;
            
            _titleText.text = bonus.isLast ? string.Format(LocalString.Get(210267), bonus.level) : 
                        string.Format(LocalString.Get(210100), StringMaker.GetSummonName(bonus.type), bonus.level);
            _levelText.text = bonus.isLast ? string.Empty : string.Format(LocalString.Get(210101), bonus.level - 1, bonus.dia);
            _giftIcon.SetActive(!bonus.isLast);
            _countText.text = Define.AddUnit(bonus.getCount, 3, 2);
            var grade = isEquipment ? DbGrade.Get(DbSelector.GetEquipment(bonus.type, bonus.getId).GetGrade()) : null;
            _gradeText.text = isEquipment ? LocalString.Get(grade.NameId) : string.Empty;

            (isEquipment ? _equipmentRewardImg : _rewardImg).sprite = DbCurrency.Get(bonus.type).GetResource(bonus.getId);
            (isEquipment ? _rewardImg : _equipmentRewardImg).sprite = Manager.Resource.Load<Sprite>(Define.EmptySprite);
            _gradeImg.sprite = Manager.Resource.Load<Sprite>(isEquipment ? grade.Id.ToString() : Define.EmptySprite);
        }
    }

    public class SummonBonusItem
    {
        public bool isLast;
        public CurrencyType type;
        public int level;
        public int dia;
        public int getId;
        public long getCount;
    }
}