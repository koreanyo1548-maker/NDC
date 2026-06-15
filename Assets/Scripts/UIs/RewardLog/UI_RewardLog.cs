using System.Collections.Generic;
using Data;
using Exceptions;
using Managers;
using MEC;
using UIBases;
using UIs.Shop;
using UnityEngine;
using Utils;

namespace UIs.RewardLog
{
    public class UI_RewardLog : UI_Scene
    {
        private Transform _parent;
        private List<UI_RewardLog_Item> _items = new();
        private int _useIdx = 0;
        private int _maxCount = 4;
        
        private int _stack;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _parent = Util.FindChild<Transform>(gameObject, "RewardLogs", true);

            Manager.Field.CurField.ValueChanged += (_, _) => SetMaxCount();

            return true;
        }

        public void Add(CurrencyType currency, long amount, int id = -1)
        {
            if (Manager.UI.PopupCount > 0) return;
            if (_stack < 20)
            {
                _stack++;
                Timing.CallDelayed(0.05f, () => _stack -= 1);
            }

            if (_stack == 0)
            {
                Make();
            }
            else
            {
                Timing.CallDelayed(0.05f * _stack, Make);
            }

            void Make()
            {
                UI_RewardLog_Item item;
                if (_items.Count >= _maxCount)
                {
                    _useIdx = GetLastIdx();
                    item = _items[_useIdx];
                }
                else
                {
                    item = Manager.UI.MakeSubItem<UI_RewardLog_Item>(_parent);
                    _items.Add(item);
                    _useIdx = _items.Count - 1;
                }
                item.gameObject.SetActive(false);
                item.Set(currency, amount, id);
            }
            

            int GetLastIdx()
            {
                if (_useIdx + 1 >= _maxCount) return 0;
                return _useIdx + 1;
            }
        }


        private void SetMaxCount()
        {
            var field = Manager.Field.CurField.Value;
            switch (field)
            {
                case FieldType.Stage: case FieldType.Training:
                    _maxCount = 4;
                    break;
                case FieldType.BlackMarket: case FieldType.Dia:
                    _maxCount = 1;
                    break;
                case FieldType.Awakening: case FieldType.Pet: case FieldType.Promotion: case FieldType.SkillGrowth:
                    _maxCount = 0;
                    break;
                default:
                    throw new NotDefinedFieldException(field);
            }
        }
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}