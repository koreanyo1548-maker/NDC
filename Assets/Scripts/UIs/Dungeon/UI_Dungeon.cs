using System;
using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;

using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Dungeon.TrainingGround;
using UIs.FieldMain;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon
{
    public class UI_Dungeon: UI_Popup, ILanguageSet
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _=>ClosePopupUI(), UIEffectType.None, false);
            // Util.FindChild(gameObject, "B_TrainingCheat", true).BindEvent(Functions.TrueCondition, _ => {
            //     LevelController.data.TrainingGroundStage.Value = 0;
            //     LevelController.data.MaxTraining.Value = 0;
            //     Manager.UI.ShowSingleUI<UI_Toast>().SetText("훈련장 보상이 초기화 되었습니다.");
            // }, UIEffectType.Bounce);

            var dungeonParent = Util.FindChild<Transform>(gameObject, "G_DungeonParent", true);
            DbDungeonMeta.ForEach(d =>
            {
                if (d.Id == FieldType.Promotion) return;
                if (d.Id == FieldType.Training)
                {
                    var item = Manager.UI.MakeSubItem<UI_Dungeon_Item_Training>(dungeonParent);
                    item.Set(d);
                    item.gameObject.name += "_" + d.Id;
                }
                else
                {
                    var item = Manager.UI.MakeSubItem<UI_Dungeon_Item>(dungeonParent);
                    item.Set(d);
                    item.gameObject.name += "_" + d.Id;
                }
            });
            
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_DungeonInfo", true).text =
                StringMaker.GetResetTime(210114, DbCurrency.Get(CurrencyType.SkillGrowthDungeonTicket).DailyCharge.ToString(), 
                    DbCurrency.Get(CurrencyType.SkillGrowthDungeonTicket).MaxHave.ToString());
                
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            return true;
        }
        
        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_DungeonInfo", true).text =
                StringMaker.GetResetTime(210114, DbCurrency.Get(CurrencyType.SkillGrowthDungeonTicket).DailyCharge.ToString(), 
                    DbCurrency.Get(CurrencyType.SkillGrowthDungeonTicket).MaxHave.ToString());

        }
    }
}