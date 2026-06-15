using System;

using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.StageResult
{
    public class UI_StageRewardItem: UI_Base
    {
        private Image _itemImg;
        private TextMeshProUGUI _countText;

        public override bool Init()
        {
            if (!base.Init()) return false;
            _itemImg = Util.FindChild<Image>(gameObject, "IMG_Item", true);
            _countText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Reward", true);

            return true;
        }
        
        public void Set(UIReward reward)
        {
            if (!_isInit) Init();
            
            _itemImg.sprite = Manager.Resource.Load<Sprite>(reward.currency.Resource);
            _countText.text = reward.count.ToString();
        }
    }
}