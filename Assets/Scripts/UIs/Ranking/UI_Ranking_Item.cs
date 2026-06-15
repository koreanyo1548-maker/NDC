using System;
using System.Numerics;
using Controller;
using Controller.Infos;
using Data.DbRecord;
using Data.DbShop;
using Data.DbStage;

using dynamicscroll;
using Managers;
using ThirdParty;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Ranking
{
    public class UI_Ranking_Item: DynamicScrollObject<RankingItem>, ILanguageSet
    {
        private bool _isInit;
        private Sprite[] _rankingSprites;

        private TextMeshProUGUI _rankingText;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _nicknameText;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _valueText;
        private Image _rankingImage;
        private Image _profileImage;

        private CanvasGroup _canvasGroup;
        
        private UI_Ranking _uiRanking;

        private int _title;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _uiRanking = Manager.UI.GetPopupUI<UI_Ranking>();
            _canvasGroup = transform.GetComponent<CanvasGroup>();
            
            _rankingText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Ranking", true);
            _levelText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Level", true);
            _nicknameText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Nickname", true);
            _titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Title", true);
            _valueText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Value", true);
            _rankingImage = Util.FindChild<Image>(gameObject, "IMG_Ranking", true);
            _profileImage = Util.FindChild<Image>(gameObject, "IMG_Profile", true);

            _rankingSprites = new[]
            {Manager.Resource.Load<Sprite>("UI_Rankicon_1"), 
                Manager.Resource.Load<Sprite>("UI_Rankicon_2"), 
                Manager.Resource.Load<Sprite>("UI_Rankicon_3")};
            _canvasGroup.enabled = false;
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            _isInit = true;
        }

        public override void UpdateScrollObject(RankingItem ranking, int index)
        {
            if (!_isInit) Init();
            base.UpdateScrollObject(ranking, index);

            var data = ranking.type == RankingType.Stage ? _uiRanking.GetStageOf(index)
                    : _uiRanking.GetTrainingOf(index);

            if (data == null)
            {
                _canvasGroup.enabled = true;
            }
            else
            {
                _canvasGroup.enabled = false;
                var rankingNum = index + 1;
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
                _nicknameText.text = data.DisplayName;

                if (ranking.level == 0)
                {
                    GetAndSetRankingInfo();
                }
                else
                {
                    SetInfo();
                }
            }

            void SetInfo()
            {       
                _title = ranking.title;
                _levelText.text = string.Format(LocalString.Get(210041), ranking.level);
                _titleText.text = ranking.title == 0 ? LocalString.Get(210160) : DbTitle.Get(ranking.title).GetNameWithColor();
                _profileImage.sprite = Manager.Resource.Load<Sprite>(DbProfile.Get(ranking.profile).Resource);

                BigInteger bint = data.StatValue.ToBigInt();
                //Debug.Log($"SetInfo: {data.StatValue} => {bint}");
                _valueText.text = ranking.type == RankingType.Stage ? data.StatValue.ToString() : 
                                    Define.AddUnit(bint, 3, 2);//Decode(data.StatValue), 3, 2);
            }

            void GetAndSetRankingInfo()
            {
                PlayFabManager.Leaderboard.GetRankingInfo(data.PlayFabId, (level, title, profile, stage, training) =>
                {
                    ranking.Set(level, title, profile);
                    SetInfo();
                });
            }
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
    public class RankingItem
    {
        public RankingType type;
        public int level;
        public int title;
        public int profile;

        public void Set(int level, int title, int profile)
        {
            this.level = level;
            this.title = title;
            this.profile = profile;
            if (this.profile == 0) this.profile = 1;
        }
    }

    public enum RankingType
    {
        Power,
        Stage,
        Training
    }
}