using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Dungeon.StageEntrance
{
    public class UI_DungeonStageReward_Item : UI_Base
    {
        enum Texts
        {
            T_Reward
        }

        enum Images
        {
            IMG_Reward
        }

        enum GameObjects
        {
            IMG_FirstClearBG,
            IMG_Received
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            
            return true;
        }

        public void Set(bool isFirst, Sprite resource, string count, bool isReceived)
        {
            if (!_isInit) Init();
            
            Get<GameObject>((int)GameObjects.IMG_FirstClearBG).SetActive(isFirst);
            Get<Image>((int) Images.IMG_Reward).sprite = resource;
            Get<TextMeshProUGUI>((int)Texts.T_Reward).text = count;
            Get<GameObject>((int)GameObjects.IMG_Received).SetActive(isFirst && isReceived);
            
            gameObject.SetActive(true);
        }
    }
}