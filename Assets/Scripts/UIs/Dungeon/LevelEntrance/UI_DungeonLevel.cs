using System;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.Utils;
using Exceptions;
using Managers;
using TMPro;
using UIs.Dungeon.Entrance;
using UIs.Toast;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.LevelEntrance
{
    public class UI_DungeonLevel: UI_DungeonEntrance
    {
        private int _level;
        
        public override void SetFieldType(FieldType fieldType, bool isForceRefresh = false)
        {
            base.SetFieldType(fieldType);
            _level = Math.Min(LevelController.I.GetCurStage(CurDungeon), DbSelector.GetMaxStage(fieldType));
            
            var reward = DbSelector.GetReward(fieldType, _level).GetRewards();
            Get<Image>((int) Images.IMG_Reward).sprite =
                DbCurrency.Get(reward[0].currencyType).GetResource(reward[0].id);
            
            var rewardAmount = 0L;
            switch (fieldType)
            {
                case FieldType.BlackMarket:
                {
                    rewardAmount = reward[0].count;
                    break;
                }
                case FieldType.Dia:
                {
                    rewardAmount = LevelController.data.DiaDungeonReward.Value;
                    break;
                }
                default: throw new NotDefinedFieldException(fieldType);
            }

            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = Define.AddUnit(rewardAmount, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_MaxLevel).text = "Lv." + _level;
        }
        
        protected override bool CanClear()
        {
            return base.CanClear() && LevelController.I.GetCurStage(_fieldType) > 1;
        }
        
        protected override void EnterStage()
        {
            Manager.Field.EnterDungeon(_fieldType, Mathf.Max(1, _level-2));
            Manager.UI.CloseAllPopupUI();
        }

        protected override int GetStage()
        {
            return LevelController.I.GetCurStage(CurDungeon);
        }
        
        protected override void ClearStage()
        {
            if (base.CanClear() && !CanClear())
            {
                Manager.UI.ShowSceneUI<UI_Toast>().SetText(200043);
            }
            else
            {
                Manager.UI.ShowPopupUI<UI_DungeonClear>()
                    .Set(_dungeonMeta, Math.Max(1, LevelController.I.GetCurStage(CurDungeon)));
            }
        }
    }
}