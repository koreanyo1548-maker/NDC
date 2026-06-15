using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Etc.Login
{
    public class UI_SelectServer : MonoBehaviour
    {
        public void Set(Action<int> whenEnter)
        {
            var safeArea = transform.Find("SafeArea");
            var server1 = safeArea.Find("Tg_Server1").GetComponent<Toggle>();

            safeArea.Find("B_Enter").GetComponent<Button>().onClick.AddListener(() =>
            {
                whenEnter(server1.isOn ? 0 : 1);
                gameObject.SetActive(false);
            });
        }
    }
}