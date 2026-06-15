using System;
using Controller;
using Controller.Play;
using Data.DbShop;
using Data.Utils;
using UnityEngine.Serialization;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserAdBuff
    {
        public AdBuffType AdBuffType;
        public StatType BuffType;
        public DateTime StartTime;
        public DbField<bool> IsUsing;
        public int LeftTime;
        public int UseCount;

        public DbUserAdBuff(AdBuffType adBuffType, StatType buffType, DateTime startTime, bool isUsing, int useCount, int leftTime, DbUserModel parent)
        {
            AdBuffType = adBuffType;
            BuffType = buffType;
            StartTime = startTime;
            IsUsing = new DbField<bool>(isUsing, 0, parent);
            UseCount = useCount;
            LeftTime = leftTime;
        }

        public void Use(DateTime time)
        {
            UseCount++;
            StartTime = time;
            LeftTime = DbAdBuff.Get(AdBuffType).Duration;
            IsUsing.Value = true;
            TotalStatController.I.Apply(BuffType);
        }

        public void AdSkipUse()
        {
            IsUsing.Value = true;
            TotalStatController.I.Apply(BuffType);
        }
    }
}