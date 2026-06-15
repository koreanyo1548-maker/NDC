using System;
using System.Numerics;
using Data.DbRecord;
using Data.DbShop;
using dynamicscroll;
using Managers;
using ThirdParty;
using TMPro;
using UIs.Ranking;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Friend
{
    public class UI_Friend_Item: DynamicScrollObject<FriendItem>, ILanguageSet
    {
        private bool _isInit;
        private Sprite[] _rankingSprites;

        private TextMeshProUGUI _rankingText;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _nicknameText;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _stageText;
        private TextMeshProUGUI _trainingText;
        private Image _rankingImage;
        private Image _profileImage;

        private CanvasGroup _canvasGroup;
        
        private UI_Friend _uiFriend;
        private FriendItem _friend;

        private int _title;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _uiFriend = Manager.UI.GetPopupUI<UI_Friend>();
            _canvasGroup = transform.GetComponent<CanvasGroup>();
            
            _rankingText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Ranking", true);
            _levelText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Level", true);
            _nicknameText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Nickname", true);
            _titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Title", true);
            _stageText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Stage", true);
            _trainingText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Training", true);
            _rankingImage = Util.FindChild<Image>(gameObject, "IMG_Ranking", true);
            _profileImage = Util.FindChild<Image>(gameObject, "IMG_Profile", true);
            
            Util.FindChild(gameObject, "B_Remove", true).BindEvent(Functions.TrueCondition, _ => Remove(), UIEffectType.Bounce);

            _rankingSprites = new[]
            {Manager.Resource.Load<Sprite>("UI_Rankicon_1"), 
                Manager.Resource.Load<Sprite>("UI_Rankicon_2"), 
                Manager.Resource.Load<Sprite>("UI_Rankicon_3")};
            _canvasGroup.enabled = false;
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            _isInit = true;
        }

        public override void UpdateScrollObject(FriendItem friend, int index)
        {
            if (!_isInit) Init();
            base.UpdateScrollObject(friend, index);

            _friend = friend;
            _canvasGroup.enabled = friend.ranking == 0;
            
            if (friend.level == 0)
            {
                GetAndSetRankingInfo();
            }
            else
            {
                SetInfo();
            }

            void SetInfo()
            {       
                _title = friend.title;
                _levelText.text = string.Format(LocalString.Get(210041), friend.level);
                _titleText.text = friend.title == 0 ? LocalString.Get(210160) : DbTitle.Get(friend.title).GetNameWithColor();
                _profileImage.sprite = Manager.Resource.Load<Sprite>(DbProfile.Get(friend.profile).Resource);
                _stageText.text = friend.stage.ToString();
                _trainingText.text = Define.AddUnit(Decode(friend.training), 3, 2);
            }

            void SetRanking()
            {
                _nicknameText.text = friend.nickname;
                
                var rankingNum = friend.ranking;
                var useImage = rankingNum < 4;

                _rankingText.gameObject.SetActive(!useImage);
                _rankingImage.gameObject.SetActive(useImage);
                if (useImage)
                {
                    _rankingImage.sprite = _rankingSprites[rankingNum - 1];
                }
                else
                {
                    _rankingText.text = rankingNum.ToString();
                }

                _canvasGroup.enabled = false;
            }

            void GetAndSetRankingInfo()
            {
                PlayFabManager.Leaderboard.GetRankingInfo(friend.id, (level, title, profile, stage, training) =>
                {
                    friend.Set(level, title, profile, stage, training);
                    SetInfo();
                });

                PlayFabManager.Leaderboard.GetRankingOf(friend.id, leaderboard =>
                {
                    friend.Set(leaderboard.DisplayName, leaderboard.Position + 1);
                    SetRanking();
                });
            }
        }

        private void Remove()
        {
            _uiFriend.TryRemove(_friend.id);
        }
        
        public BigInteger Decode(int num)
        {
            var decoded = Math.Floor( num / 100000000f);
            var mantissa = (num - decoded * 100000000) / 10000000;
            return (BigInteger)(mantissa * Math.Pow(10, decoded));
        }

        public void OnLanguageChanged(Locale locale)
        {
            _titleText.text = _title == 0 ? LocalString.Get(210160) : DbTitle.Get(_title).GetNameWithColor();
        }
    }

    [Serializable]
    public class FriendItem
    {
        public string id;
        public string nickname;
        public int ranking;
        public int level;
        public int title;
        public int profile;
        public int stage;
        public int training;

        public void Set(int level, int title, int profile, int stage, int training)
        {
            this.level = level;
            this.title = title;
            this.profile = profile;
            this.stage = stage;
            this.training = training;
            if (this.profile == 0) this.profile = 1;
        }

        public void Set(string nickname, int ranking)
        {
            this.nickname = nickname;
            this.ranking = ranking;
        }
    }
}