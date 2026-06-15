using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbEvent;
using DG.Tweening.Core.Easing;
using dynamicscroll;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Friend
{
    public class UI_Friend: UI_Popup
    {
        private DynamicScroll<FriendItem, UI_Friend_Item> _friendRect;
        
        private Dictionary<string, int> _friendRankings = new();
        
        private List<FriendItem> _friendData = new();
        private List<FriendItem> _friendDataSave = new();

        private GameObject _noFriend;

        private GameObject _addFriendUI;
        private TMP_InputField _addFriendInputField;
        
        private GameObject _removeFriendUI;
        
        private SlicedFilledImage _expImg;
        private TextMeshProUGUI _expText;
        private Image _rewardImg;
        private bool _canReward;
        
        private CoroutineHandle _refreshRoutine;
        private bool _shouldRefresh;

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _addFriendUI = Util.FindChild(gameObject, "UI_AddFriend", true);
            _addFriendInputField = Util.FindChild<TMP_InputField>(_addFriendUI, "I_FriendInputField", true);

            _removeFriendUI = Util.FindChild(gameObject, "UI_RemoveFriend", true);
            Util.FindChild(_addFriendUI, "IMG_DimmedAdd", true).BindEvent(Functions.TrueCondition, 
                _ => _addFriendUI.gameObject.SetActive(false), UIEffectType.None, false);
            Util.FindChild(_addFriendUI, "B_AddFriend", true).BindEvent(Functions.TrueCondition, _ => AddFriend(), UIEffectType.Bounce);

            Util.FindChild(_removeFriendUI, "B_CloseRemoveFriend", true).BindEvent(Functions.TrueCondition,
                _ => _removeFriendUI.gameObject.SetActive(false));
            Util.FindChild(_removeFriendUI, "IMG_DimmedRemove", true).BindEvent(Functions.TrueCondition,
                _ => _removeFriendUI.gameObject.SetActive(false));
            Util.FindChild(_removeFriendUI, "B_RemoveFriend", true).BindEvent(Functions.TrueCondition,
                _ => RemoveFriend());

            _addFriendUI.SetActive(false);
            _removeFriendUI.SetActive(false);
            
            Timing.CallDelayed(Timing.DeltaTime, () =>
            {
                foreach (var friend in FriendController.I.Friends)
                {
                    _friendDataSave.Add(new FriendItem {id = friend});
                }

                _friendData = _friendDataSave.Clone();

                _friendRect = new DynamicScroll<FriendItem, UI_Friend_Item>();
                _friendRect.Initiate(Util.FindChild<DynamicScrollRect>(gameObject, "V_Friend", true), _friendData,
                    -1, "Prefabs/UI/SubItem/UI_Friend_Item");

                Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(),
                    UIEffectType.None, false);
                Util.FindChild(gameObject, "B_Add", true).BindEvent(Functions.TrueCondition, 
                _ => _addFriendUI.gameObject.SetActive(true), UIEffectType.Bounce, false);
                
                Util.FindChild(gameObject, "B_Invite", true)
                    .BindEvent(Functions.TrueCondition, _ => InviteFriend(), UIEffectType.Bounce);
                

                _noFriend = Util.FindChild(gameObject, "T_NoFriend", true);

                _expImg = Util.FindChild<SlicedFilledImage>(gameObject, "IMG_Exp", true);
                _expText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Exp", true);
                _rewardImg = Util.FindChild<Image>(gameObject, "B_Reward", true);
                Util.FindChild(gameObject, "B_Reward", true).BindEvent(() => _canReward, _ => GetReward(), UIEffectType.Bounce);
                
                _refreshRoutine = Timing.RunCoroutine(_RefreshRoutine());
                CheckReward();
                SetNoFriend();
            });
            return true;
        }

        private void SetNoFriend()
        {
            _noFriend.SetActive(FriendController.I.Friends.Count == 0);
        }

        private void OnEnable()
        {
            if (_shouldRefresh)
            {
                Timing.CallDelayed(Timing.DeltaTime, () =>
                {
                    _friendRankings.Clear();
                    _friendData = _friendDataSave.Clone();
                    _friendRect.ChangeList(_friendData, 0);
                    _shouldRefresh = false;
                    Timing.KillCoroutines(_refreshRoutine);
                    _refreshRoutine = Timing.RunCoroutine(_RefreshRoutine());
                });
            }
        }

        IEnumerator<float> _RefreshRoutine()
        {
            yield return Timing.WaitForSeconds(300);
            _shouldRefresh = true;
        }

        private void AddFriend()
        {
            if (FriendController.I.Friends.Count >= 30)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText("이미 최대 친구 수를 가지고 있습니다.");
                return;
            }
            
            var id = _addFriendInputField.text;

            if (id.Equals(SettingController.UID))
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText("본인은 친구로 추가할 수 없습니다.");
                return;
            }
                
            
            PlayFabManager.Leaderboard.CheckIdIsValid(id, IfValid, IfNotValid);

            void IfValid()
            {
                FriendController.I.Friends.Add(id);
                _friendDataSave.Add(new FriendItem {id = id});
                _shouldRefresh = true;
                OnEnable();
                _addFriendUI.SetActive(false);
                _addFriendInputField.text = string.Empty;
                CheckReward();
                SetNoFriend();
            }

            void IfNotValid()
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText("존재하지 않는 UID 입니다.");
            }
        }

        private string _removeId;
        public void TryRemove(string id)
        {
            _removeId = id;
            _removeFriendUI.SetActive(true);
        }

        private void RemoveFriend()
        {
            _removeFriendUI.SetActive(false);
            FriendController.I.Friends.Remove(_removeId);
            _friendDataSave.RemoveAt(_friendDataSave.FindIndex(f => f.id == _removeId));
            _shouldRefresh = true;
            OnEnable();
            CheckReward();
            SetNoFriend();
        }

        private void GetReward()
        {
            var lastRewarded = FriendController.I.FriendRewarded;
            var reward = DbFriendReward.Get(lastRewarded + 1);
            
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            CurrencyController.I.GetReward(reward.RewardType, reward.RewardCount, reward.RewardId);
            if (reward.RewardType == CurrencyType.Dia) CurrencyController.I.SetDiaLog($"친구보상 {reward.Id} 보상", reward.RewardCount, prev);

            FriendController.I.FriendRewarded = reward.Id;
            
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbReward>();
            rewardsForToast.Add(new DbReward(reward.RewardType, reward.RewardCount, reward.RewardId));
            toast.SetReward(210031, rewardsForToast);
            
            CheckReward();
        }

        private void CheckReward()
        {
            var lastRewarded = FriendController.I.FriendRewarded;
            var nextReward = DbFriendReward.Get(lastRewarded + 1);
            if (nextReward == null)
            {
                _rewardImg.material = Define.GetUIMaterial(true);
                _rewardImg.GetComponent<Animator>().enabled = false;
                _rewardImg.GetComponent<UIShiny>().enabled = false;
                _expText.text = "MAX";
                _expImg.fillAmount = 1;
                _canReward = false;
            }
            else
            {
                var curFriendCount = FriendController.I.Friends.Count;
                _canReward = curFriendCount >= nextReward.NeedCount;
                _expText.text = curFriendCount + "/" + nextReward.NeedCount;
                _expImg.fillAmount = 1f * curFriendCount / nextReward.NeedCount;
                _rewardImg.material = Define.GetUIMaterial(!_canReward);
                _rewardImg.GetComponent<Animator>().enabled = _canReward;
                _rewardImg.GetComponent<UIShiny>().enabled = _canReward;
            }
        }

        private void InviteFriend()
        {
            new NativeShare().SetUrl("onelink.to/99ksb2").Share();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}