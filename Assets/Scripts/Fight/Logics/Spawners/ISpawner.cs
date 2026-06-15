using UnityEngine;

namespace Fight.Logics.Spawners
{
    public interface ISpawner
    {
        public Vector3[] GetRandomPos(int count);
    }
}