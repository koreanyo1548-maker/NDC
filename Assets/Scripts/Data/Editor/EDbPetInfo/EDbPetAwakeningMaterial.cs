using System;
using System.Collections.Generic;

namespace Data.Editor.EDbPetInfo
{
    [Serializable]
    public class EDbPetAwakeningMaterial
    {
        public int Id;
        public List<int> EquipmentCounts;
        public List<int> Stones;

    }
}