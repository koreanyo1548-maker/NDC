using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbSummon
{
    [Serializable]
    public class EDbSummonNumberProbability
    {
        public GradeType Id;
        public List<int> Probability;
    }
}