using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Chat;
using UIs.Utils;
using Utils;

namespace UIs.FieldMain
{
    // public class UI_MainChat: UI_Scene
    // {
    //     private TextMeshProUGUI _chatText;
    //     
    //     private void Start()
    //     {
    //         Init();
    //     }
    //
    //     public override bool Init()
    //     {
    //         if (!base.Init()) return false;
    //         
    //         Util.FindChild(gameObject, "IMG_ChatBox", true).BindEvent(Functions.TrueCondition, _ => OpenChatPopup(), UIEffectType.Bounce);
    //         _chatText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Chat", true);
    //         _chatText.text = string.Empty;
    //         
    //         HiveManager.OnChatReceive += SetChat;
    //
    //         return true;
    //     }
    //
    //     private void SetChat(string nickname, string message)
    //     {
    //         _chatText.text = $"{nickname}: {message}";
    //     }
    //
    //     private void OpenChatPopup()
    //     {
    //         Manager.UI.ShowPopupUI<UI_Chat>();
    //     }
    //
    //     public override bool NeedRaycast()
    //     {
    //         return true;
    //     }
    // }
}