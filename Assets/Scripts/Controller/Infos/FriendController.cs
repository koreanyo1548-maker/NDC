using System.Collections.Generic;
using Utils;

namespace Controller.Infos
{
    public class FriendController: Singleton<FriendController>
    {
        public List<string> Friends;
        public int FriendRewarded;

        public void Set(List<string> friends, int friendRewarded)
        {
            Friends = friends;
            FriendRewarded = friendRewarded;
        }
    }
}