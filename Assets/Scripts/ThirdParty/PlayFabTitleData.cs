using System;
using Data.Stores;

namespace ThirdParty
{
    public class PlayFabTitleData
    {
        public bool CanShowCoupon = true;

        public void CheckCoupon(string coupon, Action afterResponse)
        {
            afterResponse();
        }

        public void CheckMail(Action<MailInfo, bool> withMail, bool handleNewOnly)
        {
            // 서버 메일 제거 — 추후 로컬 or Firebase로 재설계
        }

        public void CheckKick(Action<bool> afterResponse)
        {
            afterResponse(true);
        }

        public void CheckDontRank()
        {
            // no-op
        }

        public void CheckAppStoreReview()
        {
            // no-op
        }
    }
}
