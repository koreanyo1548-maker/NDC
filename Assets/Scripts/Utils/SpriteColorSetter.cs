using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace Utils
{
    public class SpriteColorSetter : MonoBehaviour
    {
        private SpriteRenderer[] _sprites;
        
        private void Awake()
        {
            _sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        }
        
        public void FadeToColor(Color inColor, Color outColor, float inTime, float outTime)
        {
            for (var idx = 0; idx < _sprites.Length; ++idx)
            {
                var sprite = _sprites[idx];
                sprite.DOColor(inColor, inTime).OnComplete(() =>
                {
                    sprite.DOColor(outColor, outTime);
                });
            }   
        }

        public void StopFading()
        {
            for (var idx = 0; idx < _sprites.Length; ++idx)
            {
                var sprite = _sprites[idx];
                sprite.DOKill();
                sprite.color = Color.white;
            }   
        }

        // private IEnumerator<float> _FadeToColor(Color inColor, Color outColor, float inTime, float outTime)
        // {
        //     var time = 0f;
        //     var added = Timing.DeltaTime / inTime;
        //     while (time <= 1)
        //     {
        //         var color =  Color.Lerp(outColor, inColor, time);
        //         
        //         time += added;
        //         yield return Timing.WaitForOneFrame;
        //     }
        //
        //     added = Timing.DeltaTime / outTime;
        //     while (time >= 0)
        //     {
        //         time -= added;
        //         var color =  Color.Lerp(outColor, inColor, time);
        //         for (var idx = 0; idx < _sprites.Length; ++idx)
        //         {
        //             _sprites[idx].color = color;
        //         }   
        //         yield return Timing.WaitForOneFrame;
        //     }
        // }
    }
}
