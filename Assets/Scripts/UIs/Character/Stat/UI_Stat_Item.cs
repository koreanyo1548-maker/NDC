using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.Utils;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;
using Util = Utils.Util;

namespace UIs.Character.Stat
{
    public class UI_Stat_Item: UI_Base, ILanguageSet
    {
        private EventsManager _levelEventsManager;
        private EventsManager _badgeEventsManager;
        private EventsManager _goldEventManager;

        private StatController.StatLevel _stat;

        private UI_EventHandler _longClickBtn;


        
        enum Texts
        {
            T_Level,
            T_Stat,
            T_Cost,
            T_LevelUp
        }

        enum GameObjects
        {
            IMG_Badge
        }

        enum Images
        {
            B_LevelUp,
            IMG_Stat
        }

        enum Transforms
        {
            EffectPosition
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Transform>(typeof(Transforms));
            Bind<Image>(typeof(Images));
            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();

            return true;
        }

        public void SetInfo(DbGoldStat stat)
        {
            if (!_isInit) Init();
            _stat = StatController.I.Get(stat.Id);
            var statMeta = DbStat.Get(stat.Id);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = LocalString.Get(statMeta.StaticNameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel").text = string.Format(LocalString.Get(210039), DbGoldStat.Get(statMeta.Id).GetMaxLevel());
            Get<Image>((int)Images.IMG_Stat).sprite = Manager.Resource.Load<Sprite>("Stat_icon_" + stat.Id);
            Get<Image>((int)Images.B_LevelUp).gameObject.BindEvent(Condition, OnLevelUpButtonClicked, UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Get<Image>((int) Images.B_LevelUp).GetComponent<UI_EventHandler>();

            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(false);
            
            Util.FindChild(gameObject, "Lock", true).GetOrAddComponent<UI_StatLocked>().Set(stat, () =>
            {
                _levelEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenLevelChanged,
                    updatedField = new[] {_stat.Level, SettingController.data.StatUpCount}
                });
            
                _badgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenBadgeChanged,
                    updatedField = new[] {BadgeController.data.Stats}
                });
            
                _goldEventManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenLevelChanged,
                    updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Gold)}
                });
                WhenBadgeChanged();
            });

            WhenLevelChanged();
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenLevelChanged()
        {
            var isMaxLevelUp = SettingController.data.StatUpCount.Value == -1;
            var canLevelUp = _stat.CanLevelUp(SettingController.data.StatUpCount.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = isMaxLevelUp ?
                string.Format(LocalString.Get(210041), _stat.Level.Value) + "<color=#F9EF66> >> " + string.Format(LocalString.Get(210041), _stat.Level.Value + canLevelUp)
                :string.Format(LocalString.Get(210041), _stat.Level.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Cost).text = Define.AddUnit(_stat.GetNeedGoldForLevelUp(canLevelUp), 3, 2);
            var nextValue = _stat.GetStatValueMultiplied(_stat.Level.Value + canLevelUp);
            Get<TextMeshProUGUI>((int) Texts.T_Stat).text = (_stat.value * _stat.multiply).ToString(_stat.form) + (canLevelUp == 0 ? string.Empty : "<color=#F8EF66>  >>  " + nextValue.ToString(_stat.form) + "</color>");
            SetLevelUpButtons(Condition());
        }

        private void WhenBadgeChanged()
        {
            var can = Condition();
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(BadgeController.data.Stats.Value[DbGoldStat.Get(_stat.StatType).Index].Value);
            SetLevelUpButtons(can);
        }

        private void SetLevelUpButtons(bool can)
        {
            Get<Image>((int)Images.B_LevelUp).material = Define.GetUIMaterial(!can);
            Get<TextMeshProUGUI>((int) Texts.T_Cost).color = can ? Color.white : Define.ColorFF454A;
            Get<TextMeshProUGUI>((int) Texts.T_LevelUp).color = can ? Color.white : Define.ColorCFCFCF;
        }

        private void OnLevelUpButtonClicked(PointerEventData eventData)
        {
            var canLevelUp = _stat.CanLevelUp(SettingController.data.StatUpCount.Value);
            if (canLevelUp == 0) return;
            if (CurrencyController.I.TryUse(CurrencyType.Gold, _stat.GetNeedGoldForLevelUp(canLevelUp)))
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                _stat.LevelUp(canLevelUp);
                var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
                levelUp.GetComponent<ParticleSystem>().Play();
                levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
            }
            if (!Condition()) _longClickBtn.StopLongClick();
        }

        private bool Condition()
        {
            return _stat.CanLevelUp(SettingController.data.StatUpCount.Value, true) > 0;
        }
        
        private void OnDisable()
        {
            _levelEventsManager?.Dispose();
            _badgeEventsManager?.Dispose();
            _goldEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _levelEventsManager?.Reconnect();
            _badgeEventsManager?.Reconnect();
            _goldEventManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = LocalString.Get(_stat.statMeta.StaticNameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel").text = string.Format(LocalString.Get(210039), DbGoldStat.Get(_stat.StatType).GetMaxLevel());
        }
    }
}