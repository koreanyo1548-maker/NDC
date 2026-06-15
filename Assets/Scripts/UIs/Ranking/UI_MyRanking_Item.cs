using System;
using System.Numerics;
using Controller;
using Controller.Infos;
using Controller.Play;
using Data.DbRecord;
using Data.DbShop;
using Data.DbStage;

using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Ranking
{
    public class UI_MyRanking_Item: UI_Base
    {
        private Sprite[] _rankingSprites;

        enum Texts
        {
            T_Ranking,
            T_Level,
            T_Nickname,
            T_Title,
            T_Value
        }

        enum Images
        {
            IMG_Ranking,
            IMG_Profile
        }
        
        enum GameObjects
        {
            T_Ranking,
            IMG_Ranking
        }
        
        private void Awake()
        {
            Init();
        }

        public override bool Init()
        {
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            _rankingSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_Rankicon_1"), 
                    Manager.Resource.Load<Sprite>("UI_Rankicon_2"), 
                    Manager.Resource.Load<Sprite>("UI_Rankicon_3")};

            return true;
        }

        private void OnEnable()
        {
            Get<TextMeshProUGUI>((int)Texts.T_Nickname).text = SettingController.Nickname;
            Get<TextMeshProUGUI>((int)Texts.T_Level).text = string.Format(LocalString.Get(210041), LevelController.data.Level.Value);
            var title = EquipController.data.Title.Value;
            Get<TextMeshProUGUI>((int)Texts.T_Title).text = title == 0 ? LocalString.Get(210160) : DbTitle.Get(title).GetNameWithColor();
            Get<Image>((int)Images.IMG_Profile).sprite = 
                Manager.Resource.Load<Sprite>(DbProfile.Get(EquipController.data.Profile.Value).Resource);
        }

        public void Set(int ranking, RankingType rankingType)
        {
            Get<TextMeshProUGUI>((int)Texts.T_Value).text = 
                rankingType == RankingType.Power ? Define.AddUnit(LevelController.data.MaxPower.Value, 3, 2) 
                : rankingType == RankingType.Stage ? LevelController.data.MaxStage.Value.ToString()
                : Define.AddUnit(LevelController.data.MaxTraining.Value, 3, 2);
                
            if (ranking == 0)
            {
                Get<GameObject>((int)GameObjects.T_Ranking).SetActive(false);
                Get<GameObject>((int)GameObjects.IMG_Ranking).SetActive(false);
                return;
            }
            var useImage = ranking > 0 && ranking < 4;

            Get<GameObject>((int)GameObjects.T_Ranking).SetActive(!useImage);
            Get<GameObject>((int)GameObjects.IMG_Ranking).SetActive(useImage);
            if (useImage)
            {
                Get<Image>((int) Images.IMG_Ranking).sprite = _rankingSprites[ranking - 1];
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_Ranking).text = ranking.ToString();
            }
        }
    }
}