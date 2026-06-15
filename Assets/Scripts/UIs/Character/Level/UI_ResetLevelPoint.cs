using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Level
{
    public class UI_ResetLevelPoint: UI_Popup
    {
        private EventsManager _resetEventsManager;
        private LevelPointController.LevelPointLevel _levelPoint;

        enum Images
        {
            B_Reset
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Image>((int)Images.B_Reset).gameObject.BindEvent(ResetCondition, OnResetButtonClicked, UIEffectType.Bounce);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Cost", true).text =
                DbCost.Get(CostType.LevelPointResetDia).Cost.ToString("N0");
                
            
            _resetEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenDiaChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });

            return true;
        }

        public void Set(LevelPointController.LevelPointLevel levelPoint)
        {
            if (!_isInit) Init();
            _levelPoint = levelPoint;
            WhenDiaChanged();
        }

        private void OnResetButtonClicked(PointerEventData eventData)
        {
            if (!_levelPoint.CanReset()) return;
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            if (CurrencyController.I.TryUse(CurrencyType.Dia, DbCost.Get(CostType.LevelPointResetDia).Cost))
            {
                CurrencyController.I.SetDiaLog("레벨 포인트 초기화", -DbCost.Get(CostType.LevelPointResetDia).Cost, prev);
                var count = _levelPoint.Reset();
                CurrencyController.I.ReturnLevelPoint(count);
                ClosePopupUI();
                PlayFabManager.Store.ForceSave();
            }
        }
        
        private void WhenDiaChanged()
        {
            Get<Image>((int)Images.B_Reset).material = Define.GetUIMaterial(!ResetCondition());
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
        private bool ResetCondition()
        {
            return _levelPoint.CanReset() &&
                   CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value >= DbCost.Get(CostType.LevelPointResetDia).Cost;
        }
        private void OnDisable()
        {
            _resetEventsManager.Dispose();
        }

        private void OnEnable()
        {
            _resetEventsManager?.Reconnect();
        }
    }
}