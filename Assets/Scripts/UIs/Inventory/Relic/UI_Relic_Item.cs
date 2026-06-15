using Data;
using Data.DbDefinition;
using Data.DbRelicInfo;
using Data.DbUser.Equipment;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Relic
{
    public class UI_Relic_Item: UI_Base, ILanguageSet
    {
        private EventsManager _levelEventsManager;
        private EventsManager _countEventsManager;
        
        private DbRelic _relicMeta;
        private DbUserRelic _relic;
        
        private UI_EventHandler _longClickBtn;

        private bool _isLocked;
        
        enum Texts
        {
            T_Stat,
            T_LevelName,
            T_Cost,
            T_SellCost,
            T_Probability
        }

        enum Images
        {
            B_LevelUp
        }
        
        enum GameObjects
        {
            B_LevelUp,
            B_Sell,
            Locked,
            IMG_LevelUpBadge
        }
        
        enum Transforms
        {
            EffectPosition
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Transform>(typeof(Transforms));
            
            return true;
        }

        public void SetInfo(DbRelic relic)
        {
            if (!_isInit) Init();

            _relicMeta = relic;
            _relic = DbUserRelic.Get(relic.Id);
            _isLocked = _relic.Level.Value == 0 && _relic.Count.Value == 0;
            Get<GameObject>((int)GameObjects.Locked).SetActive(_isLocked);
            
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel", true).text = string.Format(LocalString.Get(210039), DbRelicLevel.Count);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade", true).text = LocalString.Get(DbGrade.Get(_relicMeta.Grade).NameId);
            Util.FindChild<Image>(gameObject, "IMG_Relic", true).sprite = Manager.Resource.Load<Sprite>(_relicMeta.Resource);
            Util.FindChild<Image>(gameObject, "IMG_Grade", true).sprite = Manager.Resource.Load<Sprite>(_relicMeta.Grade.ToString());
            Get<GameObject>((int) GameObjects.B_Sell).SetActive(false);
            
            Get<GameObject>((int)GameObjects.B_LevelUp).BindEvent(CanLevelUp, _ => LevelUp(), UIEffectType.Bounce, true, UIEvent.LongClick); 
            _longClickBtn = Get<GameObject>((int) GameObjects.B_LevelUp).GetComponent<UI_EventHandler>();

            Get<GameObject>((int)GameObjects.B_Sell).BindEvent(Functions.TrueCondition, _ => Sell(), UIEffectType.Bounce);
            
            _levelEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenRelicLevelChanged,
                updatedField = new[] {_relic.Level}
            });
            
            _countEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenRelicCountChanged,
                updatedField = new[] {_relic.Count}
            });
            
            WhenRelicCountChanged();
            WhenRelicLevelChanged();
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenRelicCountChanged()
        {
            if (_isLocked) CheckLocked();
            
            if (IsMaxLevel())
            {
                var have = _relic.Count.Value > 0;
                Get<GameObject>((int) GameObjects.B_LevelUp).SetActive(!have);
                Get<GameObject>((int) GameObjects.B_Sell).SetActive(have);
                if (have) Get<TextMeshProUGUI>((int)Texts.T_SellCost).text = 
                    Define.AddUnit(_relic.Count.Value * DbCost.Get(CostType.RelicSellCost).Cost, 6, 0);
                Get<Image>((int)Images.B_LevelUp).material = Define.GetUIMaterial(true);
            }
            else
            {
                var count = _relic.Count.Value;
                Get<TextMeshProUGUI>((int)Texts.T_Cost).text = Define.AddUnit(count, 3, 2) + "/1";
                Get<Image>((int)Images.B_LevelUp).material = Define.GetUIMaterial(count == 0);
                Get<GameObject>((int) GameObjects.IMG_LevelUpBadge).SetActive(count > 0);
            }
        }

        private void WhenRelicLevelChanged()
        {
            if (_isLocked) CheckLocked();

            var level = _relic.Level.Value;
            var isMaxLevel = IsMaxLevel();
            if (isMaxLevel)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Cost).text = LocalString.Get(210047);
                Get<GameObject>((int) GameObjects.IMG_LevelUpBadge).SetActive(false);
            }
            
            Get<TextMeshProUGUI>((int) Texts.T_Stat).text = isMaxLevel
                ? StringMaker.GetFinalString(_relicMeta.StatType, DbRelicLevel.Get(level).GetStat(_relic.Id))
                : StringMaker.GetIncreaseString(_relicMeta.StatType, level == 0 ? 0 : DbRelicLevel.Get(level).GetStat(_relic.Id),
                    DbRelicLevel.Get(level + 1).GetStat(_relic.Id));
            Get<TextMeshProUGUI>((int) Texts.T_Probability).text = isMaxLevel
                ? LocalString.Get(210239)
                : string.Format(LocalString.Get(210386), DbRelicGrowthProbability.Get(level + 1).GetPr(_relic.Id) / 10);
            Get<TextMeshProUGUI>((int)Texts.T_LevelName).text = 
                string.Format(LocalString.Get(210041), level) + " " + LocalString.Get(_relicMeta.NameId);
        }

        private bool CanLevelUp()
        {
            return _relic.Count.Value > 0 && !IsMaxLevel();
        }

        private bool IsMaxLevel()
        {
            return _relic.Level.Value == DbRelicLevel.Count;
        }

        private void LevelUp()
        {
            var success = _relic.TryGrowth();
            if (success)
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
            }
            var levelUp = Manager.Resource.Instantiate(success ? "Particles/LevelUpEffect" : "Particles/RelicFailedEffect", 10).transform;
            levelUp.GetComponent<ParticleSystem>().Play();
            levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
            if (!CanLevelUp()) _longClickBtn.StopLongClick();
        }

        private void Sell()
        {
            _relic.Sell();
        }

        private void CheckLocked()
        {
            if (_relic.Level.Value > 0 || _relic.Count.Value > 0)
            {
                _isLocked = false;
                 Get<GameObject>((int)GameObjects.Locked).SetActive(false);
            }
        }
        private void OnDisable()
        {
            _levelEventsManager.Dispose();
            _countEventsManager.Dispose();
        }

        private void OnEnable()
        {
            _levelEventsManager?.Reconnect();
            _countEventsManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_MaxLevel", true).text = string.Format(LocalString.Get(210039), DbRelicLevel.Count);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade", true).text = LocalString.Get(DbGrade.Get(_relicMeta.Grade).NameId);
            WhenRelicLevelChanged();
        }
    }
}