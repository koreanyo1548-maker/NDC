using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;

using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Lock;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_EquipPet: UI_Base
    {
        public enum Mode
        {
            Edit,
            Show
        }
        private EventsManager _equipEventsManager;
        private EventsManager _petEventsManager;
        
        private Animator _animator;
        private Canvas _canvas;
        private int _parentOrder;
        private int _petId;
        
        private Mode _curMode = Mode.Show;
        public Mode CurMode => _curMode;


        enum GameObjects
        {
            PetSelection1 = 0,
            PetSelection2 = 1,
            PetSelection3 = 2,
            PetSelection4 = 3,
            IMG_PetCover
        }

        enum Images
        {
            IMG_Pet1 = 0,
            IMG_Pet2 = 1,
            IMG_Pet3 = 2,
            IMG_Pet4 = 3,
            IMG_Grade1,
            IMG_Grade2,
            IMG_Grade3,
            IMG_Grade4
        }

        enum Texts
        {
            T_Count1,
            T_Count2,
            T_Count3,
            T_Count4,
            T_Grade1,
            T_Grade2,
            T_Grade3,
            T_Grade4,
            T_Level1,
            T_Level2,
            T_Level3,
            T_Level4
        }

        enum Stars
        {
            G_AwakeningStar1,
            G_AwakeningStar2,
            G_AwakeningStar3,
            G_AwakeningStar4,
        }

        private void Awake()
        {
            _canvas = transform.GetComponent<Canvas>();
        }

        private void Start()
        {
            Init();
        }
        
        public override bool Init()
        {
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));

            _animator = Util.FindChild(gameObject, "EquipPets", true).GetComponent<Animator>();
            
            Get<GameObject>((int)GameObjects.IMG_PetCover).BindEvent(Functions.TrueCondition, CloseEquipChange);

            for (var idx = 1; idx <= 4; ++idx)
            {
                var curIdx = idx -1;
                var obj = Util.FindChild(gameObject,"ActivePet" + idx, true);
                
                if (idx == 1) obj.BindEvent(Functions.TrueCondition, eventData => SelectEquip(eventData, curIdx));
                else
                {
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.PetSlot2 + idx - 2, null,
                        Util.FindChild(obj, "IMG_Lock"), null,
                        () => obj.BindEvent(Functions.TrueCondition, eventData => SelectEquip(eventData, curIdx)));
                }
                
                Util.FindChild(gameObject, "G_AwakeningStar" + idx, true).GetOrAddComponent<UI_Star>();
            }
            Bind<UI_Star>(typeof(Stars));

            _equipEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = () => WhenEquipChanged(true),
                updatedField = EquipController.data.Pets.Value.ToArray()
            });

            _petEventsManager = new EventsManager(this, new EventsManager.Config());
            
            WhenEquipChanged(false);
            WhenEquipChanged(true);
            CloseEquipChange(null);
            return true;
        }

        /// <param name="isEquippedChanged"> true:장착 펫이 바뀌는 경우 (), false: 장착 펫의 상태가 바뀌는 경우 </param>
        private void WhenEquipChanged(bool isEquippedChanged)
        {
            var pets = EquipController.data.Pets.Value;
            var count = pets.Count;
            if (isEquippedChanged)
            {
                var equipped = new List<DbField>();
                for (var idx = 0; idx < count; ++idx)
                {
                    if (pets[idx].Value == -1) continue;
                    var pet = DbUserPet.Get(pets[idx].Value);
                    equipped.Add(pet.Awakening);
                    equipped.Add(pet.Growth);
                }
                _petEventsManager.Set(() => WhenEquipChanged(false), equipped.ToArray());
            }
            
            for (var idx = 0; idx < count; ++idx)
            {
                var pet = pets[idx].Value;
                var petInfo = DbUserPet.Get(pet);
                var petMeta = DbPet.Get(pet);
                Get<Image>((int)Images.IMG_Pet1 + idx).sprite = Manager.Resource.Load<Sprite>(pet == -1 ? Define.EmptySprite : DbPet.Get(pet).Resource);
                Get<Image>((int)Images.IMG_Grade1 + idx).sprite = Manager.Resource.Load<Sprite>((pet == -1 ? GradeType.Normal :petMeta.Grade).ToString());
                Get<TextMeshProUGUI>((int) Texts.T_Count1 + idx).text = pet == -1 ? string.Empty : petInfo.Count.Value.ToString();
                Get<TextMeshProUGUI>((int)Texts.T_Grade1 + idx).text = pet == -1 ? string.Empty : LocalString.Get(DbGrade.Get(petMeta.Grade).NameId);
                Get<TextMeshProUGUI>((int)Texts.T_Level1 + idx).text = pet == -1 ? string.Empty : string.Format(LocalString.Get(210041), petInfo.Growth.Value);
                Get<UI_Star>(idx).Set(pet == -1 ? 0 : petInfo.Awakening.Value);
            }
        }

        public void OpenEquipChange(int petId)
        {
            _petId = petId;
            Manager.Guide.Check(this);
            _animator.enabled = true;
            _curMode = Mode.Edit;
            for (var idx = (int)GameObjects.PetSelection1; idx <= (int)GameObjects.PetSelection4; ++idx)
            {
                Get<GameObject>(idx).SetActive(true);
            }
            Get<GameObject>((int)GameObjects.IMG_PetCover).SetActive(true);
        }

        private void CloseEquipChange(PointerEventData eventData)
        {
            Manager.Guide.Check(this);
            _animator.enabled = false;
            _curMode = Mode.Show;
            for (var idx = (int)GameObjects.PetSelection1; idx <= (int)GameObjects.PetSelection4; ++idx)
            {
                Get<GameObject>(idx).SetActive(false);
            }
            Get<GameObject>((int)GameObjects.IMG_PetCover).SetActive(false);
        }

        private void SelectEquip(PointerEventData eventData, int idx)
        {
            if (_curMode == Mode.Show)
            {
                var petId = EquipController.data.Pets.Value[idx].Value;
                if (petId == -1) return;
                Manager.UI.ShowPopupUI<UI_Pet>().Set(DbUserPet.Get(petId));
            }
            else
            {
                EquipController.I.ChangePetEquip(idx, _petId);
                CloseEquipChange(eventData);
            }
        }
        
        private void OnDisable()
        {
            _equipEventsManager?.Dispose();
            _petEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _equipEventsManager?.Reconnect();
            _petEventsManager?.Reconnect();
        }

        public void SetOrder(int order)
        {
            _parentOrder = order;
            _canvas.sortingOrder = _parentOrder + 2;
        }

        public int GetOrder()
        {
            return _canvas.sortingOrder;
        }
    }
}