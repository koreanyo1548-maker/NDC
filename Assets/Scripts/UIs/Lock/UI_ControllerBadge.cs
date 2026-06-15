using System;
using Controller;
using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbDefinition;
using Data.Utils;
using UIBases;
using UIs.Utils;
using UnityEngine.UI;
using Utils;

namespace UIs.Lock
{
    public class UI_ControllerBadge: UI_Base
    {
        private EventsManager _badgeEventsManager;
        private EventsManager _lockEventsManager;
        
        private ControllerField[] _updated;
        private LockType _lockType;
        private Func<bool> _isOnFunc;

        private Image _image;
        private RepeatingScale _scaling;
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            
            _image = transform.GetComponent<Image>();
            _scaling = gameObject.GetOrAddComponent<RepeatingScale>();
            
            return true;
        }
        
        public void Set(ControllerField[] updated, Func<bool> isOnFunc, LockType? lockType = null)
        {
            if (!_isInit) Init();
            
            _updated = updated;
            _isOnFunc = isOnFunc;
            
            if (lockType == null)
            {
                _badgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenBadgeChanged,
                    updatedController = _updated
                });
                
                WhenBadgeChanged();
            }
            else
            { 
                _lockType = (LockType) lockType;
                var lockMeta = DbLock.Get(_lockType);
                var isLocked = LevelController.I.CheckIsLocked(lockMeta);
                if (isLocked)
                { 
                    _lockEventsManager = new EventsManager(this, new EventsManager.Config
                    {
                        handler = CheckLocked,
                        updatedField = LevelController.I.GetUpdatedFieldForLock(lockMeta)
                    });
                    gameObject.SetActive(false);
                }
                else CheckLocked();
            }
        }
        
        private void CheckLocked()
        {
            if (!LevelController.I.CheckIsLocked(DbLock.Get(_lockType)))
            {
                _badgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenBadgeChanged,
                    updatedController = _updated
                });
                
                WhenBadgeChanged();
                _lockEventsManager?.Dispose();
            }
        }
        
        private void WhenBadgeChanged()
        {
            var enable = _isOnFunc();
            _image.enabled = enable;
            _scaling.enabled = enable;
        }

        private void OnEnable()
        {
            _badgeEventsManager?.Reconnect();
        }

        private void OnDisable()
        {
            _badgeEventsManager?.Dispose();
        }
    }
}