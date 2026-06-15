using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;

using Data.DbUser.Equipment;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Skill;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_Pet: UI_Popup
    {
        private DbUserPet _pet;
        
        private UI_Star _starUI;

        private Sprite[] _equipBtnSprites;
        
        private UI_EquipPet _equipPet;
        private Canvas _canvas;

        public bool Have => _pet.Have.Value;
        
        enum Texts
        {
            T_Name,
            T_Level,
            T_Grade,
            T_EquipAttack,
            T_EquipHp,
            T_Option,
            T_Equip,
            T_BibleHp
        }
        
        enum GameObjects
        {
            B_Prev,
            B_Next,
            Have
        }

        
        enum Images
        {
            IMG_Grade,
            IMG_Equipment,
            B_Equip
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            Get<GameObject>((int)GameObjects.B_Prev).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_pet.Prev());
            }, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_pet.Next());
            }, UIEffectType.Bounce);
            
            _equipBtnSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_DefaultButton_round3"), Manager.Resource.Load<Sprite>("UI_DefaultButton_round4")};
            
            Util.FindChild(gameObject,"IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_Equip", true).BindEvent(Functions.TrueCondition, _ => TryEquip());
            Util.FindChild(gameObject, "B_Growth", true).BindEvent(Functions.TrueCondition, _ => OpenGrowthPopup());
            Util.FindChild(gameObject, "B_Awakening", true).BindEvent(Functions.TrueCondition, _ => OpenAwakeningPopup());
            
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();
            _equipPet = Manager.UI.GetPopupUI<UI_PetInventory>().EquipUI;
            _canvas = transform.GetComponent<Canvas>();
            return true;
        }

        public void Set(DbUserPet pet)
        {
            if (!_isInit) Init();
            _canvas.sortingOrder = _equipPet.GetOrder() + 1;
            _pet = pet;
            var meta = DbPet.Get(pet.Id);
            
            Get<GameObject>((int)GameObjects.B_Prev).SetActive(pet.Prev() != null);
            Get<GameObject>((int)GameObjects.B_Next).SetActive(pet.Next() != null);

            var have = pet.Have.Value;
            Get<GameObject>((int)GameObjects.Have).SetActive(have);

            var isEquipped = have && EquipController.I.IsEquipped(pet);
            Get<Image>((int) Images.B_Equip).sprite = _equipBtnSprites[isEquipped ? 0 : 1];
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(isEquipped ? 210223 : 210222);
            
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), pet.Growth.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(meta.GetNameId());
            _starUI.Set(pet.GetAwakening());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(meta.GetGrade()).NameId);
            Get<Image>((int) Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(meta.GetResource());
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(meta.GetGrade().ToString());
            Get<TextMeshProUGUI>((int) Texts.T_EquipAttack).text = StringMaker.GetFinalString(StatType.PetAttackBonus, 
                meta.EquipAttack + (have ? pet.Growth.Value-1 : 0) * meta.EquipGrowthAttack);
            Get<TextMeshProUGUI>((int)Texts.T_EquipHp).text =  StringMaker.GetFinalString(StatType.PetHpBonus, 
                meta.EquipHp + (have ? pet.Growth.Value-1 : 0) * meta.EquipGrowthHp);
            Get<TextMeshProUGUI>((int) Texts.T_BibleHp).text = string.Format(LocalString.Get(210247),
                LocalString.Get(DbGrade.Get(meta.Grade).NameId), meta.BibleHpBonus);
                
            Get<TextMeshProUGUI>((int)Texts.T_Option).text = (meta.GetAwakening() as DbPetAwakening).GetDescription(meta.GetAwakening().GetStat(pet.GetAwakening()));
        }

        private void TryEquip()
        {
            if (Manager.Field.CurField.Value == FieldType.Pet)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200033);
            }
            else if (EquipController.I.IsEquipped(_pet))
            {
                ClosePopupUI();
                EquipController.I.RemovePetEquip(_pet);
            }
            else
            {
                ClosePopupUI();
                _equipPet.OpenEquipChange(_pet.Id);
            }
        }

        private void OpenGrowthPopup()
        {
            ClosePopupUI();
            Manager.UI.ShowPopupUI<UI_EquipGrowthSP>().Set(EquipmentType.Pet, _pet);
        }

        private void OpenAwakeningPopup()
        {
            ClosePopupUI();
            Manager.UI.ShowPopupUI<UI_EquipAwakening>().Set(EquipmentType.Pet, _pet);
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
