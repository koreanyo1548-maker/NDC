using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbForbiddenInfo;
using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.FieldMain;
using UIs.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_Nickname : UI_Popup
    {
        private TMP_InputField _input;
        private TextMeshProUGUI _costText;
        private TextMeshProUGUI _infoText;
        private Image _confirmBtn;
        private Image _changeBtn;

        private EventsManager _diaHandler;

        private bool _isTutorial;
        

        public override bool Init()
        {
            if (!base.Init()) return false;
            _input = Util.FindChild<TMP_InputField>(gameObject, "InputField_Nickname", true);
            _input.onValueChanged.AddListener(_ => CheckCondition());
            _costText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Cost", true);
            _costText.text = DbCost.Get(CostType.NicknameResetDia).Cost.ToString();
            _infoText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Info", true);
            _infoText.text = string.Empty;
            _confirmBtn = Util.FindChild<Image>(gameObject, "B_Confirm", true);
            _confirmBtn.gameObject.BindEvent(() => NicknameCondition() == 0, _ => SetNickname(), UIEffectType.Bounce);
            _changeBtn = Util.FindChild<Image>(gameObject, "B_Change", true);
            _changeBtn.gameObject.BindEvent(() => NicknameCondition() == 0, _ => SetNickname(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(() => !_isTutorial, _ => ClosePopupUI(), UIEffectType.None, false);

            _diaHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = CheckCondition,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });
            return true;
        }


        public void Set(bool isTutorial)
        {
            if (!_isInit) Init();
            
            _isTutorial = isTutorial;
            if (isTutorial) Manager.UI.CanClosePopup = false;
            _confirmBtn.gameObject.SetActive(_isTutorial);
            _changeBtn.gameObject.SetActive(!_isTutorial);
            _input.text = string.Empty;

            CheckCondition();
        }

        private void CheckCondition()
        {
            var condition = NicknameCondition();
            var cantChange = condition != 0;
            if (_isTutorial) _confirmBtn.material = Define.GetUIMaterial(cantChange);
            else _changeBtn.material = Define.GetUIMaterial(cantChange);
            _infoText.text = cantChange && _input.text.Length > 0 ? LocalString.Get(condition) : string.Empty;
        }
        
        private int NicknameCondition()
        {
            if (!_isTutorial)
            {
                if (CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value < DbCost.Get(CostType.NicknameResetDia).Cost)
                {
                    _costText.color = Define.ColorFF454A;
                    return -1;
                }
                
                _costText.color = Color.white;
            }
            
            var str = _input.text;
            if (str.Length < 3) return 210336;
            if (str.Length > 10) return 210337;
            for (var idx = 0; idx < str.Length; ++idx)
            {
                if (str[idx] >= '가' && str[idx] <= '힝') continue;
                if (str[idx] >= 'a' && str[idx] <= 'z') continue;
                if (str[idx] >= 'A' && str[idx] <= 'Z') continue;
                if (str[idx] >= '0' && str[idx] <= '9') continue;
                if (str[idx] >= '一' && str[idx] <= '鿕') continue;
                if (str[idx] >= 'ぁ' && str[idx] <= 'ゔ') continue;
                if (str[idx] >= 'ァ' && str[idx] <= 'ヺ') continue;
                if (str[idx] == 'ー') continue;
                return 210338;
            }

            return 0;
        }

        private void SetNickname()
        {
            if (!_isTutorial)
            {
                if (_input.text.Equals(SettingController.Nickname))
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200039);
                }
                else
                {
                    Change(() =>
                    {
                        var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                        CurrencyController.I.TryUse(CurrencyType.Dia, DbCost.Get(CostType.NicknameResetDia).Cost);
                        CurrencyController.I.SetDiaLog("닉네임 재설정", -DbCost.Get(CostType.NicknameResetDia).Cost, prev);
                    });
                }
            }
            else
            {
                Change(() => {});
            }

            void Change(Action toDo)
            {
                if (IsForbidden())
                {
                    _infoText.text = LocalString.Get(200059);
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200059);
                    return;
                }

                PlayFabManager.TrySetDisplayName(_input.text, (displayName) =>
                {
                    SettingController.Nickname = displayName;
                    Manager.UI.CanClosePopup = true;
                    Manager.UI.GetSceneUI<UI_MainTop>().SetNickname();
                    ClosePopupUI();
                    toDo();
                });
            }
            
        }
        
        private bool IsForbidden()
        {
            var check = _input.text.ToLower();
            var count = DbForbidden.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (Regex.IsMatch(check, @$"{DbForbidden.Get(idx).Word}"))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            if (_isTutorial)
            {
                Manager.Field.SpawnGame();
            }
            else Manager.UI.GetPopupUI<UI_PlayerInfo>().SetNickname();
        }
        
        private void OnDisable()
        {
            _diaHandler.Dispose();
        }

        private void OnEnable()
        {
            _diaHandler?.Reconnect();
            if (_isInit)
            {
                _infoText.text = string.Empty;
            }
        }
    }
}