using Controller;
using Controller.Play;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_NicknameIDCard: UI_Scene
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            transform.GetComponent<Canvas>().sortingOrder = 100;
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Nickname", true).text = SettingController.Nickname;
            Util.FindChild<AnimationEventSetter>(gameObject, "SafeArea", true).SetAction(() =>
            {
                Manager.UI.CloseSceneUI(this);
                Manager.Field.SpawnGame();
            });
            return true;
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}