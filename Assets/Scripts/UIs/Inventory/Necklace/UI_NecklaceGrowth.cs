using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbNecklaceInfo;
using Data.DbUser.Equipment;
using Data.Utils;
using Fight.Stats;
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

namespace UIs.Inventory.Necklace
{
    public class UI_NecklaceGrowth : UI_Popup
    {
        private Sprite[] _equipBtnImgs;
        private EventsManager _currencyEventsManager;
        
        enum Texts
        {
            T_Name,
            T_Level,
            T_Grade1,
            T_Grade2,
            T_EquipStat,
            T_UpgradeCost,
            T_Upgrade,
            T_OwnStat,
            T_AwkStat0,
            T_AwkStat1,
            T_AwkStat2,
            T_AwkCount,
            T_Equip
        }

        enum GameObjects
        {
            B_Prev,
            B_Next,
            Awk0,
            Awk1,
            Awk2,
            UI_Awk_Item,
            IMG_AwkLock
        }

        enum Images
        {
            IMG_Grade1,
            IMG_Equipment1,
            IMG_Grade2,
            IMG_Equipment2,
            IMG_UpgradeIcon,
            B_Upgrade,
            B_Equip
        }

        enum Transforms
        {
            EffectPosition
        }
        
        private DbUserNecklace _necklace;
        
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
                Set(_necklace.PrevHave());
            }, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_necklace.NextHave());
            }, UIEffectType.Bounce);
            Get<Image>((int)Images.B_Equip).gameObject.BindEvent(Functions.TrueCondition, _ => Equip(), UIEffectType.Bounce);

            _equipBtnImgs = new[]
            {
                Manager.Resource.Load<Sprite>("UI_YesButton_round"),
                Manager.Resource.Load<Sprite>("UI_NoButton_round")
            };
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
         

            return true;
        }

        public void Set(DbUserNecklace necklace)
        {
            if (!_isInit) Init();

            _necklace = necklace;
            
            var meta = necklace.Meta;

            Get<GameObject>((int)GameObjects.B_Prev).SetActive(necklace.PrevHave() != null);
            Get<GameObject>((int)GameObjects.B_Next).SetActive(necklace.NextHave() != null);
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(meta.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Grade1).text = Get<TextMeshProUGUI>((int)Texts.T_Grade2).text 
                = LocalString.Get(DbGrade.Get(meta.Grade).NameId);
            Get<Image>((int) Images.IMG_Equipment1).sprite = Get<Image>((int) Images.IMG_Equipment2).sprite = 
                Manager.Resource.Load<Sprite>(meta.Resource);
            Get<Image>((int) Images.IMG_Grade1).sprite = Get<Image>((int) Images.IMG_Grade2).sprite 
                = Manager.Resource.Load<Sprite>(meta.Grade.ToString());

            SetEquip();
            LevelInfoSet();
            WhenCurrencyChanged();
            SetUpgradeBtn();
            
            if (_currencyEventsManager == null)
            {
                _currencyEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenCurrencyChanged,
                    updatedField = new DbField[]
                    {
                        CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone),
                        necklace.Count
                    }
                });
            }
            else
            {
                _currencyEventsManager.Set(WhenCurrencyChanged, new DbField[]
                {
                    CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone),
                    necklace.Count
                });
            }
        }

        private void SetEquip()
        {
            var isEquipped = EquipController.I.IsEquipped(_necklace);
            Get<Image>((int) Images.B_Equip).sprite = _equipBtnImgs[isEquipped ? 1 : 0];
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(isEquipped ? 210085 : 210086);
        }

        private void Equip()
        {
            if (EquipController.I.IsEquipped(_necklace))
            {
                EquipController.I.RemoveNecklaceEquip(_necklace);
            }
            else
            {
                var isSuccess = EquipController.I.EquipNecklace(-1, _necklace);
                if (!isSuccess) Manager.UI.ShowSingleUI<UI_Toast>().SetText(200063);
            }

            SetEquip();
        }

        private void LevelInfoSet()
        {
            var upgradeType = _necklace.GetNextUpgrade();
            var growth = _necklace.Growth.Value;
            var meta = _necklace.Meta;
            
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), growth);
            Get<TextMeshProUGUI>((int) Texts.T_EquipStat).text = GetStatString(meta.EquipStat, 
                DbNecklaceEquipStat.Get(growth).Stats[meta.EquipIdx], upgradeType == 0 ? DbNecklaceEquipStat.Get(growth+1).Stats[meta.EquipIdx] : 0);
            Get<TextMeshProUGUI>((int) Texts.T_OwnStat).text = GetStatString(StatType.NecklaceHpBonus, 
                DbNecklaceOwnStat.Get(growth).Stats[meta.OwnIdx], upgradeType == 0 ? DbNecklaceOwnStat.Get(growth+1).Stats[meta.OwnIdx] : 0);
            _starUI.Set(_necklace.Awakening.Value);

            var lockAwk = upgradeType != 1 && _necklace.Awakening.Value == 0;
            Get<GameObject>((int)GameObjects.IMG_AwkLock).SetActive(lockAwk);
            var awkIdx = 0;
            if (!lockAwk)
            {
                var awkStats = _necklace.GetAwakeningStat(upgradeType == 1);
                foreach (var stat in awkStats)
                {
                    Get<TextMeshProUGUI>((int)Texts.T_AwkStat0 + awkIdx).text = GetStatString(stat.Key, stat.Value.Item1, stat.Value.Item2);
                    Get<GameObject>((int)GameObjects.Awk0 + awkIdx).SetActive(true);
                    awkIdx++;
                }
            }

            while (awkIdx < 3)
            {
                Get<GameObject>((int) GameObjects.Awk0 + awkIdx).SetActive(false);
                awkIdx++;
            }

            Get<GameObject>((int)GameObjects.UI_Awk_Item).SetActive(upgradeType == 1);
            
            Get<Image>((int) Images.B_Upgrade).color = upgradeType == 1 ? Define.ColorFFC34F : Color.white;
            Get<TextMeshProUGUI>((int) Texts.T_Upgrade).text = LocalString.Get(upgradeType == -1 ? 210060 : upgradeType == 0 ? 210061 : 210062);
            Get<Image>((int)Images.IMG_UpgradeIcon).sprite = Manager.Resource.Load<Sprite>(
                upgradeType == 1 ? DbCurrency.Get(CurrencyType.AwakeningStone).Resource : meta.Resource);

            string GetStatString(StatType stat, int curStat, int nextStat)
            {
                return nextStat != 0 ? StringMaker.GetIncreaseString(stat, curStat, nextStat) :
                    StringMaker.GetFinalString(stat,curStat);
            }
        }
        
        private void WhenCurrencyChanged()
        {
            Get<Image>((int) Images.B_Upgrade).material = Define.GetUIMaterial(!_necklace.CanUpgrade());
            var upgradeType = _necklace.GetNextUpgrade();
            if (upgradeType == 1)
            {
                Get<TextMeshProUGUI>((int) Texts.T_UpgradeCost).text = Define.AddUnit(CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value, 3, 2)
                 + "/" +Define.AddUnit(_necklace.GetAwakeningStoneCount(), 3, 2);
                var need = _necklace.GetAwakeningNeedCount();
                var have = _necklace.Count.Value;
                Get<TextMeshProUGUI>((int)Texts.T_AwkCount).text = 
                    (have < need ? "<color=#FF3737>" + Define.AddUnit(have, 3, 2) + "</color>" 
                        : Define.AddUnit(have, 3, 2)) + "/" + Define.AddUnit(need, 3, 2);
            }
            else
            {
                Get<TextMeshProUGUI>((int)Texts.T_UpgradeCost).text = 
                    Define.AddUnit(_necklace.Count.Value, 3, 2) + "/" + 
                    Define.AddUnit(upgradeType == 0 ? _necklace.GetGrowthNeedCount() : _necklace.GetMergeNeedCount(), 3, 2);
            }
        }
        
        private void Upgrade(PointerEventData eventData)
        {
            var upgradeType = _necklace.GetNextUpgrade();
            Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
            _necklace.UpgradeIt();
            var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
            levelUp.GetComponent<ParticleSystem>().Play();
            levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
            LevelInfoSet();

            if (upgradeType == 1)
            {
                var meta = _necklace.Meta;
                var awakening = DbNecklaceAwakening.Get(_necklace.Id);
                var level = _necklace.Awakening.Value;
                var description =
                        StringMaker.GetAwakeningStringWithColor(awakening.GetOption(level), awakening.GetStat(level));

                Manager.UI.ShowSingleUI<UI_AwakeningToast>().SetInfo(Manager.Resource.Load<Sprite>(meta.Resource),
                    LocalString.Get(meta.NameId), level,
                    awakening.GetLevel(level - 1),
                    awakening.GetLevel(level),
                    meta.Grade, description);
            }
            else if (upgradeType == 0 && _necklace.GetNextUpgrade() == 1)
            {
                Get<Image>((int)Images.B_Upgrade).gameObject.GetComponent<UI_EventHandler>().StopLongClick();
            }
            SetUpgradeBtn();
        }

        private void SetUpgradeBtn()
        {
            var upgradeType = _necklace.GetNextUpgrade();
            Get<Image>((int)Images.B_Upgrade).gameObject.UnbindEvent();
            Get<Image>((int)Images.B_Upgrade).gameObject.BindEvent(_necklace.CanUpgrade, Upgrade, UIEffectType.Bounce, false, 
                upgradeType == 1 ? UIEvent.Click : UIEvent.LongClick);
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
        
        
        private void OnDisable()
        {
            _currencyEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _currencyEventsManager?.Reconnect();
        }

    }
}