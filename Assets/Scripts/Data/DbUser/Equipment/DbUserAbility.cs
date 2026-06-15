using System.Collections.Generic;
using Controller.Play;
using Data.DbAbility;
using Data.Utils;
using Managers;
using Newtonsoft.Json;
using ThirdParty;

namespace Data.DbUser.Equipment
{
    public class DbUserAbility : DbUserModel<DbUserAbility, int>
    {
        public DbField<bool> IsUsing { get; private set; }
        public DbField<bool> IsLocked { get; private set; }
        public DbField<StatType> Option { get; private set; }
        public GradeType OptionGrade { get; private set; }
        public StatType Rune { get; private set; }


        public override void Set(List<DbUserAbility> obj)
        {
            Init(obj);
        }

        public GradeType Change()
        {
            if (IsLocked.Value) return OptionGrade; // 실제로 불리면 안되지만 혹시 모르니 잠궈놓음

            var prev = Option.Value;
            Option.Value = DbAbilityOption.GetRandom();
            OptionGrade = DbAbilityOptionSummon.GetRandomGrade();
            Rune = DbAbilityRune.GetRandom();
            TotalStatController.I.Apply(prev);
            TotalStatController.I.Apply(Option.Value);

            return OptionGrade;
        }

        public void SetLock(bool isLocked)
        {
            IsLocked.Value = isLocked;
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Ability);
        }

        protected override List<DbUserAbility> GetInitials()
        {
            var abilities = new List<DbUserAbility>();
            for (var idx = 0; idx < 25; ++idx)
            {
                abilities.Add(new DbUserAbility(idx, idx < 5, false, StatType.None, GradeType.Normal, StatType.None));
            }

            return abilities;
        }

        public override List<DbUserAbility> AdjustDataModification(List<DbUserAbility> obj)
        {
            return obj;
        }

        public DbUserAbility()
        {

        }

        [JsonConstructor]
        public DbUserAbility(int Id, bool IsUsing, bool IsLocked, StatType Option, GradeType OptionGrade, StatType Rune)
        {
            this.Id = Id;
            this.IsUsing = new DbField<bool>(IsUsing, Id, this);
            this.IsLocked = new DbField<bool>(IsLocked, Id, this);
            this.Option = new DbField<StatType>(Option, Id, this);
            this.OptionGrade = OptionGrade;
            this.Rune = Rune;
        }
    }
}