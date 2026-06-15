using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbDungeon;

using Data.Utils;
using Exceptions;
using Managers;
using Newtonsoft.Json;
using TMPro;
using UIBases;
using UIs.StageResult;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon
{
    public class UI_DungeonClear: UI_Popup
    {
        private DbDungeonMeta _dungeon;
        private int _stage;
        
        enum Texts
        {
            T_Ticket,
            T_Title,
            T_ClearCount,
            T_ClearInfo
        }

        enum Images
        {
            IMG_Ticket
        }

        private int _curCount;
        private int _maxCount;

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            
            Util.FindChild(gameObject, "B_Min", true).BindEvent(Functions.TrueCondition, _ => ChangeClearCount(1), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Max", true).BindEvent(Functions.TrueCondition, _ => ChangeClearCount(_maxCount), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Minus", true).BindEvent(Functions.TrueCondition, _ => ChangeClearCount(_curCount-1), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Plus", true).BindEvent(Functions.TrueCondition, _ => ChangeClearCount(_curCount+1), UIEffectType.Bounce);
            
            Util.FindChild(gameObject, "B_Clear", true).BindEvent(Functions.TrueCondition, _ => ClearStage(), UIEffectType.Bounce, false);
            
            return true;
        }

        public void Set(DbDungeonMeta dungeon, int stage)
        {
            if (!_isInit) Init();

            _dungeon = dungeon;
            _stage = stage;
            _curCount = 1;
            _maxCount = CurrencyController.I.GetTicketModel(dungeon.Use).Value;

            Get<TextMeshProUGUI>((int) Texts.T_Ticket).text = _maxCount.ToString("N0");
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = string.Format(LocalString.Get(210121), stage);
            Get<Image>((int)Images.IMG_Ticket).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(dungeon.Use).Resource);
            
            SetClearCount();
        }

        private void ChangeClearCount(int count)
        {
            if (count < 1 || count > _maxCount) return;

            _curCount = count;
            SetClearCount();
        }

        private void SetClearCount()
        {
            Get<TextMeshProUGUI>((int) Texts.T_ClearCount).text = _curCount.ToString();
            Get<TextMeshProUGUI>((int) Texts.T_ClearInfo).text = string.Format(LocalString.Get(210124), _curCount);
        }
        
        private void ClearStage()
        {
            LevelController.I.ClearDungeon(_dungeon, _dungeon.Use, _curCount, _stage);
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