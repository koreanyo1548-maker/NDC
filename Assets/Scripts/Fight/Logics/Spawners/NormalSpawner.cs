using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fight.Logics.Spawners
{
    public class NormalSpawner: Spawner, ISpawner
    {
        private int _spawnPos = 0;

        public NormalSpawner(float boundaryX, float boundaryY)
        {
            _boundaryX = boundaryX;
            _boundaryY = boundaryY;
        }
        
        public Vector3[] GetRandomPos(int count)
        {
            if (_spawnPos == 6) _spawnPos = 0;
            var addX = new[] {-1, 0, 0, 1};
            var addY = new[] {0, -1, 1, 0};

            var poses = new Vector3[count];
            var have = 1;
            var xPos = _spawnPos > 2 ? 2 -_spawnPos % 3 : _spawnPos % 3;
            var yPos = _spawnPos > 2 ? 1 : 0;
            poses[0] = new Vector3(SelfClampX(Random.Range(xPos / 3f * _boundaryX, (xPos+1) / 3f * _boundaryX)),
                SelfClampY(Random.Range((yPos+1) / 2f * _boundaryY, yPos / 2f * _boundaryY)));

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

            _spawnPos++;
            return poses;
        }
    }
}