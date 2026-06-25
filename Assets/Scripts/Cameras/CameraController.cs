using System;
using Controller;
using Controller.Play;
using DG.Tweening;
using Managers;
using UnityEngine;
using Utils;

namespace Cameras
{
    public class CameraController: MonoBehaviour
    {
        private static CameraController instance;
        public static CameraController I => instance;

        public Vector3 Center => _camera.transform.position;
        
        private Transform _player;
        private Vector3 _position = new(0, 0, -10);
        private Transform _holder;

        private float _minX;
        private float _maxX;
        [SerializeField]
        private float _minY;
        [SerializeField]
        private float _maxY;

        private LayerMask _effectLayer;
        private LayerMask _powerSavingLayer;
        private LayerMask _defaultLayer;

        private Camera _camera;
        
        private void Awake()
        {
            instance = this;
            _holder = transform.parent;
            _camera = transform.GetComponent<Camera>();
            
            _effectLayer = 1 << LayerMask.NameToLayer("Effect");
            _powerSavingLayer = 1 << LayerMask.NameToLayer("PowerSaving");
            _defaultLayer = _camera.cullingMask;
            OnOffUI(false);
        }

        public void OnOffPowerSaving(bool isOn)
        {
            if (isOn) _camera.cullingMask = _powerSavingLayer;
            else _camera.cullingMask = _defaultLayer;
        }

        public void OnOffUI(bool isOn)
        {
            if (isOn) _camera.cullingMask &= ~_effectLayer;
            else _camera.cullingMask |= _effectLayer;
        }
        
        public void Init(Transform player)
        {
            _player = player;
            var cameraSizeY = GetComponent<Camera>().orthographicSize;

            _minX = Screen.width * cameraSizeY / Screen.height+0.3f;
            _maxX = Manager.Field.MaxX - _minX;
            _maxY = -0.61f;
            _minY = -1.8f;
            
            _position.x = _player.position.x;
            _position.y =  _player.position.y;
            _holder.position = _position;
        }
        
        
        private void LateUpdate()
        {
            var direction = _player.position - _holder.position;
            var velocity = direction.magnitude * Time.deltaTime * 5;
            var normalize = direction.normalized;
            _position.x = Mathf.Clamp(_position.x + velocity * normalize.x, _minX, _maxX);
            _position.y = Mathf.Clamp(_position.y + velocity * normalize.y, _minY, _maxY);
            _holder.position = Vector3.Lerp(_holder.position, _position, 0.15f);
        }

        public void Shake(float power = 1)
        {
            if (!SettingController.data.IsCameraShaking.Value) return;
            transform.DOKill();
            transform.localPosition = Define.Zero3;
            transform.DOShakePosition(0.5f * power, 0.1f * power, 10, 90);
        }
    }
}
