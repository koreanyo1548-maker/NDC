using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbCoupon
    {
        public string Id;
        public List<string> Rewards;
    }
}