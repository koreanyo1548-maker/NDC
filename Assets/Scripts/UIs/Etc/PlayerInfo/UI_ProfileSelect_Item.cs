using System;
using Controller.Infos;
using Data.DbShop;
using Managers;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_ProfileSelect_Item: UI_Base
    {
        enum GameObjects
        {
            IMG_Lock,
            IMG_Equip
        }

        enum Images
        {
            UI_ProfileSelect_Item
        }

        private DbProfile _profile;
        private bool _isLocked = true;

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            
            return true;
        }

        public void Set(DbProfile profile)
        {
            if (!_isInit) Init();
            
            _profile = profile;
            
            Get<Image>((int)Images.UI_ProfileSelect_Item).sprite = Manager.Resource.Load<Sprite>(_profile.Resource);

            gameObject.BindEvent(CanEquip, _ => EquipProfile(), UIEffectType.Bounce);

            EquipController.data.Profile.ValueChanged += (_,_) => SetEquip();
            
            SetLock();
            SetEquip();
        }

        private void SetEquip()
        {
            var isEquipped = EquipController.I.IsEquipped(_profile);
            Get<GameObject>((int)GameObjects.IMG_Equip).SetActive(isEquipped);
            Get<Image>((int) Images.UI_ProfileSelect_Item).color = isEquipped || _isLocked ? Define.Color7D7D7D : Color.white;
        }

        private bool CanEquip()
        {
            return !EquipController.I.IsEquipped(_profile);
        }
        
        private void SetLock()
        {
            if (!_isLocked) return;
            _isLocked = !_profile.IsConditionPassed();
            Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(_isLocked);
            Get<Image>((int) Images.UI_ProfileSelect_Item).color = _isLocked ? Define.Color7D7D7D : Color.white;
        }

        private void EquipProfile()
        {
            if (_isLocked)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(string.Format(LocalString.Get(210382), LocalString.Get(DbCostume.Get(_profile.Goal).NameId)));
            }
            else
            {
                EquipController.I.Equip(_profile);
            }
        }
        
        private void OnEnable()
        {
            if (_isInit) SetLock();
        }
    }
}