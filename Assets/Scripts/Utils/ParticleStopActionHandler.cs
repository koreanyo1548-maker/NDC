using Managers;
using SkillEffects;
using UnityEngine;

namespace Utils
{
    public class ParticleStopActionHandler: MonoBehaviour
    {
        [SerializeField] private ParticleStopAction stopAction;
        
        void OnParticleSystemStopped()
        {
            switch (stopAction)
            {
                case ParticleStopAction.DestroyParent:
                    Manager.Resource.Destroy(transform.parent.gameObject);
                    break;
                case ParticleStopAction.DestroyMe:
                    Manager.Resource.Destroy(gameObject);
                    break;
                case ParticleStopAction.StopSkillInParent:
                    transform.parent.GetComponent<SkillEffect>().SkillEnd();
                    break;
                case ParticleStopAction.StopSkillItSelf:
                    transform.GetComponent<SkillEffect>().SkillEnd();
                    break;
            }
        }

        enum ParticleStopAction
        {
            DestroyParent,
            DestroyMe,
            StopSkillInParent,
            StopSkillItSelf
        }
    }
}