using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Managers.Base
{
    public class UpdateManager: MonoBehaviour
    {
        public List<IUpdateable> _updates;

        private void Awake()
        {
            _updates = new List<IUpdateable>();
        }

        private void Update()
        {
            var count = _updates.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                _updates[idx].OnUpdate();
            }
        }

        public void Add(IUpdateable update)
        {
            if (_updates.Contains(update)) return;
            _updates.Add(update);
        }

        public void Remove(IUpdateable update)
        {
            _updates.Remove(update);
        }
    }
}