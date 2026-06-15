using Data.DbEquipment;
using Data.Utils;

namespace Data.DbUser.Equipment
{
    public interface IDbUserEquipment
    {
        public bool IsNew();
        public int GetId();
        public int GetCount();
        public int GetAwakening();
        public int GetGrowth();
        public bool GetHave();
        public DbUserModel GetModel();
        public bool CanMerge(int mergeCount = 1);
        public IDbUserEquipment NextHave(bool exceptLast = false);
        public IDbUserEquipment PrevHave();
        public IDbEquipment GetMeta();

        public bool IsMaxGrowth(bool checkAwakening);
        public long GetGrowthStoneCount();
        public void GrowthIt();
        public void MergeIt(int count = 1);
        public bool IsMaxAwakening();
        public void AwakeningIt();
        public bool CanAwakening();
        public long GetAwakeningStoneCount();
        public int GetAwakeningEquipCount();
        public DbField<int> GetCountModel();
    }
}