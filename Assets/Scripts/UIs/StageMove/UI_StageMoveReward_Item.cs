using System;
using Data;
using Data.DbDefinition;

using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.StageMove
{
    public class UI_StageMoveReward_Item: UI_Base
    {
        enum Texts
        {
            T_Grade
        }

        enum Images
        {
            IMG_Grade,
            IMG_Reward
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

            return true;
        }

        public void Set(string resource, GradeType grade)
        {
            if (!_isInit) Init();
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(grade.ToString());
            Get<Image>((int) Images.IMG_Reward).sprite = Manager.Resource.Load<Sprite>(resource);
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(grade).NameId);
        }
    }
}