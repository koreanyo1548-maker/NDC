using UnityEngine;

namespace Utils
{
    [DisallowMultipleComponent]
    public class MonsterPartSorter : MonoBehaviour
    {
        [SerializeField] private int baseOrder;
        [SerializeField] private int zStep = 100;
        [SerializeField] private bool updateEveryFrame = true;

        private Transform _visualRoot;
        private SpriteRenderer[] _renderers;

        public void Init(Transform visualRoot)
        {
            _visualRoot = visualRoot != null ? visualRoot : transform;
            Refresh();
            Apply();
        }

        private void Awake()
        {
            if (_visualRoot == null)
                _visualRoot = transform;

            Refresh();
        }

        private void LateUpdate()
        {
            if (updateEveryFrame)
                Apply();
        }

        public void Refresh()
        {
            if (_visualRoot == null)
                _visualRoot = transform;

            _renderers = _visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        }

        public void Apply()
        {
            if (_visualRoot == null || _renderers == null) return;

            for (var idx = 0; idx < _renderers.Length; ++idx)
            {
                var spriteRenderer = _renderers[idx];
                if (spriteRenderer == null) continue;

                var relativeZ = _visualRoot.InverseTransformPoint(spriteRenderer.transform.position).z;
                spriteRenderer.sortingOrder = baseOrder + Mathf.RoundToInt(-relativeZ * zStep);
            }
        }
    }
}
