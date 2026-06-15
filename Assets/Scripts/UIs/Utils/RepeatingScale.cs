using DG.Tweening;
using UnityEngine;
using Utils;

namespace UIs.Utils
{
    public class RepeatingScale: MonoBehaviour
    {
        private void Start()
        {
            var animator = gameObject.GetOrAddComponent<Animator>();
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animator/Badge");
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}