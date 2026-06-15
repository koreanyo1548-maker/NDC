using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Currency;
using Managers;
using UIBases;
using UIs.Pet;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Mail
{
    public class UI_Mail : UI_Popup
    {
        private EventsManager _mailEventManager;
        
        private Dictionary<int, UI_Mail_Item> _mails = new();

        enum Transforms
        {
            G_MailParent
        }
        
        
        void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Transform>(typeof(Transforms));
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Util.FindChild(gameObject, "B_GetAll", true).BindEvent(Functions.TrueCondition, _ => GetAllMailReward(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_DeleteAllRewarded", true).BindEvent(Functions.TrueCondition, _ => DeleteAllRewardedMail(), UIEffectType.Bounce);

            var mailParent = Get<Transform>((int) Transforms.G_MailParent);
            
            var toRemove = new List<int>();
            CurrencyController.data.Mail.ForEach(
                mail =>
                {
                    if (mail.IsHide.Value) return;
                    if (!mail.IsShop && mail.MailInfo == null)
                    {
                        toRemove.Add(mail.Id);
                        return;
                    }
                    var item = Manager.UI.MakeSubItem<UI_Mail_Item>(mailParent);
                    item.Set(mail);
                    item.transform.SetSiblingIndex(0);
                    _mails.Add(mail.Id, item);
                });

            CurrencyController.I.RemoveMails(toRemove);
            
            _mailEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenMailCountChanged,
                updatedField = new [] {CurrencyController.data.Mail}
            });
            
            return true;
        }

        private void WhenMailCountChanged()
        {
            var mailItemIds = _mails.Keys.ToList();
            foreach (var mail in mailItemIds)
            {
                if (!CurrencyController.I.HaveMail(mail))
                {
                    Manager.Resource.Destroy(_mails[mail].gameObject);
                    _mails.Remove(mail);
                }
            }
            CurrencyController.data.Mail.ForEach(
                m =>
                {
                    if (!m.IsHide.Value && !_mails.ContainsKey(m.Id) && (m.IsShop || m.MailInfo != null))
                    {
                        var item = Manager.UI.MakeSubItem<UI_Mail_Item>(Get<Transform>((int)Transforms.G_MailParent));
                        item.transform.localScale = Vector3.one;
                        item.Set(m);
                        item.transform.SetSiblingIndex(0);
                        _mails.Add(m.Id, item);
                    }
                });
        }

        private void GetAllMailReward()
        {
            if (!CurrencyController.I.GetAllMailReward())
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200036);
            }
        }

        private void DeleteAllRewardedMail()
        {
            CurrencyController.I.DeleteAllRewardedMail();
        }

        private void OnEnable()
        {
            _mailEventManager?.Reconnect();
        }

        private void OnDisable()
        {
            _mailEventManager?.Dispose();
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