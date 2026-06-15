using System;
using System.Collections.Generic;
using Controller;
using UIBases;
using Utils;

namespace UIs.Pet
{
    public class UI_Bookshelf: UI_Base
    {
        private List<UI_Bookshelf_Item> _items;
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _items = new();
            for (var idx = 0; idx < 3; ++idx)
            {
                _items.Add(transform.GetChild(idx).gameObject.GetOrAddComponent<UI_Bookshelf_Item>());
                var curIdx = idx;
                _items[curIdx].Set(curIdx);
            }
            
            return true;
        }
    }
}