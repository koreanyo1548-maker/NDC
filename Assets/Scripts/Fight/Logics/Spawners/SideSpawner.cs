using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fight.Logics.Spawners
{
    public class SideSpawner: Spawner, ISpawner
    {
        private int _spawnPos = 1;

        public SideSpawner(float boundaryX, float boundaryY)
        {
            _boundaryX = boundaryX;
            _boundaryY = boundaryY;
        }
        
        public Vector3[] GetRandomPos(int count)
        {
            if (_spawnPos == 1) _spawnPos = 3;
            else if (_spawnPos == 3) _spawnPos = 1;
            var addX = new[] {-1, 0, 0, 1};
            var addY = new[] {0, -1, 1, 0};

            var poses = new Vector3[count];
            var have = 1;
            var xPos = _spawnPos % 5;
            poses[0] = new Vector3(SelfClampX(Random.Range(xPos / 5f * _boundaryX, (xPos+1) / 5f * _boundaryX)),
                SelfClampY(Random.Range(0, _boundaryY)));

            var compareIdx = 0;
            var prevHave = 1;
            while (have < count)
            {
                if (prevHave != have) compareIdx = 0;
                prevHave = have;
                var compareX = poses[compareIdx].x;
                var compareY = poses[compareIdx].y;
                for (var idx = 0; idx < 4 && have < count; ++idx)
                {
                    var x = compareX + addX[idx];
                    var y = compareY + addY[idx];
                    if (IsValid(x, y) && !Array.Exists(poses, p => Mathf.Abs(p.x - x) + Mathf.Abs(p.y - y) < 0.5f))
                    {
                        poses[have] = new Vector3(SelfClampXWithRandom(x), SelfClampYWithRandom(y));
                        have++;
                    }
                }

                compareIdx++;
            }

            return poses;
        }
    }
}