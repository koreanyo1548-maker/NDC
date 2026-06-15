using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbAbility
{
    [Serializable]
    public class DbAbilityOptionSummon: DbModel<DbAbilityOptionSummon, GradeType>
    {
        public int Probability;
        
        public override void Load()
        {
            fileName = "AbilityOptionSummon";
            if (Application.isPlaying) Init();
        }

        public static GradeType GetRandomGrade()
        {
            var pick = GradeType.Normal;
            var gradeChoose = Random.Range(0, 1000000);

            var pr = 0;
            while (gradeChoose >= pr)
            {
                pr += Get(pick).Probability;
                pick++;
            }
            return pick - 1;
        }
    }
}