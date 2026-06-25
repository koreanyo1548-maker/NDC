using Controller;
using Controller.Infos;
using Data.DbEquipment;
using Data.DbShop;
using Managers;
using UnityEngine;
using Utils;

namespace Costume
{
    public class CostumeSetter: MonoBehaviour
    {
        // Spine 캐릭터로 전환하면서 기존 2D SpriteRenderer 외형은 사용하지 않는다.
        // 추후 Spine 코스튬 Skin 컬럼이 준비되면 SpineEquipmentSetter 쪽에서 슬롯별로 처리한다.
        private const bool UseSpineAppearance = true;

        // Base sprites (항상 표시되는 기본 파트)
        private SpriteRenderer _head;
        private SpriteRenderer _baseEyes;
        private SpriteRenderer _baseFace;
        private SpriteRenderer _baseHair;
        private SpriteRenderer _baseMouth;
        private SpriteRenderer _body;
        private SpriteRenderer _cloak;

        // Costume overlays (코스튬 레이어)
        private SpriteRenderer _eyes;
        private SpriteRenderer _frontHead;
        private SpriteRenderer _backHead;
        private SpriteRenderer _face;

        private SpriteRenderer _weapon;

        private void Awake()
        {
            if (UseSpineAppearance)
            {
                DisableLegacySpriteRenderers();
                enabled = false;
                return;
            }

            _head = Util.FindChild<SpriteRenderer>(gameObject, "head", true);
            _baseEyes = Util.FindChild<SpriteRenderer>(gameObject, "eyes", true);
            _baseFace = Util.FindChild<SpriteRenderer>(gameObject, "face", true);
            _baseHair = Util.FindChild<SpriteRenderer>(gameObject, "Hair", true);
            _baseMouth = Util.FindChild<SpriteRenderer>(gameObject, "mouth", true);
            _body = Util.FindChild<SpriteRenderer>(gameObject, "Body", true);
            _cloak = Util.FindChild<SpriteRenderer>(gameObject, "cloak", true);
            _eyes = Util.FindChild<SpriteRenderer>(gameObject, "Costume_eyes", true);
            _frontHead = Util.FindChild<SpriteRenderer>(gameObject, "Costume_headF", true);
            _backHead = Util.FindChild<SpriteRenderer>(gameObject, "Costume_headB", true);
            _face = Util.FindChild<SpriteRenderer>(gameObject, "Costume_face", true);
            _weapon = Util.FindChild<SpriteRenderer>(gameObject, "Weapon", true);

            EquipController.data.BodyCostume.ValueChanged += (_, _) => SetBody();
            EquipController.data.WeaponCostume.ValueChanged += (_, _) => SetWeapon();
            EquipController.data.Weapon.ValueChanged += (_, _) => SetWeapon();
            SetBody();
            SetWeapon();
        }

        private void DisableLegacySpriteRenderers()
        {
            DisableRenderer("head");
            DisableRenderer("eyes");
            DisableRenderer("face");
            DisableRenderer("Hair");
            DisableRenderer("mouth");
            DisableRenderer("Body");
            DisableRenderer("cloak");
            DisableRenderer("Costume_eyes");
            DisableRenderer("Costume_headF");
            DisableRenderer("Costume_headB");
            DisableRenderer("Costume_face");
            DisableRenderer("Weapon");
        }

        private void DisableRenderer(string childName)
        {
            var renderer = Util.FindChild<SpriteRenderer>(gameObject, childName, true);
            if (renderer != null) renderer.enabled = false;
        }

        private void SetBody()
        {
            var costume = EquipController.data.BodyCostume.Value;
            var sprites = Manager.Resource.Load<CostumeScriptableObject>("Costumes/" + (costume == 0 ? "Default" : DbCostume.Get(costume).Resource));

            // Base sprites
            // ppu != 100이면 전체 교체 코스튬 — null 슬롯은 렌더러 비활성화
            bool fullReplacement = sprites.isFullReplacement;
            ApplyOrHide(_head,      sprites.head,     fullReplacement);
            ApplyOrHide(_baseEyes,  sprites.baseEyes,  fullReplacement);
            ApplyOrHide(_baseFace,  sprites.baseFace,  fullReplacement);
            ApplyOrHide(_baseHair,  sprites.baseHair,  fullReplacement);
            ApplyOrHide(_baseMouth, sprites.baseMouth, fullReplacement);
            ApplyOrHide(_body,      sprites.body,      fullReplacement);
            ApplyOrHide(_cloak,     sprites.cloak,     fullReplacement);

            // Costume overlays
            _eyes.sprite = sprites.eyes;
            _frontHead.sprite = sprites.frontHead;
            _backHead.sprite = sprites.backHead;
            _face.sprite = sprites.face;

        }

        private static void ApplyOrHide(SpriteRenderer sr, Sprite sprite, bool hideIfNull)
        {
            if (sprite != null)
            {
                sr.enabled = true;
                sr.sprite = sprite;
            }
            else if (hideIfNull)
            {
                sr.enabled = false;
            }
        }

        private Vector3 _particlePosition = new Vector3(-0.9f, 3.66f, 0);
        private void SetWeapon()
        {
            // var weapon = EquipController.data.Weapon.Value;
            // _weapon.sprite = Manager.Resource.Load<Sprite>(weapon != 0 ? DbWeapon.Get(weapon).Resource : "Scythe (1)", false);
            var weaponCostume = EquipController.data.WeaponCostume.Value;
            if (weaponCostume == 0)
            {
                var weapon = EquipController.data.Weapon.Value;
                _weapon.sprite = Manager.Resource.Load<Sprite>(weapon != 0 ? DbWeapon.Get(weapon).Resource : "Scythe (1)", false);
            }
            else
            {
                _weapon.sprite = Manager.Resource.Load<Sprite>("Weapon_" + DbCostume.Get(weaponCostume).Resource, false);
            }
            
            // if (weaponCostume == 20)
            // {
            //     var particle = Manager.Resource.InstantiateParticle("Particles/WeaponWizardEffect", 1, _weapon.transform).GetComponent<ParticleSystem>();
            //     particle.transform.localPosition = _particlePosition;
            //     particle.transform.localScale = Define.PointTwo;
            //     particle.Simulate( 0.0f, true, true );
            //     particle.Play();
            // }
            // else
            // {
            if (_weapon.transform.childCount > 0)
                Manager.Resource.Destroy(_weapon.transform.GetChild(0).gameObject);
            // }
        }
    }
}
