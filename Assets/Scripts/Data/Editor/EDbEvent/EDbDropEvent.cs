using System;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbDropEvent
    {
        public int Id;
        public string StartDate;
        public int DropDuration;
        public int ShopDuration;
    }
}