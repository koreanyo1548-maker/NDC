using Fight.Units;
using Managers;
using UnityEngine;
using Utils;

namespace Cameras
{
    public class TouchEffects: MonoBehaviour, IUpdateable
    {
        private Camera _camera;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _camera = transform.GetComponent<Camera>();
            Manager.Updates.Add(this);
        }

        public void OnUpdate()
        {
            #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                var position = _camera.ScreenToWorldPoint(Input.mousePosition);
            #else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                var position = _camera.ScreenToWorldPoint(Input.GetTouch(0).position);
            #endif
                position.z = 0;
                var effect = Manager.Resource.Instantiate("Particles/TouchEffect", 5, transform);
                effect.transform.position = position;
                var particle = effect.GetComponent<ParticleSystem>(); 
                particle.Simulate(0, true, true);
                particle.Play(true);
            }
        }
    }
}