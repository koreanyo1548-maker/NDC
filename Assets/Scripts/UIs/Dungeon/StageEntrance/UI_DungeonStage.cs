using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbDungeon;
using Data.Utils;
using dynamicscroll;
using Managers;
using TMPro;
using UIs.Dungeon.Entrance;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.StageEntrance
{
    public class UI_DungeonStage: UI_DungeonEntrance
    {
        private DynamicScroll<DungeonStageItem, UI_DungeonStage_Item> _stages;

        public UIField<int> CurStage { get; private set; }
        private Dictionary<FieldType, List<DungeonStageItem>> _stageData = new();
        private List<UI_DungeonStageReward_Item> _rewards = new();

        enum GameObjects
        {
            BookHp
        }
        
        enum Transforms
        {
            RewardParent
        }

        enum DynamicScrollRects
        {
            V_Stage
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Transform>(typeof(Transforms));
            Bind<DynamicScrollRect>(typeof(DynamicScrollRects));
            Bind<GameObject>(typeof(GameObjects));

            var awakeningList = new List<DungeonStageItem>();
            DbAwakeningDungeonLevel.ForEach(a =>
            {
                awakeningList.Add(new DungeonStageItem { level = a.Id });
            });
            _stageData.Add(FieldType.Awakening, awakeningList);
            var skillGrowthList = new List<DungeonStageItem>();
            DbSkillGrowthDungeonLevel.ForEach(a =>
            {
                skillGrowthList.Add(new DungeonStageItem { level = a.Id });
            });
            _stageData.Add(FieldType.SkillGrowth, skillGrowthList);
            var petDungeonList = new List<DungeonStageItem>();
            DbPetDungeonLevel.ForEach(a =>
            {
                petDungeonList.Add(new DungeonStageItem {level = a.Id});
            });
            _stageData.Add(FieldType.Pet, petDungeonList);
            
            CurStage = new UIField<int>(0);
            _stages = new DynamicScroll<DungeonStageItem, UI_DungeonStage_Item>();
            _stages.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.V_Stage), _stageData[_fieldType],
                -1,"Prefabs/UI/SubItem/UI_DungeonStage_Item");
          
            return true;
        }

        public override void SetFieldType(FieldType fieldType, bool isForceRefresh = false)
        {
            var isChanged = _fieldType != fieldType;
            base.SetFieldType(fieldType, isForceRefresh);
            if (isChanged || isForceRefresh)
            {
                _stages.ChangeList(_stageData[_fieldType], 0);
            }
            
            SetStage(LevelController.I.GetCurStage(fieldType));
            _stages.MoveToIndex(CurStage.Value-1, 0.1f);
            _stages.ForceUpdateList();
        }
        
        public void SetStage(int stage)
        {
            var maxStage = DbSelector.GetMaxStage(_fieldType);
            if (stage > maxStage) stage = maxStage;
            CurStage.Value = stage;
            var canClear = CurStage.Value < LevelController.I.GetCurStage(_fieldType);
            var stageMeta = DbSelector.GetStage(_fieldType, CurStage.Value);
            
            var power = stageMeta.GetPower();
            Get<TextMeshProUGUI>((int) Texts.T_Power).text = Define.AddUnit(power, 3, 2);
            Get<TextMeshProUGUI>((int)Texts.T_Power).color = power > TotalStatController.Power.Value ? Define.ColorFF454A : Define.ColorFFF8AA;
            Get<Image>((int)Images.B_Clear).material = Define.GetUIMaterial(!CanClear());

            var isPet = _fieldType == FieldType.Pet;
            Get<GameObject>((int) GameObjects.BookHp).SetActive(isPet);
            if (isPet) Get<TextMeshProUGUI>((int) Texts.T_BookHp).text = LevelController.I.GetBibleHp().ToString();
            SetRewards();

            void SetRewards()
            {
                var parent = Get<Transform>((int) Transforms.RewardParent);
                var reward = DbSelector.GetReward(_fieldType, CurStage.Value);
                var rewards = new List<DbReward>();
                rewards.AddRange(reward.GetFirstClearReward());
                var firstRewardCount = rewards.Count;
                if (reward.IsRewardStatic())
                {
                    rewards.AddRange(reward.GetRewards());
                    SetRewardCount(rewards.Count, parent);
                    for (var idx = 0; idx < rewards.Count; ++idx)
                    {
                        SetReward(idx, idx < firstRewardCount, rewards[idx].currencyType, Define.AddUnit(rewards[idx].count, 3, 2), canClear);
                    }
                    Get<TextMeshProUGUI>((int) Texts.T_RewardCount).text = string.Empty;
                }
                else
                {
                    var probabilities = reward.GetProbabilities();
                    SetRewardCount(firstRewardCount + probabilities.Count, parent);
                    for (var idx = 0; idx < firstRewardCount; ++idx)
                    {
                        SetReward(idx, true, rewards[idx].currencyType, rewards[idx].count.ToString(), canClear);
                    }
                    for (var idx = firstRewardCount; idx < firstRewardCount + probabilities.Count; ++idx)
                    {
                        SetReward(idx, false, probabilities[idx - firstRewardCount].category, probabilities[idx - firstRewardCount].probability + "%", canClear);
                    }
                    Get<TextMeshProUGUI>((int) Texts.T_RewardCount).text = string.Format(LocalString.Get(210128), reward.GetRewardCounts());
                }
            }

            void SetRewardCount(int count, Transform parent)
            {
                for (var idx = _rewards.Count; idx < count; ++idx) _rewards.Add(Manager.UI.MakeSubItem<UI_DungeonStageReward_Item>(parent));
                for (var idx = count; idx < _rewards.Count; ++idx) _rewards[idx].gameObject.SetActive(false);
            }

            void SetReward(int idx, bool isFirst, CurrencyType currency, string countText, bool isReceived)
            {
                _rewards[idx].Set(isFirst, Manager.Resource.Load<Sprite>(DbCurrency.Get(currency).Resource), countText, isReceived);
            }
        }

        protected override bool CanClear()
        {
            return base.CanClear() && CurStage.Value < LevelController.I.GetCurStage(_fieldType);
        }

        protected override void EnterStage()
        {
            Manager.Field.EnterDungeon(_fieldType, CurStage.Value);
            Manager.UI.CloseAllPopupUI();
        }


        protected override int GetStage()
        {
            return CurStage.Value;
        }
        protected override void ClearStage()
        {
            if (base.CanClear() && !CanClear())
            {
                Manager.UI.ShowSceneUI<UI_Toast>().SetText(200043);
            }
            else
            {
                Manager.UI.ShowPopupUI<UI_DungeonClear>().Set(_dungeonMeta, CurStage.Value);
            }
        }
        
    }
}