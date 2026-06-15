using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;
using Fight.Units;
using UnityEngine;
using Utils;

namespace Managers.Game
{
    public class PetManager
    {
        private Pet[] _pets = new Pet[4];
        private bool _haveDefaultPet;
            
        private EventsManager _lockEventsManager;
        
        #region Monbehaviours
        
        private GameObject _petParent;
        private GameObject PetParent
        {
            get
            {
                if (_petParent != null) return _petParent;

                _petParent = GameObject.Find("@Pet_Root");
                if (_petParent == null)
                {
                    _petParent = new GameObject {name = "@Pet_Root"};
                }

                return _petParent;
            }
        }
        
        #endregion
        
        public PetManager()
        {
            var lockMeta = DbLock.Get(LockType.Pet);
            var isLocked = LevelController.I.CheckIsLocked(lockMeta);
            if (isLocked)
            {
                _lockEventsManager = new EventsManager(Manager.Player, new EventsManager.Config
                {
                    handler = CheckLocked,
                    updatedField = LevelController.I.GetUpdatedFieldForLock(lockMeta)
                });
            }
            else CheckLocked();
        }

        private void CheckLocked()
        {
            if (!LevelController.I.CheckIsLocked(DbLock.Get(LockType.Pet)))
            {
                for (var idx = 0; idx < 4; ++idx)
                {
                    var curIdx = idx;
                    EquipController.data.Pets.Value[curIdx].ValueChanged += (_, _) => WhenPetEquipChanged(curIdx);
                    WhenPetEquipChanged(idx);
                }
                _lockEventsManager?.Dispose();
            }
        }

        private void WhenPetEquipChanged(int equipIdx)
        {
            var petId = EquipController.data.Pets.Value[equipIdx].Value;

            if (petId == -1)
            {
                _pets[equipIdx]?.Remove();
                _pets[equipIdx] = null;
                if (!_haveDefaultPet)
                {
                    foreach (var petField in EquipController.data.Pets.Value)
                    {
                        if (petField.Value != -1) return;
                    }
                
                    var pet = Manager.Resource.Instantiate("Pets/DefaultPet", 1, PetParent.transform).GetOrAddComponent<Pet>();
                    pet.Set(-1, Manager.Player.GetPetPosition(0));
                    _pets[0] = pet;
                    _haveDefaultPet = true;
                }
            }
            else
            {
                _pets[equipIdx]?.Remove();
                if (_haveDefaultPet)
                {
                    _pets[0].Remove();
                    _pets[0] = null;
                    _haveDefaultPet = false;
                }
                var pet = Manager.Resource.Instantiate
                    ("Pets/" + DbPet.Get(petId).Resource, 1, PetParent.transform).GetOrAddComponent<Pet>();
                pet.Set(petId, Manager.Player.GetPetPosition(equipIdx));
                _pets[equipIdx] = pet;
            }

        }
    }
}