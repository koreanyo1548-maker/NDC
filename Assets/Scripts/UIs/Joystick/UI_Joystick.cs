using System;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UIs.Joystick
{
    public class UI_Joystick : UI_Scene, IDragHandler
    {
        // public float Horizontal => _input.x;//snapX ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x;
        // public float Vertical => _input.y;// snapY ? SnapFloat(input.y, AxisOptions.Vertical) : input.y;
        // public Vector2 Direction => new Vector2(Horizontal, Vertical);

        // public float HandleRange
        // {
        //     get => handleRange;
        //     set => handleRange = Mathf.Abs(value);
        // }
        //
        public float DeadZone
        {
            get => deadZone;
            set => deadZone = Mathf.Abs(value);
        }

        //public bool SnapX { get { return snapX; } set { snapX = value; } }
        //public bool SnapY { get { return snapY; } set { snapY = value; } }

        [SerializeField] private float handleRange = 1;

        [SerializeField] private float deadZone = 0.5f;
        //[SerializeField] private bool snapX = false;
        //[SerializeField] private bool snapY = false;
        private AxisOptions axisOptions = AxisOptions.Both;

        protected RectTransform _background;
        private RectTransform _handle;
        private RectTransform _baseRect;

        private Camera _cam;
        private float _scaleFactor;

        public Vector2 _input = Vector2.zero;

        public Action<float, float> JoystickAction = null; 
        public Action JoystickEndAction = null; 
        public Action JoystickStartAction = null; 

        protected virtual void Start()
        {
            Init();
            _background = transform.Find("Background").GetComponent<RectTransform>();
            _handle = _background.Find("Handle").GetComponent<RectTransform>();
            _baseRect = GetComponent<RectTransform>();
            var canvas = GetComponent<Canvas>();
            _cam = canvas.worldCamera;
            _scaleFactor = canvas.scaleFactor;

            var center = new Vector2(0.5f, 0.5f);
            _background.pivot = center;
            _handle.anchorMin = center;
            _handle.anchorMax = center;
            _handle.pivot = center;
            _handle.anchoredPosition = Vector2.zero;

            gameObject.BindEvent(Functions.TrueCondition, StartUse, UIEffectType.None, false, UIEvent.Down);
            gameObject.BindEvent(Functions.TrueCondition, EndUse, UIEffectType.None, false, UIEvent.Up);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            Using(eventData);
        }

        private bool _isStart;

        protected virtual void StartUse(PointerEventData eventData)
        {
            var position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
            var radius = _background.sizeDelta / 2;
            if (((eventData.position - position) / (radius * _scaleFactor)).magnitude < deadZone) return;

            _isStart = true;
            JoystickStartAction.Invoke();
            Using(eventData);
        }

        private void Using(PointerEventData eventData)
        {
            var position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
            var radius = _background.sizeDelta / 2;
            _input = (eventData.position - position) / (radius * _scaleFactor);
            if (!_isStart)
            {
                if (_input == Vector2.zero) return;
                _isStart = true;
                JoystickStartAction.Invoke();
            }
            
            HandleInput(_input.magnitude, _input.normalized);
            _handle.anchoredPosition = _input * radius * handleRange;
        
            JoystickAction.Invoke(_input.y, _input.x);
        }
    
        protected virtual void EndUse(PointerEventData eventData)
        {
            _isStart = false;
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
            JoystickEndAction.Invoke();
        }

        private void HandleInput(float magnitude, Vector2 normalised)
        {
            // if (magnitude > deadZone)
            // {
            if (magnitude > 1) _input = normalised;
            // }
            // else
            //     _input = Vector2.zero;
        }
    

        private void FormatInput()
        {
            if (axisOptions == AxisOptions.Horizontal)
                _input = new Vector2(_input.x, 0f);
            else if (axisOptions == AxisOptions.Vertical)
                _input = new Vector2(0f, _input.y);
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (axisOptions == AxisOptions.Both)
            {
                float angle = Vector2.Angle(_input, Vector2.up);
                if (snapAxis == AxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                else if (snapAxis == AxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                return value;
            }
            else
            {
                if (value > 0)
                    return 1;
                if (value < 0)
                    return -1;
            }
            return 0;
        }


        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            var localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
            {
                Vector2 sizeDelta;
                var pivotOffset = _baseRect.pivot * (sizeDelta = _baseRect.sizeDelta);
                return localPoint - _background.anchorMax * sizeDelta + pivotOffset;
            }
            return Vector2.zero;
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }

    public enum AxisOptions { Both, Horizontal, Vertical }
}