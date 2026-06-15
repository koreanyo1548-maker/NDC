using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;

namespace Data.DbUser.Etc
{
    [Serializable]
    public class DbUserBadge: DbUserModel<DbUserBadge, int>
    {
        [DataMember] public DbFieldList<bool> Stats { get; private set; }
        [DataMember] public DbField<bool> LevelPoint { get; private set; }
        [DataMember] public DbField<bool> LevelUp { get; private set; }
        [DataMember] public DbField<bool> Dungeon { get; private set; }
        [DataMember] public DbField<bool> Promotion { get; private set; }
        [DataMember] public DbFieldList<bool> Quests { get; private set; }
        [DataMember] public DbField<bool> Summon { get; private set; }
        [DataMember] public DbFieldList<bool> Weapons { get; private set; }
        [DataMember] public DbFieldList<bool> Accessories { get; private set; }
        [DataMember] public DbFieldList<bool> Relics { get; private set; }
        [DataMember] public DbFieldList<bool> Skills { get; private set; }
        [DataMember] public DbField<bool> Necklace { get; private set; }
        [DataMember] public DbField<bool> InGameShop { get; private set; }
        [DataMember] public DbField<bool> Title { get; private set; }
        [DataMember] public DbField<bool> Book { get; private set; }
        [DataMember] public DbField<bool> Attend { get; private set; }


        public override void Set(List<DbUserBadge> obj)
        {
            Init(obj);
        }


        protected override List<DbUserBadge> GetInitials()
        {
            return new List<DbUserBadge>
            {
                new()
            };
        }

        public override List<DbUserBadge> AdjustDataModification(List<DbUserBadge> obj)
        {
            throw new Exception("Should not be called");
        }

        public DbUserBadge()
        {
            Stats = new DbFieldList<bool>(new List<bool>(), 0, this);
            LevelPoint = new DbField<bool>(false, 0, this);
            LevelUp = new DbField<bool>(false, 0, this);
            Dungeon = new DbField<bool>(false, 0, this);
            Promotion = new DbField<bool>(false, 0, this);
            Quests = new DbFieldList<bool>(new List<bool>(), 0, this);
            Summon = new DbField<bool>(false, 0, this);
            Weapons = new DbFieldList<bool>(new List<bool>(), 0, this);
            Accessories = new DbFieldList<bool>(new List<bool>(), 0, this);
            Relics = new DbFieldList<bool>(new List<bool>(), 0, this);
            Skills = new DbFieldList<bool>(new List<bool>(), 0, this);
            Necklace = new DbField<bool>(false, 0, this);
            InGameShop = new DbField<bool>(false, 0, this);
            Title = new DbField<bool>(false, 0, this);
            Book = new DbField<bool>(false, 0, this);
            Attend = new DbField<bool>(false, 0, this);
        }
    }
}