using dynamicscroll;
using TMPro;
using UIBases;
using Utils;

namespace UIs.Chat
{
    public class UI_Chat_Item : DynamicScrollObject<ChatItem>
    {
        private TextMeshProUGUI _nicknameText;
        private TextMeshProUGUI _messageText;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _nicknameText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true);
            _messageText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_ChatMessage", true);
        }

        public override void UpdateScrollObject(ChatItem chat, int index)
        {
            base.UpdateScrollObject(chat, index);

            _nicknameText.text = chat.nickname;
            _messageText.text = chat.message;
        } 
    }
    
    public class ChatItem
    {
        public string nickname;
        public string message;

        public ChatItem(string nickname, string message)
        {
            this.nickname = nickname;
            this.message = message;
        }
    }
}