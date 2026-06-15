using System;
using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPromote;

using Exceptions;
using Managers;
using UIBases;
using UIs.Etc;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Lock
{
    public class UI_Locked: UI_Base
    {
        private DbLock _lock;

        private Image _image;
        private GameObject _lockObj;
        private GameObject _unlockObj;

        private Action _whenUnlocked;

        private EventsManager _lockEventsManager;

        public void Set(LockType lockType, Image image, GameObject lockObj, GameObject unlockObj, Action whenUnlocked)
        {
            _lock = DbLock.Get(lockType);

            _image = image;
            _lockObj = lockObj;
            _unlockObj = unlockObj;
            _whenUnlocked = whenUnlocked;

            if (LevelController.I.CheckIsLocked(_lock))
            {
                if (_image != null) _image.material = Define.GetUIMaterial(true);
                _lockObj.SetActive(true);
                if (_unlockObj != null) _unlockObj.SetActive(false);
                
                gameObject.BindEvent(Functions.TrueCondition, _ => ShowLockToast(), UIEffectType.Bounce);
            
                _lockEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = CheckLocked,
                    updatedField = LevelController.I.GetUpdatedFieldForLock(_lock)
                });
            }
            else CheckLocked();
        }
        
        private void CheckLocked()
        {
            var isLocked = LevelController.I.CheckIsLocked(_lock);

            if (!isLocked)
            {
                if (_image != null) _image.material = Define.GetUIMaterial(false);
                _lockObj.SetActive(false);
                if (_unlockObj != null) _unlockObj.SetActive(true);
                _lockEventsManager?.Dispose();

                gameObject.UnbindEvent();
                _whenUnlocked();
                
                if (_lockEventsManager != null && _lock.UsePopup) Manager.UI.ShowPopupUI<UI_UnlockContents>().SetText(_lock.NameId);
                Destroy(transform.GetComponent<UI_Locked>());
            }
        }

        private void ShowLockToast()
        {
            var condition = string.Empty;
            switch (_lock.Condition)
            {
                case LockConditionType.Promotion: condition = string.Format(LocalString.Get(200029), LocalString.Get(DbPromotion.Get(_lock.Goal).NameId)); break;
                case LockConditionType.Stage: condition = string.Format(LocalString.Get(200030), _lock.Goal); break;
                case LockConditionType.BibleLevel: condition = string.Format(LocalString.Get(200035), _lock.Goal); break;
                case LockConditionType.GuideQuest:
                    condition = string.Format(LocalString.Get(200031), _lock.Goal-10001);
                    break;
                case LockConditionType.NewbieDay:
                    return;
                default: throw new NotDefinedLockConditionException(_lock.Condition);
            }
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(condition);
        }
    }
}