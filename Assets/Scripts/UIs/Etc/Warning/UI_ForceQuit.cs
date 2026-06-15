using System;
using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Etc.Warning
{
    public class UI_ForceQuit: UI_Scene
    {
        public enum QuitType
        {
            DataConflict,
            LoginNetwork,
            Maintenance,
            MultiLogin,
            Network
        }
        
        private QuitType _quitReason;
        private TextMeshProUGUI _quitText;

        private Action<DateTime> _toDo;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Util.FindChild(gameObject, "B_Yes", true).BindEvent(Functions.TrueCondition, _ => CloseGame(), UIEffectType.Bounce);

            _quitText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Quit", true);
            transform.GetComponent<Canvas>().sortingOrder = 210;
            
            return true;
        }

        public void Set(QuitType quitReason)
        {
            if (!_isInit) Init();
            
            _toDo = null;
            _quitReason = quitReason;
            _quitText.text = LocalString.Get(_quitReason == QuitType.DataConflict ? 210196 : 
                _quitReason == QuitType.LoginNetwork ? 210271 :
                _quitReason == QuitType.Network ? 210271 :
                _quitReason == QuitType.Maintenance ? 210284 : 210285);

            if (_quitReason == QuitType.Maintenance || _quitReason == QuitType.MultiLogin)
            {
                Time.timeScale = 0;
            }
        }

        public void Set(QuitType quitReason, Action<DateTime> toDo)
        {
            Set(quitReason);
            _toDo = toDo;
        }

        private void CloseGame()
        {
            if (_quitReason == QuitType.DataConflict)
            {
                PlayFabManager.Store.SaveAndRestart();
            }
            else if (_quitReason == QuitType.Maintenance || _quitReason == QuitType.MultiLogin)
            {
                PlayFabManager.Store.Exit();
            }
            else if (_quitReason == QuitType.LoginNetwork)
            {
                Destroy(gameObject);
            }
            else if (_quitReason == QuitType.Network)
            {
                if (_toDo != null)
                {
                    PlayFabManager.Store.DoWithTime(_toDo);
                }
                Manager.UI.CloseSingleUI(this);
            }
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}