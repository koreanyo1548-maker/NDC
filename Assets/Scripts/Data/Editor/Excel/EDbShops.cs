using System.Collections.Generic;
using Data.DbShop;
using Data.Editor.EDbDefinition;
using Data.Editor.EDbShop;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Shop")]
    public class EDbShops: ScriptableObject
    {
        [SerializeField] public List<EDbInAppShop> InAppShop;
        [SerializeField] public List<EDbInGameShop> InGameShop;
        [SerializeField] public List<EDbBlackMarket> BlackMarket;
        [SerializeField] public List<EDbPassShop> PassShop;
        [SerializeField] public List<EDbLevelPass> LevelPass;
        [SerializeField] public List<EDbStagePass> StagePass;
        [SerializeField] public List<EDbSoulPass> SoulPass;
        [SerializeField] public List<EDbCoupon> Coupon;
        [SerializeField] public List<EDbAdBuff> AdBuff;
        [SerializeField] public List<EDbCostume> Costume;
        [SerializeField] public List<EDbProfile> Profile;
    }

}