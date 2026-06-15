using System;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserBuying
    {
        public int ItemId;
        public int BuyCnt;

        public DbUserBuying(int itemId, int buyCnt)
        {
            ItemId = itemId;
            BuyCnt = buyCnt;
        }
    }
}