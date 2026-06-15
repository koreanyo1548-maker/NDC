using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Character.Stat;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;

namespace UIs.Character.Level
{
    public class UI_LevelPoint_Item: UI_Base, ILanguageSet
    {
        private EventsManager _pointEventsManager;
        private EventsManager _badgeEventsManager;

        private LevelPointController.LevelPointLevel _levelPoint;

        private UI_EventHandler _longClickBtn;

        enum Texts
        {
            T_Level,
            T_Stat
        }

        enum Images
        {
            B_LevelUp,
            B_Reset,
            IMG_Stat
        }

        enum GameObjects
        {
            IMG_Badge
        }
        
        enum Transforms
        {
            EffectPosition
        }
        public override bool Init()
        {
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Transform>(typeof(Transforms));
            Bind<Image>(typeof(Images));
            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            
            return true;
        }

        public void SetInfo(DbLevelPoint levelPoint)
        {
            if (!_isInit) Init();
            _levelPoint = LevelPointController.I.Get(levelPoint);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = LocalString.Get(DbStat.Get(levelPoint.Id).StaticNameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel").text = string.Format(LocalString.Get(210039), levelPoint.MaxLevel);
            Util.FindChild(gameObject, "B_LevelUp").BindEvent(LevelUpCondition, OnLevelUpButtonClicked, UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Util.FindChild(gameObject, "B_LevelUp").GetComponent<UI_EventHandler>();
            Util.FindChild(gameObject, "B_Reset").BindEvent(ResetCondition, OnResetButtonClicked, UIEffectType.Bounce);
            Get<Image>((int)Images.IMG_Stat).sprite = Manager.Resource.Load<Sprite>("Stat_icon_" + levelPoint.Id);
            
            Util.FindChild(gameObject, "Lock", true).GetOrAddComponent<UI_StatLocked>().Set(levelPoint, () =>
            {
                _pointEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenLevelChanged,
                    updatedField = new[] {_levelPoint.Level, SettingController.data.LevelUpCount}
                });
                
                _badgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenBadgeChanged,
                    updatedField = new[] {BadgeController.data.LevelPoint}
                });
                WhenBadgeChanged();
            });
            WhenLevelChanged();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenLevelChanged()
        {
            var isMaxLevelUp = SettingController.data.LevelUpCount.Value == -1;
            var canLevelUp = _levelPoint.CanLevelUp(SettingController.data.LevelUpCount.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = isMaxLevelUp ?
                string.Format(LocalString.Get(210041), _levelPoint.Level.Value) + "<color=#F9EF66> >> " + string.Format(LocalString.Get(210041), _levelPoint.Level.Value + canLevelUp)
                :string.Format(LocalString.Get(210041), _levelPoint.Level.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Stat).text = (_levelPoint.value * _levelPoint.multiply).ToString(_levelPoint.form) + (canLevelUp == 0 ? string.Empty 
                : "<color=#F8EF66>  >>  " +  ((_levelPoint.Level.Value + canLevelUp) * _levelPoint.meta.Value * _levelPoint.multiply).ToString(_levelPoint.form) + "</color>");
            Get<Image>((int) Images.B_LevelUp).material = Define.GetUIMaterial(!LevelUpCondition());
            Get<Image>((int) Images.B_Reset).material = Define.GetUIMaterial(!ResetCondition());
        }

        private void OnLevelUpButtonClicked(PointerEventData eventData)
        {
            var canLevelUp = _levelPoint.CanLevelUp(SettingController.data.LevelUpCount.Value);
            if (canLevelUp == 0) return;
            if (CurrencyController.I.TryUse(CurrencyType.LevelPoint, canLevelUp))
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                _levelPoint.LevelUp(canLevelUp);   
                var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
                levelUp.GetComponent<ParticleSystem>().Play();
                levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
            }
            
            if (!LevelUpCondition()) _longClickBtn.StopLongClick();
        }
        
        private void WhenBadgeChanged()
        {
            var condition = LevelUpCondition();
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(condition);
            Get<Image>((int) Images.B_LevelUp).material = Define.GetUIMaterial(!condition);
        }

        private void OnResetButtonClicked(PointerEventData eventData)
        {
            var popup = Manager.UI.ShowPopupUI<UI_ResetLevelPoint>();
            popup.Set(_levelPoint);
        }

        private bool LevelUpCondition()
        {
            return _levelPoint.CanLevelUp(SettingController.data.LevelUpCount.Value, true) > 0;
        }

        private bool ResetCondition()
        {
            return _levelPoint.CanReset();
        }
        
        private void OnDisable()
        {
            _pointEventsManager?.Dispose();
            _badgeEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _pointEventsManager?.Reconnect();
            _badgeEventsManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = LocalString.Get(DbStat.Get(_levelPoint.meta.Id).StaticNameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel").text = string.Format(LocalString.Get(210039), _levelPoint.meta.MaxLevel);
        }
    }
}