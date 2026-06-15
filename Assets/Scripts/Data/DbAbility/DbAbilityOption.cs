using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbAbility
{
    [Serializable]
    public class DbAbilityOption: DbModel<DbAbilityOption, StatType>
    {
        public static List<StatType> _keys = new();
        public List<int> Value;
        
        public override void Load()
        {
            fileName = "AbilityOption";
            if (Application.isPlaying) Init();
            ForEach(a => _keys.Add(a.Id));
        }

        public static StatType GetRandom()
        {
            return _keys[Random.Range(0, Count)];
        }
    }
}