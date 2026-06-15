using System;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbPetInfo;

using Data.Stores;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_Bible: UI_Base
    {   
        private EventsManager _petGrowthChangeEventManager;
        private EventsManager _levelChangedEventManager;
        private EventsManager _equipChangedEventManager;
        private UI_EventHandler _longClickBtn;

        private DbBibleLevel _next;
        
        enum Texts
        {
            T_Level,
            T_Attack,
            T_Hp,
            T_GrowthCost,
            T_HpPlus
        }

        enum Images
        {
            B_Growth
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            
            Get<Image>((int)Images.B_Growth).gameObject.BindEvent(GrowthCondition, _ => Growth(), UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Get<Image>((int) Images.B_Growth).GetComponent<UI_EventHandler>();

            _petGrowthChangeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenStoneChanged,
                updatedField = new[] {CurrencyController.I.GetStoneModel(CurrencyType.PetGrowthStone)}
            });

            _levelChangedEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenLevelChanged,
                updatedField = new[] {LevelController.data.BibleLevel}
            });

            _equipChangedEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = new[] {EquipController.data.Pets}
            });

            WhenEquipChanged();
            WhenLevelChanged();
            return true;
        }

        private bool GrowthCondition()
        {
            if (_next == null) return false;
            return CurrencyController.I.GetStoneModel(CurrencyType.PetGrowthStone).Value >= _next.SpendCount;
        }

        private void WhenLevelChanged()
        {
            var level = LevelController.data.BibleLevel.Value;
            _next = DbBibleLevel.Get(level + 1);
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), level);

            var current = DbBibleLevel.Get(level);
            var haveNext = _next != null;
            Get<TextMeshProUGUI>((int) Texts.T_Attack).text = current.Attack + (haveNext ? "<color=#FFD53B> >> " + _next.Attack + "</color>" : string.Empty);
            Get<TextMeshProUGUI>((int) Texts.T_Hp).text = current.Hp + (haveNext ? "<color=#FFD53B> >> " + _next.Hp + "</color>" : string.Empty);

            WhenStoneChanged();
        }

        private void WhenEquipChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_HpPlus).text = EquipController.I.GetPetHpBonus() + "%";
        }
        
        private void WhenStoneChanged()
        {
            if (_next == null)
            {
                Get<TextMeshProUGUI>((int) Texts.T_GrowthCost).text = LocalString.Get(210239);
                Get<Image>((int) Images.B_Growth).material = Define.GetUIMaterial(true);
            }
            else
            {
                var have = CurrencyController.I.GetStoneModel(CurrencyType.PetGrowthStone).Value;
                var need = _next.SpendCount;
                var canLevelUp = have >= need;
                Get<TextMeshProUGUI>((int) Texts.T_GrowthCost).text =
                    canLevelUp ? $"{Define.AddUnit(have, 3, 2)}/{Define.AddUnit(need, 3, 2)}" :
                    $"<color=#FF4940>{Define.AddUnit(have, 3, 2)}</color>/{Define.AddUnit(need, 3, 2)}";
                Get<Image>((int) Images.B_Growth).material = Define.GetUIMaterial(have < need);
            }
        }

        private void Growth()
        {
            if (CurrencyController.I.TryUse(CurrencyType.PetGrowthStone, _next.SpendCount))
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                LevelController.I.BibleLevelUp();
            }
            
            if (!GrowthCondition()) _longClickBtn.StopLongClick();
        }
        
        private void OnDisable()
        {
            _petGrowthChangeEventManager.Dispose();
            _levelChangedEventManager.Dispose();
            _equipChangedEventManager.Dispose();
        }

        private void OnEnable()
        {
            _petGrowthChangeEventManager?.Reconnect();
            _levelChangedEventManager?.Reconnect();
            _equipChangedEventManager?.Reconnect();
        }
    }
}