using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserEquip: DbUserModel<DbUserEquip, int>
    {
        [DataMember] public DbField<int> Weapon { get; private set; }
        [DataMember] public DbField<int> Accessory { get; private set; }
        [DataMember] public List<DbFieldList<int>> Skills { get; private set; }
        [DataMember] public DbFieldList<int> Necklaces { get; private set; }
        [DataMember] public DbFieldList<int> Pets { get; private set; }
        [DataMember] public DbField<int> Title { get; private set; }
        [DataMember] public DbField<int> BodyCostume { get; private set; }
        [DataMember] public DbField<int> WeaponCostume { get; private set; }
        [DataMember] public DbField<int> Profile { get; private set; }


        public override void Set(List<DbUserEquip> obj)
        {
            Init(obj);
            if (Get(0).Profile.Value == 0) Get(0).Profile.Value = 1;
        }


        protected override List<DbUserEquip> GetInitials()
        {
            return new List<DbUserEquip>
            {
                new()
            };
        }

        public override List<DbUserEquip> AdjustDataModification(List<DbUserEquip> obj)
        {
            return obj;
        }


        [JsonConstructor]
        public DbUserEquip(int Id, int Weapon, int Accessory, List<List<int>> Skills, 
            List<int> Necklaces, List<int> Pets, 
            int Title, int BodyCostume, int WeaponCostume, int Profile)
        {
            this.Id = Id;
            this.Weapon = new DbField<int>(Weapon, 0, this);
            this.Accessory = new DbField<int>(Accessory, 0, this);
            this.Skills = new List<DbFieldList<int>>();
            for (var idx = 0; idx < Skills.Count; ++idx)
            {
                this.Skills.Add(new DbFieldList<int>(Skills[idx], 0, this));
                foreach (var skill in this.Skills[idx].Value)
                {
                    skill.SetId(0, this);
                }
            }

            this.Necklaces = new DbFieldList<int>(Necklaces, 0, this);
            this.Pets = new DbFieldList<int>(Pets, 0, this);
            this.Title = new DbField<int>(Title, 0, this);
            this.BodyCostume = new DbField<int>(BodyCostume, 0, this);
            this.WeaponCostume = new DbField<int>(WeaponCostume, 0, this);
            this.Profile = new DbField<int>(Profile, 0, this);
        }

        public DbUserEquip()
        {
            Id = 0;
            Weapon = new DbField<int>(0, Id, this);
            Accessory = new DbField<int>(0, Id, this);
            Skills = new List<DbFieldList<int>>();
            for (var idx = 0; idx < 5; ++idx)
            {
                Skills.Add(new DbFieldList<int>( new List<int> {-1, -1, -1, -1}, Id, this));
            }

            Necklaces = new DbFieldList<int>(new List<int> {-1, -1, -1, -1, -1, -1, -1}, Id, this);
            Pets = new DbFieldList<int>(new List<int> {-1, -1, -1, -1}, Id, this);
            Title = new DbField<int>(0, Id, this);
            BodyCostume = new DbField<int>(0, Id, this);
            WeaponCostume = new DbField<int>(0, Id, this);
            Profile = new DbField<int>(1, Id, this);
        }
    }
}