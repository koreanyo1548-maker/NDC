using DG.Tweening;
using UnityEngine;
using Utils;

namespace UIs.Utils
{
    public class RepeatingAlpha: MonoBehaviour
    {
        private void Start()
        {
            var animator = gameObject.GetOrAddComponent<Animator>();
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animator/Alpha");
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}