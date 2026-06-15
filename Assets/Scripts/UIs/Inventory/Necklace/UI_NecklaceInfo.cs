using System.Collections.Generic;
using System.Linq;
using Controller.Have;
using Data.DbDefinition;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Inventory.Necklace
{
    public class UI_NecklaceInfo: UI_Popup
    {
        private List<UI_NecklaceInfo_Item> _items = new();
        
        enum Texts
        {
            T_Title,
            T_Empty
        }

        enum Transforms
        {
            V_NecklaceInfo
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Transform>(typeof(Transforms));

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            
            return true;
        }

        public void Set(bool isEquipStat)
        {
            if (!_isInit) Init();

            var stats = from pair in (isEquipStat ? NecklaceController.I.GetAllEquipStat() : NecklaceController.I.GetAllOwnStat())
                orderby pair.Value * DbStat.Get(pair.Key).ShowMultiply descending select pair;
            var count = stats.Count();
            
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(isEquipStat ? 210063 : 210064);
            Get<TextMeshProUGUI>((int) Texts.T_Empty).text = count == 0 ? LocalString.Get(isEquipStat ? 210400 : 210401) : string.Empty;
            
            while (_items.Count < count)
            {
                _items.Add(Manager.UI.MakeSubItem<UI_NecklaceInfo_Item>(Get<Transform>((int)Transforms.V_NecklaceInfo)));
            }

            var statIdx = 0;
            foreach (var stat in stats)
            {
                _items[statIdx].gameObject.SetActive(true);
                _items[statIdx++].Set(stat.Key, stat.Value);
            }

            for (var idx = count; idx < _items.Count; ++idx) _items[idx].gameObject.SetActive(false);
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}