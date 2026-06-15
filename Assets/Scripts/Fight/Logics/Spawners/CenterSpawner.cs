using UnityEngine;

namespace Fight.Logics.Spawners
{
    public class CenterSpawner: Spawner, ISpawner
    {
        private int _spawnPos = 0;

        public CenterSpawner(float boundaryX, float boundaryY)
        {
            _boundaryX = boundaryX;
            _boundaryY = boundaryY;
        }
        
        public Vector3[] GetRandomPos(int count)
        {
            return new Vector3[] {new(_boundaryX * 0.5f + 3, _boundaryY * 0.5f)};
        }
    }
}