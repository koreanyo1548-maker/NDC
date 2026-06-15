using System;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Character;
using UIs.Dungeon;
using UIs.StageResult;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.FieldMain.MainLeft
{
    public class UI_MainQuest: UI_Base, ILanguageSet
    {
        enum Texts
        {
            T_RewardCount,
            T_Name,
            T_Count
        }

        enum Images
        {
            IMG_Reward
        }

        enum GameObjects
        {
            IMG_Glow,
            IMG_Hand
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));

            gameObject.BindEvent(() => CanRewarded() || CanLeadToGuide(), GetReward, UIEffectType.Bounce, false);

            QuestController.data.QuestId.ValueChanged += WhenQuestChanged;
            QuestController.data.DoCount.ValueChanged += WhenCountChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            WhenQuestChanged(this, null);

            return true;
        }

        private bool CanRewarded()
        {
            return QuestController.data.CanRewarded;
        }

        private bool CanLeadToGuide()
        {
            if (QuestController.data.GuideEffect != -1) return false;
            var toDo = QuestController.data.ToDo;
            if (toDo == QuestType.MonsterKillCount || toDo == QuestType.CheckStageClear) return false;
            return true;
        }

        private void GetReward(PointerEventData eventData)
        {
            if (CanRewarded())
            {
              Manager.Sound.PlaySFX(SFXType.UI_MainQuest);
              QuestController.I.GetMainQuestReward();
              Manager.UI.ShowSceneUI<UI_RewardEffect>().Set(Get<Image>((int)Images.IMG_Reward).transform.position);
            }
            else
            {
                var toDo = QuestController.data.ToDo;
                switch (toDo)
                {
                    case QuestType.CheckAttackLevel: case QuestType.CheckHpLevel:
                    case QuestType.CheckCriticalProbabilityLevel: case QuestType.CheckCriticalAttackBonusLevel:
                        Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(UI_MainBottom.GameObjects.B_Character);
                        Manager.UI.GetPopupUI<UI_Character>().OpenTab(UI_Character.Images.B_StatTab);
                        break;
                    case QuestType.CheckLevelUp:
                        Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(UI_MainBottom.GameObjects.B_Character);
                        Manager.UI.GetPopupUI<UI_Character>().OpenTab(UI_Character.Images.B_LevelTab);
                        break;
                    case QuestType.CheckWeaponSummon: case QuestType.CheckAccessorySummon: case QuestType.CheckSkillSummon:
                    case QuestType.WeaponSummon: case QuestType.AccessorySummon:
                        Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(UI_MainBottom.GameObjects.B_Summon);
                        break;
                    case QuestType.CheckSkill2Equip:
                        Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(UI_MainBottom.GameObjects.B_Skill);
                        break;  
                    case QuestType.CheckAwakeningDungeonClear:
                        Manager.UI.ShowPopupUI<UI_Dungeon>();
                        break;
                }
            }
        }

        private void WhenQuestChanged(object sender, EventArgs eventArgs)
        {
            var isGuide = QuestController.data.IsOnGuide.Value;
            if (isGuide)
            {
                var guide = QuestController.data.GuideMeta;
                var reward = guide.Rewards[0];
                Set(reward.currencyType, reward.count, guide.NameId, guide.Id - 10000);
            }
            else
            {
                if (QuestController.data.LoopCount.Value > DbPlay.Get(PlayType.MainQuestMaxLoop).Value)
                {
                    QuestController.data.QuestId.ValueChanged -= WhenQuestChanged;
                    QuestController.data.DoCount.ValueChanged -= WhenCountChanged;
                    LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
                    Manager.Resource.Destroy(gameObject);
                    return;
                }
                var meta = QuestController.data.MainMeta;
                Set(meta.RewardType, meta.RewardCounts, meta.NameId);
            }

            WhenCountChanged(this, null);
            
            
            void Set(CurrencyType reward, long rewardCount, int nameId, int guideId = 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(nameId); //(isGuide? guideId + "." : string.Empty) + LocalString.Get(nameId);
                Get<TextMeshProUGUI>((int) Texts.T_RewardCount).text = rewardCount.ToString();
                Get<Image>((int) Images.IMG_Reward).sprite =
                    Manager.Resource.Load<Sprite>(DbCurrency.Get(reward).Resource);
            }
        }

        private void WhenCountChanged(object sender, EventArgs eventArgs)
        {   
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = $"{QuestController.data.DoCount.Value}/{QuestController.data.Goal}";
            var canGet = CanRewarded();
            Get<GameObject>((int) GameObjects.IMG_Glow).SetActive(canGet);
            Get<GameObject>((int) GameObjects.IMG_Hand).SetActive(canGet);
        }

        public void OnLanguageChanged(Locale locale)
        {
            WhenQuestChanged(this, null);
        }
    }
}