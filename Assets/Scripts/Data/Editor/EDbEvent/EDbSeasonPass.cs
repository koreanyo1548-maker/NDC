using System;
using System.Collections.Generic;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbSeasonPass
    {
        public int Id;
        public string StartDate;
        public int Duration;
        public int ShopId;
        public int NameId;
    }
}