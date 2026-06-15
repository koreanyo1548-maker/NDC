
using Managers;
using MEC;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.Toast
{
    public class UI_Toast: UI_Scene
    {
        enum Texts
        {
            T_Toast
        }

        private Animator _animator;
        
        private void Start()
        {
            transform.GetComponent<Canvas>().sortingOrder = 205;
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            _animator = transform.GetComponent<Animator>();

            return true;
        }

        public void SetText(int id)
        {
            if (!_isInit) Init();

            _animator.Play("Start", 0, 0);
            Get<TextMeshProUGUI>((int) Texts.T_Toast).text = LocalString.Get(id);
        }

        public void SetText(string id)
        {
            if (!_isInit) Init();

            _animator.Play("Start", 0, 0);
            Get<TextMeshProUGUI>((int) Texts.T_Toast).text = id;
        }
        
        private void WhenAnimationDone()
        {
            Manager.UI.CloseSingleUI(this);
        }

        public override bool NeedRaycast()
        {
            return false;
        }
    }
}