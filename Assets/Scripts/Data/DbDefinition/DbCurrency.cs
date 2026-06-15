using System;
using Data.DbEquipment;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbShop;
using Data.Utils;
using Managers;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbCurrency : DbModel<DbCurrency, CurrencyType>
    {
        public int NameId;
        public string Resource;
        public int InitialValue;
        public int DailyCharge;
        public int MaxHave;
        public CurrencyCategoryType Category;

        public override void Load()
        {
            fileName = "Currency";
            if (Application.isPlaying) Init();
        }

        public Sprite GetResource(int id = 0)
        {
            return Manager.Resource.Load<Sprite>(
                Id == CurrencyType.Weapon ? DbWeapon.Get(id).Resource :
                Id == CurrencyType.Accessory ? DbAccessory.Get(id).Resource :
                Id == CurrencyType.Skill ? DbSkill.Get(id).Resource :
                Id == CurrencyType.Pet ? DbPet.Get(id).Resource :
                Id == CurrencyType.Necklace ? DbNecklace.Get(id).Resource :
                Id == CurrencyType.Costume ? DbCostume.Get(id).GetResourceString() :
                Resource);
        }

        public int GetNameId(int id = 0)
        {
            if (Id == CurrencyType.Weapon) return DbWeapon.Get(id).NameId;
            if (Id == CurrencyType.Accessory) return DbAccessory.Get(id).NameId;
            if (Id == CurrencyType.Skill) return DbSkill.Get(id).NameId;
            if (Id == CurrencyType.Pet) return DbPet.Get(id).NameId;
            if (Id == CurrencyType.Necklace) return DbNecklace.Get(id).NameId;
            return NameId;
        }

        public bool IsEquipment()
        {
            return Id is CurrencyType.Weapon or CurrencyType.Accessory or CurrencyType.Skill
                or CurrencyType.Necklace or CurrencyType.Pet;
        }
    }
}