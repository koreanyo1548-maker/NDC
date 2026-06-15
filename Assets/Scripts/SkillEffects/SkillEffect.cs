using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public abstract class SkillEffect: MonoBehaviour
    {
        public int effectId { get; protected set; }
        // protected Action _hitAction;

        public virtual Vector3 GetReferencePosition()
        {
            return default;
        }
        
        public abstract void Skill();
        //
        // public abstract void SetPosition(Vector3 start, Vector3 end);
        //
        public void SetStartPosition(Vector3? start = null, bool isMove = false)
        {
            transform.SetParent(Manager.Player.SkillEffectParent);
            if (!isMove)
            {
                if (start == null) transform.localPosition = Define.Zero3;
                else transform.position = (Vector3)start;
            }
            else
            {
                transform.localPosition = Define.Zero3;
                transform.DOMove((Vector3)start, 2).SetEase(Ease.Linear);
            }
        }

        public void SetDirection(Vector3 direction)
        {
            var cur = transform.position;
            Manager.Player.LookAt(cur.x > direction.x);
            var z = Mathf.Atan2(direction.y - cur.y, direction.x - cur.x) * 180 / Mathf.PI;
            transform.SetPositionAndRotation(cur, Quaternion.Euler(0, 0, z));
            for (var idx = 0; idx < transform.childCount; ++idx) transform.GetChild(idx).localRotation = Quaternion.Euler(0, 0, -z);
        }

        /// <summary>
        /// Target Attack, Stun인 경우 구현해야함
        /// </summary>
        public virtual void SetTarget(Transform target, Action toDo)
        {

        }

        /// <summary>
        /// Attack인 경우 구현해야함
        /// </summary>
        public virtual void SetAction(Action toDo)
        {
            
        }
        //
        // public abstract void SetPosition(Transform parent, Vector3 start);

        /// <summary>
        /// DOKill 해주기
        /// </summary>
        public virtual void WhenSkillEnd(bool reverseCall)
        {
            // _hitAction = null;
            Manager.Resource.Destroy(gameObject);
            if (reverseCall) Manager.Skill.RemoveMe(effectId);
        }

        public void SkillEnd()
        {
            WhenSkillEnd(true);
        }

        // private void OnDisable()
        // {
        //     _hitAction = null;
        // }
        //
        //
        // public void SetHitAction(Action action)
        // {
        //     _hitAction += action;
        // }
        //
        // public void Hit()
        // {
        //     _hitAction?.Invoke();
        // }
    }
}