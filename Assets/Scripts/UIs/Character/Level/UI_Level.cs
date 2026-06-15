using Coffee.UIEffects;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;

namespace UIs.Character.Level
{
    public class UI_Level: UI_Base
    {
        private EventsManager _expEventsManager;
        private EventsManager _levelPointEventsManager;
        private EventsManager _badgeEventsManager;

        private UI_EventHandler _longClickBtn;

        private Vector3 _expScale = new(1, 1, 1);

        enum Images
        {
            B_LevelUp
        }
        enum Texts
        {
            T_Level,
            T_ExpPercentage,
            T_LevelPoint
        }

        enum Transforms
        {
            IMG_Exp,
            EffectPosition
        }

        enum GameObjects
        {
            IMG_Badge,
            IMG_Exp100Icon
        }

        enum UIShinies
        {
            IMG_Exp 
        }
        
        private void Start()
        {
            Init();
        }
        
        public override bool Init()
        {
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Transform>(typeof(Transforms));
            Bind<GameObject>(typeof(GameObjects));
            Bind<UIShiny>(typeof(UIShinies));

            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            
            Get<Image>((int)Images.B_LevelUp).gameObject.BindEvent(Condition, OnLevelUpButtonClicked, UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Get<Image>((int) Images.B_LevelUp).GetComponent<UI_EventHandler>();
            _expEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenExpChanged,
                updatedField = new[] {LevelController.data.Exp}
            });

            _levelPointEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenLevelPointChanged,
                updatedField = new[] {CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint)}
            });
            
            _badgeEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenBadgeChanged,
                updatedField = new[] {BadgeController.data.LevelUp}
            });

            WhenExpChanged();
            WhenLevelPointChanged();
            WhenBadgeChanged();

            return true;
        }

        private void WhenExpChanged()
        {
            var percentage = Mathf.Min(1,LevelController.I.ExpPerNeed());
            _expScale.x = percentage;
            
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), LevelController.data.Level.Value);
            Get<TextMeshProUGUI>((int) Texts.T_ExpPercentage).text = percentage.ToString("P0");
            Get<Transform>((int) Transforms.IMG_Exp).localScale = _expScale;
        }

        private void WhenLevelPointChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_LevelPoint).text = string.Format(LocalString.Get(210042), CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).Value);
        }
        
        private void WhenBadgeChanged()
        {
            var can = Condition();
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(can);
            Get<GameObject>((int)GameObjects.IMG_Exp100Icon).SetActive(can);
            Get<Image>((int) Images.B_LevelUp).material = Define.GetUIMaterial(!can);
        }

        private void OnLevelUpButtonClicked(PointerEventData eventData)
        {
            Manager.Sound.PlaySFX(SFXType.UI_LevelUp);
            LevelController.I.LevelUp();  
            var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
            levelUp.GetComponent<ParticleSystem>().Play();
            levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
            Get<UIShiny>((int)UIShinies.IMG_Exp).Play();
            
            if (!Condition()) _longClickBtn.StopLongClick();
        }

        private bool Condition()
        {
            return BadgeController.data.LevelUp.Value;
        }
        
        private void OnDisable()
        {
            _expEventsManager?.Dispose();
            _levelPointEventsManager?.Dispose();
            _badgeEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _expEventsManager?.Reconnect();
            _levelPointEventsManager?.Reconnect();
            _badgeEventsManager?.Reconnect();
        }
    }
}