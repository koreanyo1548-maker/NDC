using Controller;
using Controller.Currency;
using Data;
using Data.DbShop;

using Data.DbUser.Currency;
using Data.Stores;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Mail
{
    public class UI_MailInfo: UI_Popup
    {
        private DbUserMail _mail;
        private Transform _rewardParent;
        
        enum Texts
        {
            T_Time,
            T_Title,
            T_Message
        }

        enum Images
        {
            B_GetReward
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            
            _rewardParent = Util.FindChild(gameObject, "RewardParent", true).transform;
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Get<Image>((int)Images.B_GetReward).gameObject.BindEvent(RewardCondition, _ => GetReward(), UIEffectType.Bounce);

            return true;
        }


        public void Set(DbUserMail mail, string time)
        {
            if (!_isInit) Init();

            _mail = mail;
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = time;
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = mail.IsShop ? LocalString.Get(mail.InAppShop.NameId) : mail.MailInfo.title;
            Get<TextMeshProUGUI>((int) Texts.T_Message).text = mail.IsShop ? LocalString.Get(210263) : mail.MailInfo.contents;

            var rewards = Define.SmallToBigReward(mail.Rewards);
            
            var rewardCount = rewards.Count;
            for (var idx = 0; idx < rewardCount; ++idx)
            {
                if (_rewardParent.childCount <= idx)
                {
                    var item = Manager.UI.MakeSubItem<UI_MailReward_Item>(_rewardParent);
                    item.transform.localScale = Vector3.one;
                    item.Set(rewards[idx]);
                }
                else
                {
                    _rewardParent.GetChild(idx).GetComponent<UI_MailReward_Item>().Set(rewards[idx]);
                }
            }
            var listCount = _rewardParent.childCount;
            for (var idx = listCount-1; idx >= rewardCount; --idx)
            {
                Manager.Resource.Destroy(_rewardParent.GetChild(idx).gameObject);
            }

            SetIsRewarded();
        }

        private void SetIsRewarded()
        {
            var isRewarded = _mail.IsRewarded.Value;
            Get<Image>((int) Images.B_GetReward).material = Define.GetUIMaterial(isRewarded);
            var rewardCount = _rewardParent.childCount;
            for (var idx = 0; idx < rewardCount; ++idx)
            {
                _rewardParent.GetChild(idx).GetComponent<UI_MailReward_Item>().SetRewarded(isRewarded);
            }
        }

        private void GetReward()
        {
            _mail.GetReward();
            SetIsRewarded();
        }

        private bool RewardCondition()
        {
            return !_mail.IsRewarded.Value;
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