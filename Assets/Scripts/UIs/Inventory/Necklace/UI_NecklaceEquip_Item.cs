using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbDefinition;
using Data.DbNecklaceInfo;
using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Necklace
{
    public class UI_NecklaceEquip_Item: UI_Base
    {
        private EventsManager _necklaceEventsManager;
        private EventsManager _equipEventHandler;
        // private EventsManager _checkCanEquipEventHandler;

        private int _position;
        private DbUserNecklace _necklace;

        private UI_Star _starUI;
        
        private ControllerField<bool> _isEquipped = new(false);

        enum Texts
        {
            T_Level,
            T_Count,
            T_Grade
        }

        enum GameObjects
        {
            Have,
            T_Empty
        }

        enum Images
        {
            IMG_Necklace,
            IMG_Grade,
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();
            
            gameObject.BindEvent(Functions.TrueCondition, _ => ShowNecklaceInfo(), UIEffectType.Bounce);

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            return true;
        }

        public void Set(int position)
        {
            if (!_isInit) Init();
            _position = position;
            
            _equipEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = new[] {EquipController.data.Necklaces.Value[position]}
            });
            // _checkCanEquipEventHandler = new EventsManager(this,  new EventsManager.Config
            // {
            //     handler = CheckCanEquipAnything,
            //     updatedField = DbNecklace GetAllGradeHave()
            // });
            // _checkCanEquipEventHandler.Dispose();
            
            WhenEquipChanged();

            Util.FindChild(gameObject, "IMG_Badge", true).GetOrAddComponent<UI_ControllerBadge>()
            .Set(new [] {_isEquipped}, () => !_isEquipped.Value);

            // DbField[] GetAllGradeHave()
            // {
            //     var necklaces = new DbField[35];
            //     var idx = 0;
            //     DbNecklace.ForEach(n =>
            //     {
            //         necklaces[idx++] = DbUserNecklace.Get(n.Id).Have;
            //     });
            //     return necklaces;
            // }
        }

        private void ShowNecklaceInfo()
        {
            if (_necklace != null)
            {
                Manager.UI.ShowPopupUI<UI_NecklaceGrowth>().Set(_necklace);
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(LocalString.Get(200062));
            }
        }
        
        
        private void SetNecklace()
        {
            var id = EquipController.I.GetEquippedNecklace(_position);
            var have = id != -1;
            Get<GameObject>((int)GameObjects.Have).SetActive(have);
            Get<GameObject>((int)GameObjects.T_Empty).SetActive(!have);
            if (!have)
            {
                _necklace = null;
                return;
            }
            
            _necklace = DbUserNecklace.Get(id);
            var meta = _necklace.Meta;
            Get<TextMeshProUGUI>((int)Texts.T_Grade).text = LocalString.Get(DbGrade.Get(meta.Grade).NameId);
            Get<Image>((int)Images.IMG_Necklace).sprite = Manager.Resource.Load<Sprite>(meta.Resource);
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(meta.Grade.ToString());

            WhenNecklaceChanged();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenNecklaceChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Level).text =
                string.Format(LocalString.Get(210041), _necklace.Growth.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(_necklace.Count.Value, 3, 2);

            _starUI.Set(_necklace.Awakening.Value);
        }

        // private void CheckCanEquipAnything()
        // {
        //     var canEquip = false;
        //     DbUserNecklace.ForEach(n => n.Meta.Grade == _grade, n =>
        //     {
        //         if (!canEquip && n.Have.Value) canEquip = true;
        //     });
        //     Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(canEquip);
        // }

        private void WhenEquipChanged()
        {
            var id = EquipController.I.GetEquippedNecklace(_position);
            var have = id != -1;
            _isEquipped.Value = have;
            // else _checkCanEquipEventHandler.Reconnect();
            
            if (_necklaceEventsManager == null)
            {
                if (have)
                {
                    _necklaceEventsManager = new EventsManager(this, new EventsManager.Config
                    {
                        handler = WhenNecklaceChanged,
                        updatedEntity = new[] {DbUserNecklace.Get(id)}
                    });
                }
            }
            else
            {
                if (have)
                {
                    _necklaceEventsManager.Set(WhenNecklaceChanged, new[]{DbUserNecklace.Get(id)});
                }
                else
                {
                    _necklaceEventsManager.Dispose();
                }
            }
            
            SetNecklace();
            if (!have) Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(Define.NoneEquipSprite);
        }

        private void OnDisable()
        {
            _necklaceEventsManager?.Dispose();
            _equipEventHandler.Dispose();
        }

        private void OnEnable()
        {
            if (_isInit) WhenEquipChanged();
            _equipEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            if (_necklace != null) Get<TextMeshProUGUI>((int)Texts.T_Grade).text =
                LocalString.Get(DbGrade.Get(_necklace.Meta.Grade).NameId);
        }
    }
}