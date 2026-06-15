using System;
using System.Collections.Generic;
using Managers;
using MEC;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Guide
{
    public abstract class UI_Guide: UI_Scene
    {
        public enum GuideHandPositionType
        {
            X0Y68,
            X78Y59
        }
        
        private RectTransform _indicator;
        private ParticleSystem _ring;
        private Transform _hand;

        private Vector3 _zero = Vector3.zero;
        private Vector3 _one = Vector3.one;
        private Dictionary<GuideHandPositionType, Vector3> _handPosition = new()
        {
            {GuideHandPositionType.X0Y68, new Vector3(0, 68, 0)},
            {GuideHandPositionType.X78Y59, new Vector3(78, 59, 0)}
        };

        public override bool Init()
        {   
            if (!base.Init()) return false;
            _indicator = transform.Find("IMG_Indicator").GetComponent<RectTransform>();
            _ring = Util.FindChild<ParticleSystem>(gameObject, "P_Ring", true);
            _hand = transform.Find("IMG_Hand").GetComponent<RectTransform>();

            return true;
        }

        protected void ActiveIndicator(bool isActive)
        {
            if (!_isInit) Init();
            _indicator.gameObject.SetActive(isActive);
            _hand.gameObject.SetActive(isActive);
            if (isActive)
            {
                _ring.Simulate(0.0f, true, false);
                _ring.Play();
            }
            else
            {
                _ring.Stop();
            }
        }

        protected void Set(RectTransform indicate, bool needFlip = false, bool isCanvasEnabled = true, GuideHandPositionType pos = GuideHandPositionType.X0Y68)
        {
            if (!_isInit) Init();
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                transform.SetParent(indicate);
                transform.localPosition = Vector3.zero;
               
                if (isCanvasEnabled)
                {
                    var canvas = gameObject.GetOrAddComponent<Canvas>();
                    canvas.enabled = true;
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = Manager.UI.PopupCount == 0 ? 9 : 50;
                }
                else
                {
                    var scaler = gameObject.GetComponent<CanvasScaler>();
                    if (scaler != null) Destroy(scaler);
                    var canvas = gameObject.GetComponent<Canvas>();
                    if (canvas != null) Destroy(canvas);
                }
                var size = indicate.sizeDelta;
                size.x += 20;
                size.y += 20;
                _indicator.sizeDelta = size;
                _indicator.localPosition = _zero;
                _ring.transform.localPosition = _zero;
                _hand.localPosition = _handPosition[pos];
                _hand.localScale = needFlip ? Define.LookLeft : Define.LookRight;
                if (!isCanvasEnabled)
                {
                    transform.SetParent(transform.parent.parent);
                    transform.SetAsLastSibling();
                }
                transform.localScale = _one;
                ActiveIndicator(true);
            }, gameObject);
        }

        public void Close()
        {
            Manager.UI.CloseSceneUI(this);
        }

        public abstract void Open();
        public abstract void Next(UI_Base popup);

        public override bool NeedRaycast()
        {
            return false;
        }
    }
}