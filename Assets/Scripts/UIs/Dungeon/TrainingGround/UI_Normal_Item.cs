using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbShop;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_Normal_Item: UI_Base
    {
        enum GameObjects
        {
            IMG_Grade,
            IMG_Reward,
            IMG_EquipmentReward,
            T_Grade
        }

        enum Texts
        {
            T_ItemCount,
            T_Grade
        }

        enum Images
        {
            IMG_Grade,
            IMG_Reward,
            IMG_EquipmentReward
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            return true;
        }

        public void Set(DbReward reward)
        {
            if (!_isInit) Init();
            
            Set(reward.currencyType, reward.count, reward.id);
        }

        public void Set(CurrencyType currency, long count, int id)
        {
            if (!_isInit) Init();
            
            var meta = DbCurrency.Get(currency);
            var isEquipment = meta.IsEquipment();
            var isCostume = currency == CurrencyType.Costume;
            Get<GameObject>((int)GameObjects.IMG_Grade).SetActive(isEquipment || isCostume);
            Get<GameObject>((int)GameObjects.IMG_Reward).SetActive(!isEquipment && !isCostume);
            Get<GameObject>((int)GameObjects.IMG_EquipmentReward).SetActive(isEquipment || isCostume);
            Get<GameObject>((int)GameObjects.T_Grade).SetActive(isEquipment || isCostume);
            Get<TextMeshProUGUI>((int)Texts.T_ItemCount).gameObject.SetActive(!isCostume);

            Get<TextMeshProUGUI>((int) Texts.T_ItemCount).text = Define.AddUnit(count, 3, 2);

            if (isEquipment)
            {
                var equipMeta = DbSelector.GetEquipment(meta.Id, id);
                Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(equipMeta.GetGrade()).NameId);
                Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(equipMeta.GetGrade().ToString());
                Get<Image>((int) Images.IMG_EquipmentReward).sprite = Manager.Resource.Load<Sprite>(meta.Resource);
            }
            else if (isCostume)
            {
                var equipMeta = DbCostume.Get(id);
                Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(equipMeta.Grade).NameId);
                Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(equipMeta.Grade.ToString());
                Get<Image>((int) Images.IMG_EquipmentReward).sprite = equipMeta.GetResource();
            }
            else
            {
                Get<Image>((int) Images.IMG_Reward).sprite = Manager.Resource.Load<Sprite>(meta.Resource);
            }
        }
    }
}