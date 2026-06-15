using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbShop;
using Data.Utils;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Cheat;
using UIs.Etc;
using UIs.Etc.PlayerInfo;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.FieldMain
{
    public class UI_MainTop: UI_Scene
    {
        public Transform Dia;
        
        enum Texts
        {
            T_Gold,
            T_Dia,
            T_Level,
            T_ExpPercentage,
            T_Power,
            T_Rank
        }

        enum Images
        {
            IMG_Profile
        }

        enum SlicedFilledImages
        {
            IMG_Exp
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Dia = Util.FindChild<Transform>(gameObject, "IMG_Dia", true);
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<SlicedFilledImage>(typeof(SlicedFilledImages));

            SetNickname();
            Util.FindChild(gameObject, "B_PlayerInfo", true).BindEvent(Functions.TrueCondition, _ =>
            {
                Manager.UI.ShowPopupUI<UI_PlayerInfo>();
            });
            // Util.FindChild(gameObject, "B_Cheat", true).BindEvent(Functions.TrueCondition, _ =>
            // {
            //     Manager.UI.ShowPopupUI<UI_Cheat>();
            // });

            CurrencyController.I.GetMoneyModel(CurrencyType.Gold).ValueChanged += (_,_) => WhenGoldChanged();
            CurrencyController.I.GetMoneyModel(CurrencyType.Dia).ValueChanged += (_,_) => WhenDiaChanged();
            LevelController.data.Exp.ValueChanged += (_,_) => WhenExpChanged();
            TotalStatController.Power.ValueChanged += (_,_) => WhenPowerChanged();
            EquipController.data.Profile.ValueChanged += (_, _) => WhenProfileChanged();

            WhenGoldChanged();
            WhenDiaChanged();
            WhenExpChanged();
            WhenPowerChanged();
            WhenProfileChanged();
            
            Timing.RunCoroutine(_GetStageRankingRoutine());

            return true;
        }

        public void SetNickname()
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = SettingController.Nickname;
        }

        private void WhenGoldChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Gold).text = Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.Gold).Value, 6, 0);
        }

        private void WhenDiaChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Dia).text = Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value, 6, 0);
        }

        private void WhenExpChanged()
        {
            var percentage =  LevelController.I.ExpPerNeed();
            Get<SlicedFilledImage>((int) SlicedFilledImages.IMG_Exp).fillAmount = percentage;
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = LevelController.data.Level.Value.ToString();
            Get<TextMeshProUGUI>((int) Texts.T_ExpPercentage).text = percentage.ToString("P0");
        }

        private void WhenPowerChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Power).text = Define.AddUnit(TotalStatController.Power.Value, 3, 2);
        }
        
        private void WhenProfileChanged()
        {
            Get<Image>((int) Images.IMG_Profile).sprite =
                Manager.Resource.Load<Sprite>(DbProfile.Get(EquipController.data.Profile.Value).Resource);
        }

        IEnumerator<float> _GetStageRankingRoutine()
        {
            PlayFabManager.Leaderboard.UpdateStage();
            
            yield return Timing.WaitForSeconds(900);
        }

        public void SetRanking(int rank)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Rank).text = rank.ToString();
        }

        public override bool NeedRaycast()
        {
            return true;
        }

    }
}