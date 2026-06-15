using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.Entrance
{
    public abstract class UI_DungeonEntrance: UI_Popup
    {
        public FieldType CurDungeon => _fieldType;
        
        protected FieldType _fieldType = FieldType.SkillGrowth;
        protected DbDungeonMeta _dungeonMeta;
        protected UIField<int> _preset = new(0);

        private EventsManager _ticketEventHandler;

        private GameObject _adTicketObj;
        protected enum Texts
        {
            T_TicketCount1,
            T_TicketCount2,
            T_Title,
            T_MaxLevel,
            T_Reward,
            T_Power,
            T_RewardCount,
            T_BookHp,
            T_ResetTime,
            T_PrevRecord
        }

        protected enum Images
        {
            IMG_Skill1 = 0,
            IMG_Skill2 = 1,
            IMG_Skill3 = 2,
            IMG_Skill4 = 3,
            B_Clear,
            B_Enter,
            IMG_Ticket1,
            IMG_Ticket2,
            IMG_Reward
        }

        
        public UIField<int> GetPresetIdx()
        {
            return _preset;
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));

            _adTicketObj = Util.FindChild(gameObject, "AdTicket", true);
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Image>((int)Images.B_Enter).gameObject.BindEvent(Condition, TryEnterStage);
            var clearBtn = Get<Image>((int)Images.B_Clear);
            if (clearBtn != null) clearBtn.gameObject.BindEvent(Condition, TryClearStage, UIEffectType.Bounce);
            
            for (var idx = 1; idx <= 5; ++idx)
            {
                var curIdx = idx -1;
                Util.FindChild(gameObject,"B_SkillPreset" + idx, true).GetOrAddComponent<UI_DungeonEntranceSkillPreset_Item>().Set(curIdx, this);
            }
            
            _ticketEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenTicketChanged,
                updatedField = null
            });

            return true;
        }
        
        public virtual void SetFieldType(FieldType fieldType, bool isForceRefresh = false)
        {
            if (!_isInit) Init();
            _fieldType = fieldType;
            
            _dungeonMeta = DbDungeonMeta.Get(_fieldType);
            _ticketEventHandler.Set(WhenTicketChanged, new []
            {
                CurrencyController.I.GetTicketModel(_dungeonMeta.Use), CurrencyController.I.GetTicketModel(_dungeonMeta.AdUse)
            });
            
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(_dungeonMeta.NameId);
            
            _preset.Value = SettingController.I.GetSkillPresetFor(_fieldType);
            SetSkillPreset();
            WhenTicketChanged();
        }
        
        private bool Condition()
        {
            if (_dungeonMeta.Use == CurrencyType.None) return true;
            return CurrencyController.I.GetTicketModel(_dungeonMeta.Use).Value > 0 || CurrencyController.I.GetTicketModel(_dungeonMeta.AdUse).Value > 0;
        }

        private void TryEnterStage(PointerEventData eventData)
        {
            if (_dungeonMeta.Use == CurrencyType.None) EnterStage();
            else if (CurrencyController.I.GetTicketModel(_dungeonMeta.Use).Value > 0)
            {
                PlayController.I.useAdTicket = false;
                EnterStage();
            }
            else // if (CurrencyController.I.GetTicketModel(_dungeonMeta.AdUse).Value > 0)
            {
                Manager.Ad.ShowAd(eAdType.Dungeon, () =>
                {
                    CurrencyController.I.TryUse(_dungeonMeta.AdUse, 1);
                    CurrencyController.I.GetReward(_dungeonMeta.Use, 1);
                    PlayController.I.useAdTicket = true;
                    Timing.CallDelayed(0.1f, EnterStage);
                });
            }
        }

        private void TryClearStage(PointerEventData eventData)
        {
            if (CurrencyController.I.GetTicketModel(_dungeonMeta.Use).Value > 0)
            {
                ClearStage();
            }
            else Manager.Ad.ShowAd(eAdType.Dungeon, () =>
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    LevelController.I.ClearDungeon(_dungeonMeta, _dungeonMeta.AdUse, 1, GetStage());
                });
            });
        }

        protected abstract int GetStage();

        protected abstract void EnterStage();

        protected abstract void ClearStage();
        
        private void WhenTicketChanged()
        {
            var ticketCount = CurrencyController.I.GetTicketModel(_dungeonMeta.Use).Value;
            var canUseTicket = ticketCount > 0;
            if (!canUseTicket) ticketCount = CurrencyController.I.GetTicketModel(_dungeonMeta.AdUse).Value;
            var currency = DbCurrency.Get(canUseTicket ? _dungeonMeta.Use : _dungeonMeta.AdUse);
            
            _adTicketObj.SetActive(!canUseTicket);
            Get<Image>((int)Images.IMG_Ticket1).sprite =
            Get<Image>((int)Images.IMG_Ticket2).sprite = Manager.Resource.Load<Sprite>(currency.Resource);
            if (!canUseTicket) Get<TextMeshProUGUI>((int)Texts.T_TicketCount1).text = ticketCount + "/" + currency.DailyCharge;
            Get<TextMeshProUGUI>((int) Texts.T_TicketCount2).text = ticketCount + "/" + currency.DailyCharge;
            Get<Image>((int)Images.B_Clear).material = Define.GetUIMaterial(!CanClear());
            Get<Image>((int)Images.B_Enter).material = Define.GetUIMaterial(ticketCount == 0);
        }

        protected virtual bool CanClear()
        {
            return CurrencyController.I.GetTicketModel(_dungeonMeta.Use).Value > 0 || CurrencyController.I.GetTicketModel(_dungeonMeta.AdUse).Value > 0;
        }
        
        public void ChangeSkillPreset(int presetIdx)
        {
            if (presetIdx < 0 || presetIdx > 4) return;
            
            _preset.Value = presetIdx;
            SettingController.I.ChangeSkillPresetFor(_fieldType, _preset.Value);
            SetSkillPreset();
        }

        protected void SetSkillPreset()
        {
            var skills = EquipController.data.Skills[_preset.Value];
            for (var idx = 0; idx < 4; ++idx)
            {
                var skill = skills.Value[idx].Value;
                Get<Image>(idx).sprite = Manager.Resource.Load<Sprite>(skill == -1 ? Define.EmptySprite : DbSkill.Get(skill).Resource);
            }
        }

        
        public override bool NeedRaycast()
        {
            return true;
        }

        private void OnEnable()
        {
            _ticketEventHandler?.Reconnect();
        }

        private void OnDisable()
        {
            _ticketEventHandler?.Dispose();
        }
        public override void WhenPopupClosed()
        {
            
        }
    }
}