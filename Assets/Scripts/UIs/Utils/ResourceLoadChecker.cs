using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Utils;
using Managers;
using MEC;
using UIs.Etc;
using UIs.Etc.Login;
using UnityEngine;
using Utils;

namespace UIs.Utils
{
    public class ResourceLoadChecker
    {
        private List<string> _popups = new()
        {
            "UI_AdBuff", "UI_AppUpdate", "UI_Book", "UI_Character", "UI_DefaultPopup", "UI_Dungeon", "UI_DungeonClear",
            "UI_DungeonLevel", "UI_DungeonStage", "UI_EquipAwakening", "UI_EquipGrowth", "UI_EquipMerge", "UI_ForceQuit",
            "UI_Inventory", "UI_Nickname", "UI_OfflineReward", "UI_Pass", "UI_Pet", "UI_PetInventory", "UI_PlayerInfo",
            "UI_PowerSaving", "UI_Promotion", "UI_Quest", "UI_Quit", "UI_Ranking", "UI_ResetLevelPoint", "UI_Setting",
            "UI_Shop", "UI_AdShop", "UI_SkillEquipGrowth", "UI_SkillInventory", "UI_StageMove", "UI_Summon", "UI_SummonBonus",
            "UI_SummonResult", "UI_UnlockBookshelf", "UI_UnlockSkillPreset", "UI_Mail", "UI_MailInfo", "UI_Chat", "UI_Attend",
            "UI_SummonProbability", "UI_DungeonTrainingGround", "UI_ProfileSelect", "UI_TrainingGroundClear", "UI_TrainingGroundReward",
            "UI_EventShop", "UI_NecklaceInfo", "UI_NecklaceGrowth"
        };

        private List<string> _scenesUIs = new()
        {
            "UI_AwakeningToast", "UI_BossStage", "UI_Fade", "UI_Guide", "UI_Joystick", "UI_MainBottom", "UI_MainLeft",
            "UI_MainRight", "UI_MainSkill", "UI_MainStage", "UI_MainTop", "UI_PetSummonEffect", "UI_PowerToast",
            "UI_RewardEffect", "UI_RewardLog", "UI_RewardToast", "UI_StageClear", "UI_StageFailed",
            "UI_Toast", "UI_UpdateToast", "UI_Loading", "UI_TrainingGroundCount"
        };

        private Dictionary<string, int> _items = new()
        {
            {"UI_AdBuff_Item", 2}, {"UI_Book_Item", 5}, {"UI_Dungeon_Item", 5}, {"UI_DungeonStage_Item", 5},
            {"UI_DungeonStageReward_Item", 5}, {"UI_Inventory_Item", 84}, {"UI_SkillInventory_Item", 28}, {"UI_LevelPoint_Item", 5},
            {"UI_Package_Item", 20}, {"UI_PackageEquip_Item", 5}, {"UI_Pass_Item", 20}, {"UI_Promotion_Item", 5},
            {"UI_Quest_Item", 86}, {"UI_Ranking_Item", 20}, {"UI_RewardLog_Item", 5}, {"UI_RewardToast_Item", 5}, {"UI_ShopDiaNormal_Item", 10},
            {"UI_ShopDiaSale_Item", 5}, {"UI_ShopDiaMileage_Item", 5}, {"UI_ShopNormal_Item", 10},
            {"UI_ShopPackage_Item", 12}, {"UI_ShopCostume_Item", 2}, {"UI_StageMove_Item", 10}, {"UI_Stat_Item", 5}, {"UI_Summon_Item", 3},
            {"UI_SummonBonus_Item", 8}, {"UI_SummonResult_Item", 32}, {"UI_Title_Item", 10}, {"UI_Mail_Item", 5}, {"UI_MailReward_Item", 20},
            {"UI_OfflineReward_Item", 6}, {"UI_StageMoveReward_Item", 32}, {"UI_Costume_Item", 10}, {"UI_ProfileSelect_Item",15},
            {"UI_Normal_Item", 3}, {"UI_TrainingGroundReward", 10}, {"UI_Relic_Item", 23}, {"UI_SeasonPassQuest_Item", 8},
            {"UI_SeasonPassReward_Item", 20}, {"UI_TrainingGroundReward_Item", 10}, {"UI_EventShop_Item", 9}, {"UI_NecklaceInfo_Item", 5},
            {"UI_Necklace_Item", 35}, {"UI_Dungeon_Item_Training", 1}, {"UI_Friend_Item", 20},

        };


        private int _totalCount;
        private int _loadCount;
        public void StartLoad()
        {
            _totalCount = _popups.Count + _scenesUIs.Count + _items.Count;
            Timing.RunCoroutine(_Loading());
        }

        IEnumerator<float> _Loading()
        {
            for (var idx = 0; idx < _scenesUIs.Count; ++idx)
            {
                DoWith(Manager.Resource.Instantiate("UI/Scene/" + _scenesUIs[idx]));
                yield return Timing.WaitForOneFrame;
            }
            
            for (var idx = 0; idx < _popups.Count; ++idx)
            {
                DoWith(Manager.Resource.Instantiate("UI/Popup/" + _popups[idx]));
                yield return Timing.WaitForOneFrame;
            }

            foreach (var item in _items)
            {
                DoWith(Manager.Resource.Instantiate("UI/SubItem/" + item.Key, item.Value));
                yield return Timing.WaitForOneFrame;
            }
            UI_Login.I.FinishLoading();

            void DoWith(GameObject obj)
            {
                Manager.Resource.Destroy(obj);
                _loadCount++;
                UI_Login.I.SetLoadingPercent(1f * _loadCount / _totalCount);
            }
        }
    }
    
}