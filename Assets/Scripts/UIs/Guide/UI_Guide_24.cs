using Managers;
using MEC;
using UIBases;
using UIs.Character;
using UIs.FieldMain;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_24: UI_Guide_Promotion
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _promotionIndex = 0;

            return true;
        }
    }
}