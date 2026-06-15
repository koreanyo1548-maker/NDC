using UnityEngine;
using UnityEngine.Events;
namespace SkillEffects
{
    public class EffectAnimation: MonoBehaviour
    {
        public UnityAction Call;
        public void Set(UnityAction call)
        {
            Call = call;
        }
        public void CallFunction()
        {
            Call();
        }
    }
}