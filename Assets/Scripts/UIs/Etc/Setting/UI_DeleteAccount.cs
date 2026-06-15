using System;
using Controller;
using Controller.Play;
using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using Utils;

namespace UIs.Etc
{
    public class UI_DeleteAccount: UI_Base
    {
        private TMP_InputField _nicknameInput;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _nicknameInput = Util.FindChild<TMP_InputField>(gameObject, "InputField_Nickname", true);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => gameObject.SetActive(false));
            Util.FindChild(gameObject, "B_Delete", true).BindEvent(Functions.TrueCondition, _ => TryDeleteAccount());
            
            return true;
        }

        private void TryDeleteAccount()
        {
            if (_nicknameInput.text.Equals(SettingController.Nickname))
            {
                //HiveManager.DeleteAccount();
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200044);
            }
        }
    }
}