using Managers;
using UnityEngine;
using DG.Tweening;
using MEC;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace Scenes
{
    public class CIScene: MonoBehaviour
    {
        public Image[] cis;
        public GameObject open;
        private void Start()
        {
            for (var idx = 0; idx < cis.Length; ++idx)
            {
                var img = cis[idx];
                img.color = Define.ColorTransparent;
                img.DOFade(1, 0.5f).OnComplete(() =>
                {
                    img.DOFade(1, 1).OnComplete(() =>
                    {
                        img.DOFade(0, 0.5f).OnComplete(() =>
                        {
                            open.SetActive(true);
                        });
                    });
                });
            }
            
            Timing.CallDelayed(2.2f, () => SceneManager.LoadScene(SceneType.Login.ToString()));
        }
    }
}