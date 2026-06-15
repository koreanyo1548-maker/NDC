using Controller;
using Controller.Currency;
using Data.DbDefinition;
using Data.DbPetInfo;
using Data.DbUser.Currency;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_Book_Item: UI_Base
    {
        private EventsManager _bookCountEventHandler;

        private DbBook _book;
        
        enum Images
        {
            IMG_Book
        }

        enum Texts
        {
            T_Count
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            gameObject.BindEvent(Functions.TrueCondition, _ => ShowBookPopup(), UIEffectType.Bounce);
            
            _bookCountEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenCountChanged,
                updatedField = null
            });
            return true;
        }

        public void Set(DbBook book)
        {
            if (!_isInit) Init();

            _book = book;
            Get<Image>((int) Images.IMG_Book).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(book.Id).Resource);
            
            _bookCountEventHandler.Set(WhenCountChanged, new []{ CurrencyController.I.GetBookModel(book.Id)});
        }

        private void WhenCountChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.GetBookModel(_book.Id).Value, 3, 2);
        }

        private void ShowBookPopup()
        {
            Manager.UI.ShowPopupUI<UI_Book>().Set(_book);
        }
        
        private void OnEnable()
        {
            _bookCountEventHandler?.Reconnect();
        }

        private void OnDisable()
        {
            _bookCountEventHandler?.Dispose();
        }
    }
}