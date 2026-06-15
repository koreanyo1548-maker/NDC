using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbRecord
{
    [Serializable]
    public class EDbGuideQuest
    {
        public int Id;
        public QuestType ToDo;
        public int NameId;
        public int Goal;
        public int EffectId;
        public int GoTo;

        public List<string> QuestRewards;
        
    }
}