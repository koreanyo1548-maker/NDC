using Controller.Play;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Stat
{
    public class UI_StatUpCount: UI_Base
    {

        enum GameObjects
        {
            IMG_X1Check,
            IMG_X10Check,
            IMG_MaxCheck
        }
        
        enum Images
        {
            B_X1,
            B_X10,
            B_Max
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
           
            Get<Image>((int)Images.B_X1).gameObject.BindEvent(Functions.TrueCondition, _ => ChangeCount(1), UIEffectType.Bounce);
            Get<Image>((int)Images.B_X10).gameObject.BindEvent(Functions.TrueCondition, _ => ChangeCount(10), UIEffectType.Bounce);
            Get<Image>((int)Images.B_Max).gameObject.BindEvent(Functions.TrueCondition, _ => ChangeCount(-1), UIEffectType.Bounce);

            SetCheckMark();
            return true;
        }

        private void ChangeCount(int changeTo)
        {
            SettingController.I.ChangeStatUpCount(changeTo);
            SetCheckMark();
        }

        private void SetCheckMark()
        {
            var count = SettingController.data.StatUpCount.Value;
            Get<GameObject>((int)GameObjects.IMG_X1Check).SetActive(count == 1);
            Get<GameObject>((int)GameObjects.IMG_X10Check).SetActive(count == 10);
            Get<GameObject>((int)GameObjects.IMG_MaxCheck).SetActive(count == -1);
            Get<Image>((int) Images.B_X1).color = count == 1 ? Color.white : Color.clear;
            Get<Image>((int)Images.B_X10).color = count == 10 ? Color.white : Color.clear;
            Get<Image>((int)Images.B_Max).color = count == -1 ? Color.white : Color.clear;
        }
    }
}