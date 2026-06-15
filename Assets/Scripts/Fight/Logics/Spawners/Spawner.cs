using UnityEngine;

namespace Fight.Logics.Spawners
{
    public abstract class Spawner
    {
        protected float _boundaryX, _boundaryY;
        
        protected bool IsValid(float x, float y)
        {
            return x >= 0 && y < 0 && x < _boundaryX && y >= _boundaryY;
        }

        protected float SelfClampX(float x)
        {
            return Mathf.Clamp(x, 0, _boundaryX);
        }

        protected float SelfClampY(float y)
        {
            return Mathf.Clamp(y, _boundaryY, 0);
        }
        
        protected float SelfClampXWithRandom(float x)
        {
            return Mathf.Clamp(x + Random.Range(-0.5f, 0.5f), 0, _boundaryX);
        }

        protected float SelfClampYWithRandom(float y)
        {
            return Mathf.Clamp(y + Random.Range(-0.5f, 0.5f), _boundaryY, 0);
        }
    }
}