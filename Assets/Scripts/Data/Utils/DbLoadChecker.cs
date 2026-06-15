using ThirdParty;
using UIs.Etc;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace Data.Utils
{
    public class DbLoadChecker: Singleton<DbLoadChecker>
    {
        public bool IsAllLoaded = false;
        private int check = 119;

        // int count = 0;
        public void Check()
        {
            check--;
//            Debug.Log($"check {check}");
            if (check == 0)
            {
                // Debug.Log("done");
                IsAllLoaded = true;
                new ResourceLoadChecker().StartLoad();
            }
            if (check < 0)
            {
                Debug.LogError("data load counting error");
            }
        }
    }
}