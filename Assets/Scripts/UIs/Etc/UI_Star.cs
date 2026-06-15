using UIBases;
using UnityEngine;

namespace UIs.Etc
{
    public class UI_Star: UI_Base
    {
        enum GameObjects
        {
            IMG_Stars1,
            IMG_Stars2,
            IMG_Stars3,
            IMG_Stars4,
            IMG_Stars5,
            IMG_Stars6,
            IMG_Stars7,
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            
            return true;
        }

        public void Set(int count)
        {
            if (!_isInit) Init();
            
            for (var idx = 0; idx < count; ++idx) Get<GameObject>((int)GameObjects.IMG_Stars1+idx).SetActive(true);
            for (var idx = count; idx < 7; ++idx) Get<GameObject>((int)GameObjects.IMG_Stars1+idx).SetActive(false);
        }
    }
}