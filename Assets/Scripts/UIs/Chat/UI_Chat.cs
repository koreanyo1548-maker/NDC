using System;
using System.Collections.Generic;
using Controller;
using Data;
using dynamicscroll;
using Managers;
using MEC;
using Newtonsoft.Json;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Summon;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Chat
{
//     public class UI_Chat : UI_Popup
//     {
//         private ChatDynamicScroll<ChatItem, UI_Chat_Item> _chatScroll;
//         
//         private CanvasGroup _canvasGroup;
//         private RectTransform _rectTransform;
//         private Transform _sizeUp;
//         private TMP_InputField _inputField;
//         private ChatDynamicScrollRect _chatRect;
//         private List<ChatItem> _chatData = new();
//         
//         private Vector2[] _anchorMin = { new(0, 0), new(0, 0) };
//         private Vector2[] _anchorMax = { new(1, 0), new(1, 1) };
//         private Vector2[] _offsetMin = { new(), new(0, 560.45f) };
//         private Vector2[] _offsetMax = { new(), new(0, 0) };
//
//         private float _keyboardHeight = 0;
//         
//         private bool _isFull = false;
//         private bool _needRefresh = false;
//         
//         private void Start()
//         {
//             Init();
//         }
//
//         public override bool Init()
//         {
//             if (!base.Init()) return false;
//
//             _canvasGroup = transform.GetComponent<CanvasGroup>();
//             _rectTransform = Util.FindChild<RectTransform>(gameObject, "Chat", true);
//             _chatRect = Util.FindChild<ChatDynamicScrollRect>(gameObject, "V_Chat", true);
//             
//             Util.FindChild(gameObject, "IMG_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
//             Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
//             Util.FindChild(gameObject, "B_Enter", true).BindEvent(Functions.TrueCondition, _ => SubmitChat(_inputField.text));
//             _sizeUp = Util.FindChild<Transform>(gameObject, "B_SizeUp", true);
//             _sizeUp.gameObject.BindEvent(Functions.TrueCondition, _ => ChangeSize());
//             Util.FindChild<Slider>(gameObject, "Slider_OpacityBar", true).onValueChanged.AddListener(ChangeOpacity);
//             
//             _inputField = Util.FindChild<TMP_InputField>(gameObject, "T_InputText", true);
//             _inputField.onSelect.AddListener(_ =>
//             {
//                 if (_keyboardHeight == 0)
//                 {
//                     Timing.CallDelayed(1, () =>
//                     {
//                         _keyboardHeight = GetHeightOfKeyboard();
//                         KeyboardActivated(true);
//                     });
//                 }
//                 else
//                 {
//                     KeyboardActivated(true);
//                 }
//             });
//             _inputField.onDeselect.AddListener(_ =>
//             {
//                 KeyboardActivated(false);
//             });
//             _inputField.onSubmit.AddListener(SubmitChat);
//
//             _offsetMin[0] = new Vector2(0f, _rectTransform.offsetMin.y);
//             _offsetMax[0] = new Vector2(0f, _rectTransform.offsetMax.y);
//             
//             _chatScroll = new();
//             _chatScroll.spacing = 0;
//             _chatScroll.Initiate(_chatRect, _chatData, -1, 
//             Manager.Resource.Load<GameObject>("Prefabs/UI/SubItem/UI_Chat_Item"));
//             
//             HiveManager.OnChatReceive += AddChat;
//
//             return true;
//         }
//
//         public void AddChat(string nickname, string message)
//         {
//             _chatData.Add(new ChatItem(nickname, message));
//             if (_chatData.Count > 50) _chatData.RemoveAt(0);
//             if (gameObject.activeSelf) _chatScroll.ChangeList(_chatData);
//             else _needRefresh = true;
//         }
//
//         private void ChangeSize()
//         {
//             _isFull = !_isFull;
//
//             _rectTransform.anchorMin = _anchorMin[_isFull ? 1 : 0];
//             _rectTransform.anchorMax = _anchorMax[_isFull ? 1 : 0];
//             _rectTransform.offsetMin = _offsetMin[_isFull ? 1 : 0];
//             _rectTransform.offsetMax = _offsetMax[_isFull ? 1 : 0];
//             _sizeUp.Rotate(0, 0, _isFull ? -90 : 90);
//         }
//
//         private void OnEnable()
//         {
//             if (_needRefresh)
//             {
//                 _chatScroll.ChangeList(_chatData);
//                 _needRefresh = false; 
//             }     
//         }
//
//         private void KeyboardActivated(bool isActivate)
//         {
//             if (isActivate)
//             {
//                 if (_isFull)
//                 {
//                     var offset = _offsetMin[1];
//                     offset.y = _keyboardHeight;
//                     _rectTransform.offsetMin = offset;
//                 }
//                 else
//                 {
//                     var offset = _offsetMin[0];
//                     offset.y = _keyboardHeight;
//                     _rectTransform.offsetMin = offset;
//                     offset = _offsetMax[0];
//                     offset.y = _keyboardHeight + _offsetMax[0]. y - _offsetMin[0].y;
//                     _rectTransform.offsetMax = offset;
//                 }
//             }
//             else
//             {
//                 _rectTransform.offsetMin = _offsetMin[_isFull ? 1 : 0];
//                 _rectTransform.offsetMax = _offsetMax[_isFull ? 1 : 0];
//             }
//         }
//
//         private void SubmitChat(string input)
//         {
//             if (input.Length == 0) return;
//             _inputField.text = string.Empty;
//             HiveManager.SendMessage(input);
//             _inputField.Select();
//         }
//
//         private void ChangeOpacity(float opacity)
//         {
//             _canvasGroup.alpha = opacity;
//         }
//         
//         
//         public override bool NeedRaycast()
//         {
//             return true;
//         }
//
//         public override void WhenPopupClosed()
//         {
//         }
//
//         private float GetHeightOfKeyboard()
//         {
//             #if UNITY_EDITOR
//             return 560.45f;
//             #elif UNITY_ANDROID
//              using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//              {
//                  var unityPlayer =
//                  unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
//                  var view = unityPlayer.Call<AndroidJavaObject>("getView");
//                  var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
//       
//                  if (view == null || dialog == null)
//                      return 0;
//       
//                  var decorHeight = 0;
//       
//                  var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
//   
//                  if (decorView != null)
//                      decorHeight = decorView.Call<int>("getHeight");
//       
//                  using (var rect = new AndroidJavaObject("android.graphics.Rect"))
//                  {
//                      view.Call("getWindowVisibleDisplayFrame", rect);
//                      return (Screen.height - rect.Call<int>("height") + decorHeight) * Define.Resolution.y / Screen.height;
//                  }
//                  
//              }
// #else
//              var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
//              return height >= Display.main.systemHeight ? 0 : height;
// #endif
//         }
//     }
}