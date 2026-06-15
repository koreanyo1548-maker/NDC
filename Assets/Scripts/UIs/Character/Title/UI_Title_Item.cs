using Controller.Infos;
using Data.DbRecord;
using Data.DbUser.Progress;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Title
{
    public class UI_Title_Item: UI_Base, ILanguageSet
    {
        private EventsManager _levelEventsManager;

        private DbTitle _title;
        private DbUserTitle _info;

        private UI_EventHandler _longClickBtn;
        
        enum Texts
        {
            T_Cost,
            T_MaxLevel,
            T_LevelName,
            T_Stat,
            T_Goal,
            T_Equip,
            T_LevelUpGoal
        }

        enum GameObjects
        {
            B_Unlock,
            B_Equip,
            IMG_Equip,
            IMG_Get,
            IMG_Badge,
            Progress
        }

        enum Images
        {
            IMG_ProgressBar,
            B_LevelUp,
            B_Equip
        }


        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            return true;
        }

        public void SetInfo(DbTitle title)
        {
            if (!_isInit) Init();

            _title = title;
            _info = DbUserTitle.Get(title.Id);
            
            Get<TextMeshProUGUI>((int)Texts.T_MaxLevel).text = string.Format(LocalString.Get(210039), _title.MaxLevel);
            Get<Image>((int)Images.B_LevelUp).gameObject.BindEvent(LevelUpCondition, LevelUp, UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Get<Image>((int) Images.B_LevelUp).GetComponent<UI_EventHandler>();

            Get<GameObject>((int)GameObjects.B_Equip).BindEvent(Functions.TrueCondition, EquipTitle, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Unlock).BindEvent(GetCondition, GetTitle);
            
            _levelEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetStatus,
                updatedField = new[] {_info.Level, EquipController.data.Title, _info.DoCount}
            });

            SetStatus();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }


        private void EquipTitle(PointerEventData eventData)
        {
            EquipController.I.Equip(_info);
        }
        
        private void LevelUp(PointerEventData eventData)
        {
            Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
            _info.LevelUp();
            
            if (!LevelUpCondition()) _longClickBtn.StopLongClick();
        }

        private void GetTitle(PointerEventData eventData)
        {
            _info.GetTitle();
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(string.Format(LocalString.Get(200012), _title.GetNameWithColor()));
            Reorder(_info.GetOrder());
        }

        private bool GetCondition()
        {
            return _info.CanHave;
        }
        
        private void SetStatus()
        { 
            var have = _info.Level.Value > 0;
            var equip = EquipController.I.IsEquipped(_info);
            Get<GameObject>((int)GameObjects.B_Unlock).SetActive(!have);
            Get<GameObject>((int)GameObjects.IMG_Equip).SetActive(equip);
            Get<GameObject>((int)GameObjects.B_Unlock).SetActive(!have);
            Get<Image>((int) Images.B_Equip).sprite =
                Manager.Resource.Load<Sprite>(equip ? "UI_DefaultButton_round3" : "UI_DefaultButton_round");
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(equip ? 210049 : 210048);

            var canLevelUp = LevelUpCondition();
            Get<Image>((int)Images.B_LevelUp).material = Define.GetUIMaterial(!canLevelUp);
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(canLevelUp);

            if (!have)
            {
                var clear = _info.CanHave;
                var hide = !clear && _title.IsSecret;
                Get<GameObject>((int)GameObjects.IMG_Get).SetActive(clear);
                Get<Image>((int) Images.IMG_ProgressBar).fillAmount = hide ? 0 : 1f * _info.DoCount.Value / _title.Goal[0];
                Get<TextMeshProUGUI>((int) Texts.T_Goal).text = hide ? LocalString.Get(210050) 
                : string.Format(LocalString.Get(210051), LocalString.Get(_title.GoalId), _info.DoCount.Value.ToString("N0"), _title.Goal[0].ToString("N0"));
                Get<GameObject>((int)GameObjects.Progress).SetActive(!clear);
                
                if (clear) Reorder(_info.GetOrder());
            }

            var level = _info.Level.Value;
            if (level == 0) level = 1;
            var isMaxLevel = level == _title.MaxLevel;
            Get<TextMeshProUGUI>((int) Texts.T_Cost).text =
                isMaxLevel ? LocalString.Get(210047) 
                    : $"{Define.AddUnit(_info.DoCount.Value, 3, 1)}/{Define.AddUnit(_title.Goal[level], 3, 1)}";
            Get<TextMeshProUGUI>((int) Texts.T_Stat).text = isMaxLevel ? StringMaker.GetFinalString(_title.Option, _title.Value * level)
                : StringMaker.GetIncreaseString(_title.Option, _title.Value * level, _title.Value * (level+1));
            Get<TextMeshProUGUI>((int) Texts.T_LevelName).text =
                $"{_title.GetNameWithColor()} {(have ? string.Format(LocalString.Get(210041), level) : string.Empty)}";
            Get<TextMeshProUGUI>((int) Texts.T_LevelUpGoal).text = isMaxLevel ? LocalString.Get(210047) : string.Format(LocalString.Get(210248),
                LocalString.Get(_title.GoalId), Define.AddUnit(_title.Goal[level], 3, 2));
        }
        
        private bool LevelUpCondition()
        {
            return _info.CanLevelUp();
        }
        
        private void OnDisable()
        {
            _levelEventsManager.Dispose();
        }

        private void OnEnable()
        {
            _levelEventsManager?.Reconnect();
            // if (_isInit) Reorder(_info.GetOrder());
        }

        public void Reorder(int siblingIdx)
        {
            transform.SetSiblingIndex(siblingIdx);
        }

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int)Texts.T_MaxLevel).text = string.Format(LocalString.Get(210039), _title.MaxLevel);
        }
    }
}