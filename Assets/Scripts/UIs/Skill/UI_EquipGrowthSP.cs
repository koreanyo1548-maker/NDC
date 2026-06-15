using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;

using Data.DbUser.Equipment;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Skill
{
    public class UI_EquipGrowthSP : UI_Popup
    {
        private EventsManager _currencyEventsManager;
        private UI_EventHandler _longClickBtn;

        public UIField<bool> Check = new(false);
        
        enum Texts
        {
            T_Name,
            T_Level,
            T_Grade,
            T_EquipStat1,
            T_EquipStat2,
            T_GrowthCost,
            T_Growth
        }

        enum GameObjects
        {
            B_Prev,
            B_Next,
            GrowthCost,
            EquipStat2
        }

        enum Images
        {
            IMG_Grade,
            IMG_Equipment,
            IMG_GrowthIcon,
            B_Growth
        }

        enum Transforms
        {
            EffectPosition
        }

        public EquipmentType EquipmentType => _equipmentType;
        private EquipmentType _equipmentType;
        private IDbUserEquipment _equipment;
        
        private UI_Star _starUI;

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));
            
            Get<GameObject>((int)GameObjects.B_Prev).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_equipmentType, _equipment.PrevHave());
                Check.Value = !Check.Value;
            }, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_equipmentType, _equipment.NextHave());
                Check.Value = !Check.Value;
            }, UIEffectType.Bounce);
            Get<Image>((int)Images.B_Growth).gameObject.BindEvent(Functions.TrueCondition, Growth, UIEffectType.Bounce, false, UIEvent.LongClick);
            _longClickBtn = Get<Image>((int) Images.B_Growth).GetComponent<UI_EventHandler>();

            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
         

            return true;
        }

        public void Set(EquipmentType type, IDbUserEquipment equipment)
        {
            if (!_isInit) Init();

            _equipmentType = type;
            _equipment = equipment;
            
            var meta = equipment.GetMeta();

            Get<GameObject>((int)GameObjects.B_Prev).SetActive(equipment.PrevHave() != null);
            Get<GameObject>((int)GameObjects.B_Next).SetActive(equipment.NextHave() != null);
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(meta.GetNameId());
            _starUI.Set(equipment.GetAwakening());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(meta.GetGrade()).NameId);
            Get<Image>((int) Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(meta.GetResource());
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(meta.GetGrade().ToString());

            Get<GameObject>((int)GameObjects.EquipStat2).SetActive(type == EquipmentType.Pet);

            Get<Image>((int)Images.IMG_GrowthIcon).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(Define.EquipmentToGrowthStone(_equipmentType)).Resource);


            LevelInfoSet();
            WhenCurrencyChanged();
            
            if (_currencyEventsManager == null)
            {
                _currencyEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenCurrencyChanged,
                    updatedField = new[] { CurrencyController.I.GetStoneModel(_equipmentType)}
                });
            }
            else
            {
                _currencyEventsManager.Set(WhenCurrencyChanged, new[] {CurrencyController.I.GetStoneModel(_equipmentType)});
            }
        }

        private void LevelInfoSet()
        {
            var isMaxGrowth = _equipment.IsMaxGrowth(true);
            var growth = _equipment.GetGrowth();
            var meta = _equipment.GetMeta();
            
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), growth);

            if (meta.GetEquipmentType() == EquipmentType.Skill)
            {
                Get<TextMeshProUGUI>((int) Texts.T_EquipStat1).text = meta.GetOwnDescription(growth, !isMaxGrowth);
            }
            else if (meta.GetEquipmentType() == EquipmentType.Pet)
            {

                Get<TextMeshProUGUI>((int) Texts.T_EquipStat1).text = GetStatString(meta.GetEquipStatType(),
                    meta.GetEquipStat(), meta.GetEquipGrowthStat());
                Get<TextMeshProUGUI>((int) Texts.T_EquipStat2).text = GetStatString(meta.GetOwnStatType(),
                    meta.GetOwnStat(), meta.GetOwnGrowthStat());
            }
            
            Get<Image>((int) Images.B_Growth).material =
                Define.GetUIMaterial(isMaxGrowth ||
                                     CurrencyController.I.GetEquipGrowthStoneCount(_equipmentType) < _equipment.GetGrowthStoneCount());
            Get<TextMeshProUGUI>((int) Texts.T_Growth).text = LocalString.Get(isMaxGrowth ? 210207 : 210061);
            Get<GameObject>((int) GameObjects.GrowthCost).SetActive(!isMaxGrowth);
            
            
            string GetStatString(StatType stat, int baseStat, int growthStat)
            {
                return isMaxGrowth ? StringMaker.GetFinalString(stat,baseStat + (growth-1) * growthStat) :
                    StringMaker.GetIncreaseString(stat, baseStat + (growth-1) * growthStat, baseStat + growth * growthStat);
            }
        }
        
        private void WhenCurrencyChanged()
        {
            if (!_equipment.IsMaxGrowth(true))
            {
                var need = _equipment.GetGrowthStoneCount();
                var have = CurrencyController.I.GetEquipGrowthStoneCount(_equipmentType);
                Get<TextMeshProUGUI>((int) Texts.T_GrowthCost).text = Define.AddUnit(have, 3, 2) + "/" +Define.AddUnit(need, 3, 2);
                Get<Image>((int) Images.B_Growth).material = Define.GetUIMaterial(have < need);
            }
        }
        
        private void Growth(PointerEventData eventData)
        {
            if (_equipment.IsMaxGrowth(false))
            {
                _longClickBtn.StopLongClick();
                return;
            }
            if (_equipment.IsMaxGrowth(true))
            {
                var toast = Manager.UI.ShowSingleUI<UI_Toast>();
                toast.SetText(200018);
                _longClickBtn.StopLongClick();
                return;
            }

            if (CurrencyController.I.TryUse(Define.EquipmentToGrowthStone(_equipmentType), _equipment.GetGrowthStoneCount()))
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                _equipment.GrowthIt();
                var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
                levelUp.GetComponent<ParticleSystem>().Play();
                levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
                LevelInfoSet();
            }
            
            if (_equipment.IsMaxGrowth(true)) _longClickBtn.StopLongClick();
        }
        
        
        private void OnDisable()
        {
            _currencyEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _currencyEventsManager?.Reconnect();
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}