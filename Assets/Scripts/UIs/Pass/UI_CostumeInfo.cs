using Data.DbDefinition;
using Data.DbShop;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Pass
{
    public class UI_CostumeInfo: UI_Popup
    {
        enum Images
        {
            IMG_Grade,
            IMG_Equip
        }

        enum Texts
        {
            T_Name,
            T_Grade,
            T_Info
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            gameObject.BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            return true;
        }

        public void Set(DbCostume costume, Vector3 position, Vector2 pivot)
        {
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int)Texts.T_Name).text = LocalString.Get(costume.NameId);
            Get<TextMeshProUGUI>((int)Texts.T_Info).text = StringMaker.GetCostumeOptionString(costume);
            Get<TextMeshProUGUI>((int)Texts.T_Grade).text = LocalString.Get(DbGrade.Get(costume.Grade).NameId);
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(costume.Grade.ToString());
            Get<Image>((int) Images.IMG_Equip).sprite = costume.GetResource();

            var child = transform.GetChild(0);
            child.GetComponent<RectTransform>().pivot = pivot;
            child.gameObject.SetActive(false);
            Timing.CallDelayed(Timing.DeltaTime, () =>
            {
                child.gameObject.SetActive(true);
                child.position = position;
            });
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            
        }
    }
}