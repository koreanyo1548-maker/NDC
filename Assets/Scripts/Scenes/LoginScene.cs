using Controller.Play;
using Data.DbAbility;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbDungeon;
using Data.DbEquipment;
using Data.DbEvent;
using Data.DbForbiddenInfo;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbRecord;
using Data.DbRelicInfo;
using Data.DbShop;
using Data.DbStage;
using Data.DbSummon;
using Data.DbUser.Etc;
using ThirdParty;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#elif UNITY_IOS
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif


namespace Scenes
{
    public class LoginScene: MonoBehaviour
    {
        [SerializeField]
        string webClientId;

#if UNITY_IOS
        IAppleAuthManager _appleAuthManager;
#endif

        void Awake()
        {
            if (SystemInfo.systemMemorySize < 2000) QualitySettings.globalTextureMipmapLimit = 2;
            else QualitySettings.globalTextureMipmapLimit = 0;

            new DbUserSetting().Set();
            SettingController.I.Init();
            // new GameObject().AddComponent<GoogleInAppUpdate>();

            new DbForbidden().Load();
            #region Character
            
            new DbGoldStat().Load();
            new DbAttackLevel().Load();
            new DbCharacterLevel().Load();
            new DbCriticalAttackBonusLevel().Load();
            new DbCriticalProbabilityLevel().Load();
            new DbHpLevel().Load();
            new DbLevelPoint().Load();
            new DbAttackBonusLevel().Load();
            new DbHpBonusLevel().Load();
            new DbBossAttackBonusLevel().Load();
            
            #endregion
            
            #region Definition
            
            new DbCost().Load();
            new DbCurrency().Load();
            new DbDungeonMeta().Load();
            new DbGrade().Load();
            new DbLock().Load();
            new DbPlay().Load();
            new DbStat().Load();
            
            #endregion
            
            #region Equipment
            
            new DbAccessory().Load();
            new DbAccessoryAwakening().Load();
            new DbAwakeningMaterial().Load();
            new DbGrowthMaterial().Load();
            new DbSkill().Load();
            new DbSkillAwakening().Load();
            new DbSkillDecomposition().Load();
            new DbWeapon().Load();
            new DbWeaponAwakening().Load();
            
            #endregion
            
            #region NecklaceInfo
            
            new DbNecklace().Load();
            new DbNecklaceAwakening().Load();
            new DbNecklaceAwakeningMaterial().Load();
            new DbNecklaceEquipStat().Load();
            new DbNecklaceGrowthMaterial().Load();
            new DbNecklaceOwnStat().Load();

            #endregion
            
            #region PetInfo
            
            new DbBibleLevel().Load();
            new DbPet().Load();
            new DbPetAwakening().Load();
            new DbPetAwakeningMaterial().Load();
            new DbBook().Load();
            new DbPetGrowthMaterial().Load();
            
            #endregion
            
            #region Promote
            
            new DbPromotion().Load();
            new DbPromotionDungeon().Load();
            
            #endregion
            
            #region Record
            
            new DbQuest().Load();
            new DbGuideQuest().Load();
            new DbMainQuest().Load();
            new DbTitle().Load();
            new DbNewbieQuest().Load();
            
            #endregion
            
            #region Shop
            
            new DbAdBuff().Load();
            new DbCoupon().Load();
            new DbPassShop().Load();
            new DbInGameShop().Load();
            new DbInAppShop().Load();
            new DbBlackMarket().Load();
            new DbLevelPass().Load();
            new DbStagePass().Load();
            new DbSoulPass().Load();
            new DbCostume().Load();
            new DbProfile().Load();
            
            #endregion

            #region Event

            new DbAttendReward().Load();
            new DbAttendEvent().Load();
            new DbAttendEventReward().Load();
            new DbSeasonPass().Load();
            new DbSeasonPassQuest().Load();
            new DbSeasonPassReward().Load();
            new DbDropEvent().Load();
            new DbDropEventShop().Load();
            new DbFriendReward().Load();
            
            #endregion
            
            #region Stage
            
            new DbMonster().Load();
            new DbStageBase().Load();
            new DbStageLevel().Load();
            new DbStageReward().Load();
            
            #endregion

            #region Dungeon
            
            new DbAwakeningDungeonReward().Load();
            new DbAwakeningDungeonLevel().Load();
            new DbSkillGrowthDungeonLevel().Load();
            new DbSkillGrowthDungeonReward().Load();
            new DbPetDungeonLevel().Load();
            new DbPetDungeonReward().Load();
            new DbBlackMarketDungeonLevel().Load();
            new DbBlackMarketDungeonReward().Load();
            new DbDiaDungeonLevel().Load();
            new DbDiaDungeonReward().Load();
            new DbTrainingGroundLevel().Load();
            new DbTrainingGroundReward().Load();
            
            #endregion

            #region Summon
            
            new DbSummonGradeProbability().Load();
            new DbSummonNumberProbability().Load();
            new DbSummonSkillProbability().Load();
            new DbSummonNecklaceProbability().Load();
            new DbSummonProduct().Load();
            new DbSummonLevel().Load();
            new DbSummonRelicProbability().Load();
            
            #endregion
            
            #region Ability
            
            new DbAbilityOption().Load();
            new DbAbilityOptionSummon().Load();
            new DbAbilityRune().Load();
            
            #endregion
            
            #region Relic
            
            new DbRelic().Load();
            new DbRelicLevel().Load();
            new DbRelicGrowthProbability().Load();

            #endregion

            #region PlayFab
            
            PlayFabManager.Init();

            #endregion

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void Start()
        {
            Application.targetFrameRate = 60;

#if UNITY_EDITOR
            PlayFabManager.ManualGuestLogin(SystemInfo.deviceUniqueIdentifier);
#else
            onLogin();
#endif
        }
#if UNITY_IOS
        void Update()
        {
            if (_appleAuthManager != null)
                _appleAuthManager.Update();
        }
#endif
        void onLogin()
        {
#if UNITY_ANDROID
            onLogin_GPGS();
#elif UNITY_IOS
            onLogin_Apple();
#endif
        }
        /// <summary>
        /// 로그인 - 구글 플레이 게임 서비스
        /// </summary>
        public void onLogin_GPGS()
        {
#if UNITY_ANDROID
            var pgp = PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    var name = PlayGamesPlatform.Instance.GetUserDisplayName();
                    var id = PlayGamesPlatform.Instance.GetUserId();
                    var url = PlayGamesPlatform.Instance.GetUserImageUrl();

                    Debug.Log($"Success {name}, {id}, {url}");
                    PlayGamesPlatform.Instance.RequestServerSideAccess(false, (authCode) =>
                    {
                        Debug.Log($"LoginWithGoogle {authCode}");
                        if (string.IsNullOrEmpty(authCode) == false)
                        {
                            PlayerPrefs.SetInt("Platform", (int)Data.LoginPlatform.GooglePlayGames);
                            PlayFabManager.LoginWithGooglePlayGamesServices(authCode);
                        }
                        else
                        {
                            Debug.LogWarning("GPGS authCode empty — falling back to guest login");
                            PlayFabManager.ManualGuestLogin(SystemInfo.deviceUniqueIdentifier);
                        }
                    });
                }
                else
                {
                    Debug.LogWarning($"GPGS auth failed ({status}) — falling back to guest login");
                    PlayFabManager.ManualGuestLogin(SystemInfo.deviceUniqueIdentifier);
                }
            });
#endif
        }
        /// <summary>
        /// 로그인 - 애플
        /// </summary>
        public void onLogin_Apple()
        {
            Debug.Log("onLogin_Apple");

#if UNITY_IOS
            string identityToken = PlayerPrefs.GetString("appleToken", "");
            if (string.IsNullOrEmpty(identityToken))
            {
                var deserializer = new PayloadDeserializer();
                _appleAuthManager = new AppleAuthManager(deserializer);

                var loginArgs = new AppleAuthLoginArgs(LoginOptions.None);
                _appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    Debug.Log($"애플 로그인 성공: {credential.User}");
                    var appleIdCredential = credential as IAppleIDCredential;
                    var passwordCredential = credential as IPasswordCredential;

                    if (appleIdCredential.State != null)
                        Debug.Log($"State: {appleIdCredential.State}");

                    if (appleIdCredential.IdentityToken != null)
                        identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
                    if (string.IsNullOrEmpty(identityToken))
                    {
                        Debug.LogError($"identityToken Empty");
                        return;
                    }
                    Debug.Log($"identityToken: {identityToken}");

                    PlayerPrefs.SetString("appleToken", identityToken);
                    PlayerPrefs.SetInt("Platform", (int)Data.LoginPlatform.Apple);
                    PlayFabManager.LoginWithApple(identityToken);
                },
                    error =>
                    {
                        Debug.LogWarning("Sign in with Apple failed — falling back to guest login");
                        var authorizationErrorCode = error.GetAuthorizationErrorCode();
                        Debug.LogWarning("Apple error: " + authorizationErrorCode.ToString() + " " + error.ToString());
                        PlayFabManager.ManualGuestLogin(SystemInfo.deviceUniqueIdentifier);
                    });
            }
            else
            {
                PlayFabManager.LoginWithApple(identityToken);
            }
#endif
        }
    }
}