using System;
using Data.DbDefinition;
using Data.DbUser.Currency;
using Managers;
using UIBases;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Pet
{
    public class UI_PetSummonEffect: UI_Scene
    {
        private DbUserBookshelf _bookshelf;
        
        enum Images
        {
            IMG_SealedBook,
            IMG_Book
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            transform.GetComponent<Canvas>().sortingOrder = 100;
                            
            return true;
        }

        public void Set(DbUserBookshelf bookshelf)
        {
            if (!_isInit) Init();
            _bookshelf = bookshelf;
            Get<Image>((int) Images.IMG_SealedBook).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(_bookshelf.BookType).Resource);
            Get<Image>((int) Images.IMG_Book).sprite = Manager.Resource.Load<Sprite>("Open_"+DbCurrency.Get(_bookshelf.BookType).Resource);
        }

        private void WhenAnimationDone()
        {
            _bookshelf.Unseal();
            Manager.Resource.Destroy(gameObject);
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}