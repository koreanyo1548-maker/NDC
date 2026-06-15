using System;
using Data;
using Data.DbDefinition;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.Inventory.Necklace
{
    public class UI_NecklaceInfo_Item: UI_Base
    {
        enum Texts
        {
            T_StatName,
            T_Stat
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            
            return true;
        }

        public void Set(StatType statType, int stat)
        {
            if (!_isInit) Init();
            
            var meta = DbStat.Get(statType);
            var statValue = (float)Math.Round(stat * meta.ShowMultiply, 4);
            Get<TextMeshProUGUI>((int) Texts.T_StatName).text = LocalString.Get(meta.StaticNameId);
            Get<TextMeshProUGUI>((int)Texts.T_Stat).text = 
                (statValue < 1000 ? statValue.ToString() : Define.AddUnit(Mathf.FloorToInt(statValue), 3, 2))
                + (meta.IsPercent ? "%" : string.Empty);
        }
    }
}