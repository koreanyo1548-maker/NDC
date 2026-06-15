using System;
using Data.DbCommon;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Dungeon.TrainingGround;
using UIs.Utils;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_BuyPopup: UI_Popup
    {
        private Action _buyAction;
        enum Texts
        {
            T_Name,
            T_Info,
            T_DiaCost
        }
        
        private UI_Normal_Item _item;

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Util.FindChild(gameObject, "B_Buy", true).BindEvent(Functions.TrueCondition, _ => Buy());
            _item = Util.FindChild<UI_Normal_Item>(gameObject, "UI_Normal_Item", true);
            
            return true;
        }

        public void Set(DbReward reward, Action buyAction, long cost)
        {
            if (!_isInit) Init();

            _buyAction = buyAction;
            var costume = DbCostume.Get(reward.id);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(costume.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Info).text = StringMaker.GetCostumeOptionString(costume);
            Get<TextMeshProUGUI>((int) Texts.T_DiaCost).text = cost.ToString("N0");
            _item.Set(reward);
        }

        private void Buy()
        {
            _buyAction();
            ClosePopupUI();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}