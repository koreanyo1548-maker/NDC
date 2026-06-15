using System;
using Managers;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public class Effect_Dash: MonoBehaviour
    {
        private Animator _start;
        private Animator _middle;
        private Animator _end;
        private ParticleSystem _startParticle;

        private int _cur;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (_start != null) return;
            
            _start = transform.Find("Skill_Blink_1").GetComponent<Animator>();
            _middle = transform.Find("Skill_Blink_2").GetComponent<Animator>();
            _end = transform.Find("Skill_Blink_3").GetComponent<Animator>();
            _startParticle = transform.GetComponentInChildren<ParticleSystem>();
            
            _start.gameObject.GetOrAddComponent<EffectAnimation>().Set(PlayNext);
            _middle.gameObject.GetOrAddComponent<EffectAnimation>().Set(PlayNext);
            _end.gameObject.GetOrAddComponent<EffectAnimation>().Set(WhenSkillEnd);
        }

        public void Skill()
        {
            _cur = 0;
            _startParticle.Play();
        }

        public void SetPosition(Vector3 start, Vector3 end)
        {
            Init();
            
            _middle.gameObject.SetActive(false);
            _end.gameObject.SetActive(false);
            
            transform.position = start;
            transform.right = end - start;
            _end.transform.position = end;
        }

        private void PlayNext()
        {
            _cur++;
            if (_cur == 1)
            {
                _middle.gameObject.SetActive(true);
                _middle.Rebind();
                _middle.Update(0f);
            }
            else if (_cur == 2)
            {
                _end.gameObject.SetActive(true);
                _end.Rebind();
                _end.Update(0f);
            }
        }
        
        public virtual void WhenSkillEnd()
        {
            Manager.Resource.Destroy(gameObject);
        }
    }
}