using System;
using System.Collections.Generic;
using Cameras;
using Controller;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbUser;
using Data.DbUser.Currency;
using Data.Stores;
using Data.Utils;
using Fight.Units;
using Managers;
using Managers.Base;
using MEC;
using ThirdParty;
using UIs.Attend;
using UIs.Etc;
using UIs.Etc.PlayerInfo;
using UIs.FieldMain;
using UIs.FieldMain.MainLeft;
using UIs.FieldMain.MainSkill;
using UIs.FieldMain.MainStage;
using UIs.Joystick;
using UIs.OfflineReward;
using UIs.Quest;
using UIs.RewardLog;
using UnityEngine;
using Utils;

namespace Scenes
{
    public class FieldScene: MonoBehaviour
    {   
        // void OnGUI()
        // {
        //     Rect position = new Rect(50, 30, Screen.width, Screen.height);
        //
        //     float fps = 1.0f / Time.deltaTime;
        //     float ms = Time.deltaTime * 1000.0f;
        //     string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
        //
        //     GUIStyle style = new GUIStyle();
        //
        //     style.fontSize = 30;
        //     style.normal.textColor = Color.white;
        //     style.fontStyle = FontStyle.Bold;
        //
        //     GUI.Label(position, text, style);   
        // }

        private void Awake()
        {
            Init();
        }
        
        private void Init()
        {
            Instantiate(Resources.Load<UI_FadeOnScene>("Prefabs/UI/Scene/UI_FadeOnScene"), transform).FadeOut();
            
            CurrencyController.I.Init();
            SkillController.I.Init();
            QuestController.I.Init();
            SeasonPassController.I.Init();
            RelicController.I.Init();
            NecklaceController.I.Init();
            LevelController.I.SetFirstStage();
            NewbieQuestController.I.Init();
            Manager.Field.Init();

            var player = Manager.Resource.Instantiate("Characters/Player").transform.GetChild(0).GetChild(0).gameObject;
            player.GetOrAddComponent<Player>().Spawn(Manager.Field.CenterX, Manager.Field.CenterY+1.4f);

            Manager.Resource.Instantiate("Characters/Bible").GetOrAddComponent<Bible>().gameObject.SetActive(false);
            
            Camera.main.gameObject.GetOrAddComponent<CameraController>().Init(player.transform.parent);

            Manager.UI.ShowSceneUI<UI_MainBottom>();
            Manager.UI.ShowSceneUI<UI_MainTop>();
            Manager.UI.ShowSceneUI<UI_MainStage>();
            Manager.UI.ShowSceneUI<UI_MainRight>();
            Manager.UI.ShowSceneUI<UI_MainSkill>();
            Manager.UI.ShowSceneUI<UI_RewardLog>();
            Manager.UI.ShowSceneUI<UI_MainLeft>();
            // Manager.UI.ShowSceneUI<UI_MainChat>();

            Manager.UI.SetPowerSavingTimer();

            Manager.Time.StartTimeCheck();
            Manager.Sound.PlayBGM(BGMType.Field);
            
            Manager.Guide.Init();
            
            if (SettingController.Nickname == null)
            {
                PlayFabManager.Store.SetAutoNickname();
            }
            
            if (PlayFabManager.Store.OfflineTime.TotalSeconds > 100)
            {
                Manager.UI.ShowPopupUI<UI_OfflineReward>().Set(PlayFabManager.Store.OfflineTime);
            }
            else if (AttendController.I.CanRewarded.Value)
            {
                Manager.UI.ShowPopupUI<UI_Attend>().SetSpawnGameWhenClose();
            }
            else
            {
                Manager.Field.SpawnGame(true);
                Manager.UI.GetSceneUI<UI_MainStage>().WhenStageChanged();
            }

            GameObject.Find("@TopEffectCamera").GetOrAddComponent<TouchEffects>();

            PlayFabManager.Leaderboard.StartUpdating();

            //HiveManager.I.ShowPromotion(PromotionType.BANNER, false, () =>
            //{
            //    HiveManager.I.ShowPromotion(PromotionType.NOTICE);
            //});
        }
    }
}