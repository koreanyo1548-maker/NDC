using System;
using System.Collections.Generic;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbAttendEvent
    {
        public int Id;
        public string StartDate;
        public int Duration;
        public List<int> RewardIds;
    }
}