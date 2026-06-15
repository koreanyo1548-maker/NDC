using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_TrainingGroundCount: UI_Scene
    {
        private GameObject _dimmed;
        private DOTweenAnimation _countScaleAnim;
        private TextMeshProUGUI _countText;

        enum EffectType
        {
            Punch,
            Scale
        }
        
        public override bool Init()
        { 
            if (!base.Init()) return false;
            transform.GetComponent<Canvas>().sortingOrder = 201;

            _dimmed = transform.Find("IMG_Dimmed").gameObject;
            _countText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Count", true);
            _countScaleAnim = _countText.GetComponent<DOTweenAnimation>();
            
            return true;
        }

        public void Count(int count, Action toDo)
        {
            if (!_isInit) Init();
            _dimmed.SetActive(count == 3);
            Timing.RunCoroutine(_CountRoutine(count, count == 3 ? EffectType.Punch : EffectType.Scale, toDo));
        }

        IEnumerator<float> _CountRoutine(int count, EffectType effect, Action onComplete)
        {
            while (count > 0)
            {
                _countText.text = count.ToString();
                if (effect == EffectType.Punch) _countText.transform.DOPunchScale(Define.One, 0.2f, 12, 1).SetEase(Ease.OutQuad).SetLoops(1);
                else if (effect == EffectType.Scale) _countScaleAnim.DORestart();
                count--;
                yield return Timing.WaitForSeconds(1);
            }

            onComplete();
            Manager.UI.CloseSceneUI(this);
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}