using Controller.Infos;
using Data.DbEquipment;
using Data.Utils;
using Fight.Units;
using UnityEngine;

namespace Costume
{
    public class SpineEquipmentSetter : MonoBehaviour
    {
        private Player _player;
        private SimpleSpineSkinAssigner _skinAssigner;
        private string _defaultRightHandSkin;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _skinAssigner = _player != null ? _player.SkinAssigner : GetComponentInChildren<SimpleSpineSkinAssigner>();
            if (_skinAssigner == null) return;

            _defaultRightHandSkin = _skinAssigner.rightHandWeaponSkin;

            EquipController.data.Weapon.ValueChanged += WhenEquipmentChanged;
            EquipController.data.Accessory.ValueChanged += WhenEquipmentChanged;
            EquipController.data.WeaponCostume.ValueChanged += WhenEquipmentChanged;

            Apply();
        }

        private void OnDestroy()
        {
            EquipController.data.Weapon.ValueChanged -= WhenEquipmentChanged;
            EquipController.data.Accessory.ValueChanged -= WhenEquipmentChanged;
            EquipController.data.WeaponCostume.ValueChanged -= WhenEquipmentChanged;
        }

        private void WhenEquipmentChanged(object sender, DbEventArgs eventArgs)
        {
            Apply();
        }

        public void Apply()
        {
            if (_skinAssigner == null) return;

            _skinAssigner.rightHandWeaponSkin = GetRightHandSkin();
            _skinAssigner.leftHandWeaponSkin = GetLeftHandSkin();
            _skinAssigner.AssignSkins();
        }

        private string GetRightHandSkin()
        {
            var costumeSkin = GetRightHandCostumeSkin();
            if (costumeSkin != null) return costumeSkin;

            var weapon = EquipController.data.Weapon.Value;
            if (weapon == 0) return _defaultRightHandSkin;

            var skin = DbWeapon.Get(weapon).GetSpineRightHandSkin();
            return string.IsNullOrEmpty(skin) ? _defaultRightHandSkin : skin;
        }

        private string GetLeftHandSkin()
        {
            var costumeSkin = GetLeftHandCostumeSkin();
            if (costumeSkin != null) return costumeSkin;

            var accessory = EquipController.data.Accessory.Value;
            if (accessory == 0) return string.Empty;

            var skin = DbAccessory.Get(accessory).GetSpineLeftHandSkin();
            return string.IsNullOrEmpty(skin) ? string.Empty : skin;
        }

        private static string GetRightHandCostumeSkin()
        {
            if (EquipController.data.WeaponCostume.Value == 0) return null;

            // Weapon costume Spine skins will be resolved here once the costume table has matching columns.
            return string.Empty;
        }

        private static string GetLeftHandCostumeSkin()
        {
            return null;
        }
    }
}
