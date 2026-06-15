using UnityEngine;
using UnityEngine.EventSystems;

namespace UIs.Joystick
{
    public class UI_FloatingJoystick: UI_Joystick
    {
        protected override void Start()
        {
            base.Start();
            _background.gameObject.SetActive(false);
            GetComponent<Canvas>().sortingOrder = -1;
        }

        protected override void StartUse(PointerEventData eventData)
        {
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            _background.gameObject.SetActive(true);
            base.StartUse(eventData);
        }

        protected override void EndUse(PointerEventData eventData)
        {
            _background.gameObject.SetActive(false);
            base.EndUse(eventData);
        }
    }
}