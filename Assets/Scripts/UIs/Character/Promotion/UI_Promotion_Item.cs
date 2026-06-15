using Controller.Infos;
using Data;
using Data.DbPromote;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.StageResult;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Promotion
{
    public class UI_Promotion_Item: UI_Base, ILanguageSet
    {
        private EventsManager _promotionEventsManager;
        private EventsManager _costumeEventsManager;

        private DbPromotion _promotion;
        private Sprite[] _btnSprites;
            
        enum Texts
        {
            T_ScenarioName,
            T_Position,
            T_Attack,
            T_Hp,
            T_Equip
        }

        enum GameObjects
        {
            B_Equip,
            B_Enter
        }
        
        enum Images
        {
            B_Equip,
            B_Enter,
            IMG_Skin
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            
            Get<GameObject>((int)GameObjects.B_Enter).BindEvent(Functions.TrueCondition, _ => EnterDungeon());
            Get<GameObject>((int)GameObjects.B_Equip).BindEvent(EquipCondition, _ => EquipCostume(), UIEffectType.Bounce);

            _btnSprites = new[]
            {
                Manager.Resource.Load<Sprite>("UI_YesButton_round"), Manager.Resource.Load<Sprite>("UI_NoButton_round")
            };
            
            _promotionEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenPromotionChanged,
                updatedField = new[] {LevelController.data.Promotion, LevelController.data.MaxStage}
            });
            _costumeEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenCostumeChanged,
                updatedField = new[] {EquipController.data.BodyCostume}
            });

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            return true;
        }

        public void SetInfo(DbPromotion promotion)
        {
            _promotion = promotion;
            if (!_isInit) Init();

            Get<TextMeshProUGUI>((int) Texts.T_ScenarioName).text = string.Empty;//LocalString.Get(scenario.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Position).text = LocalString.Get(promotion.NameId);
            Get<TextMeshProUGUI>((int)Texts.T_Attack).text = StringMaker.GetFinalStringWithColor(StatType.FinalAttackBonus, promotion.Attack, "FFD53B");
            Get<TextMeshProUGUI>((int) Texts.T_Hp).text = StringMaker.GetFinalStringWithColor(StatType.FinalHpBonus, promotion.Hp, "FFD53B");
            Get<Image>((int)Images.IMG_Skin).sprite = Manager.Resource.Load<Sprite>(promotion.Resource);
            
            
            WhenPromotionChanged();
        }

        private void WhenPromotionChanged()
        {
            var cur = LevelController.data.Promotion.Value;

            var isClear = _promotion.Id <= cur;
            Get<GameObject>((int)GameObjects.B_Enter).SetActive(!isClear);
            Get<GameObject>((int)GameObjects.B_Equip).SetActive(isClear);
            if (!isClear) Get<Image>((int) Images.B_Enter).material = Define.GetUIMaterial(cur+1 != _promotion.Id 
                || LevelController.data.MaxStage.Value < _promotion.UnlockCondition);

            WhenCostumeChanged();
        }

        private void WhenCostumeChanged()
        {
            var isClear = _promotion.Id <= LevelController.data.Promotion.Value;
            if (!isClear) return;
            
            var equip = EquipController.I.IsEquipped(CostumePositionType.Body, _promotion.CostumeId);
            Get<Image>((int) Images.B_Equip).sprite = _btnSprites[equip ? 1 : 0];
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(equip ? 210054 : 210053);
        }

        private bool EquipCondition()
        {
            return _promotion.Id <= LevelController.data.Promotion.Value;
        }
        

        private void EnterDungeon()
        {
            if (Manager.Field.CurField.Value == FieldType.Stage)
            {
                if (_promotion.Id == LevelController.data.Promotion.Value + 1 && LevelController.data.MaxStage.Value >= _promotion.UnlockCondition)
                {
                    Manager.UI.CloseAllPopupUI();
                    Manager.Field.EnterDungeon(FieldType.Promotion, _promotion.Id);
                    Manager.UI.ShowSingleUI<UI_BossStage>().SetToast(210241);
                }
                else if (_promotion.Id > LevelController.data.Promotion.Value + 1)
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200009);
                }
                else
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(string.Format(LocalString.Get(200010), _promotion.UnlockCondition));
                }
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200011); 
            }
        }

        private void EquipCostume()
        {
            EquipController.I.EquipCostume(CostumePositionType.Body, _promotion.CostumeId);
        }

        
        private void OnDisable()
        {
            _promotionEventsManager?.Dispose();
            _costumeEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _promotionEventsManager?.Reconnect();
            _costumeEventsManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            SetInfo(_promotion);
        }
    }
}