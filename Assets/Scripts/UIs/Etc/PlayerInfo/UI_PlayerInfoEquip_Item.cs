using Data.DbDefinition;
using Data.DbEquipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_PlayerInfoEquip_Item: UI_Base
    {
        private UI_Star _starUI;
        
        enum Texts
        {
            T_Level,
			T_Grade
        }

        enum Images
        {
            IMG_Grade,
            IMG_Equipment
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            return true;
        }

        public void SetInfo(IDbEquipment equipment)
        {
            if (!_isInit) Init();
            
            var _equipment = DbSelector.GetUserEquipment(equipment.GetEquipmentType(), equipment.GetId());
            
            Get<Image>((int)Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(equipment.GetResource());
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(equipment.GetGrade().ToString());
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _equipment.GetGrowth());
            Get<TextMeshProUGUI>((int)Texts.T_Grade).text = LocalString.Get(DbGrade.Get(equipment.GetGrade()).NameId);
            
            _starUI.Set(_equipment.GetAwakening());
        }

        public void SetNull()
        {
            if (!_isInit) Init();
            
            Get<Image>((int)Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(Define.EmptySprite);
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(Define.NoneEquipSprite);
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Empty;
            Get<TextMeshProUGUI>((int)Texts.T_Grade).text = string.Empty;
            _starUI.Set(0);
        }
    }
}