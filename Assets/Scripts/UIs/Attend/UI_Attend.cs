using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Data;
using Managers;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Attend
{
    public class UI_Attend: UI_Popup
    {
        private EventsManager _eventManager;
        
        private Transform _itemParent;
        private List<UI_Attend_Item> _items = new();
        
        private bool _spawnGameWhenClose;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            _itemParent = Util.FindChild<Transform>(gameObject, "G_Attend", true);
            for (var idx = 0 ; idx < 7; ++idx)
            {
                var item = _itemParent.GetChild(idx).gameObject.GetOrAddComponent<UI_Attend_Item>();
                item.Set(idx+1);
                _items.Add(item);
            }
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Util.FindChild(gameObject, "ANIM_Attend", true).GetOrAddComponent<AnimationEventSetter>().SetAction(CheckAttend);
            
            _eventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenLoopChanged,
                updatedField = new[] {AttendController.data.NextDay}
            });
            
            return true;
        }
        

        private void CheckAttend()
        {
            if (AttendController.I.CanRewarded.Value)
            {
                AttendController.I.Attend(() =>
                {
                    var curRewarded = AttendController.data.NextDay.Value - 1;
                    if (curRewarded == 0) curRewarded = 7;
                    _items[curRewarded-1].SetStatus(true, true);
                });
            }
        }
        
        private void WhenLoopChanged()
        {
            if (AttendController.data.NextDay.Value != 1 || !AttendController.I.CanRewarded.Value) return;

            var nextDay = AttendController.data.NextDay.Value;
            for (var idx = 0; idx < 7; ++idx)
            {
                _itemParent.GetChild(idx).gameObject.GetOrAddComponent<UI_Attend_Item>()
                    .SetStatus(idx + 1 < nextDay);
            }
        }

        public void SetSpawnGameWhenClose()
        {
            _spawnGameWhenClose = true;
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            if (_spawnGameWhenClose)
            {
                Manager.Field.SpawnGame();
                _spawnGameWhenClose = false;
            }
        }

        private void OnDisable()
        {
            _eventManager.Dispose();
        }

        private void OnEnable()
        {
            _eventManager?.Reconnect();
        }
    }
}