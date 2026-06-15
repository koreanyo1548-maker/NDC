using System;
using System.Numerics;
using Coffee.UIEffects;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbDungeon;
using Data.DbStage;
using DG.Tweening;
using Managers;
using TMPro;
using UIBases;
using UIs.Dungeon.TrainingGround;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace UIs.FieldMain.MainStage
{
    public class UI_MainStage_Training: UI_Scene
    {
        private EventsManager _timeManager;
        private EventsManager _damageManager;
        
        private float _timeLimit;
        private BigInteger _nextLevelDamage;
        private int _curLevel;
        private int _prevLevel; // 보스 아이템 연출을 위해서만 사용
        private bool _giveItemAnimating; // 보스 아이템 연출 대기중
        private bool _isMax;
        private bool _isNewRecord;
        
        enum Texts
        {
            T_PreviousDamageNum,
            T_CurrentDamageNum,
            T_NeedDamage,
            T_Level,
            T_Time
        }

        enum UIShinies
        {
            IMG_LevelBG
        }

        enum SlicedFilledImages
        {
            IMG_TimeProgressBar,
            IMG_GreenProgress
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            transform.GetComponent<Canvas>().sortingOrder = 1;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<SlicedFilledImage>(typeof(SlicedFilledImages));
            Bind<UIShiny>(typeof(UIShinies));
            
            Util.FindChild(gameObject, "B_RewardInfo", true).BindEvent(Functions.TrueCondition, _ => OpenRewardInfo(), UIEffectType.Bounce);

            _timeLimit = DbStageBase.Get(StageType.Training).TimeLimit;
            
            _timeManager = new EventsManager(this, new EventsManager.Config
            {
                updatedField = new[] {PlayController.data.TimeLimit},
                handler = WhenTimeChanged
            });
            _damageManager = new EventsManager(this, new EventsManager.Config
            {
                updatedController = new[] {PlayController.I.damage},
                handler = WhenDamageChanged
            });
            
            Set();
            
            return true;
        }

        private void Set()
        {
            SetPrevDamage();
            SetLevel();
            WhenTimeChanged();
        }
        
        private void SetPrevDamage()
        {
            Get<TextMeshProUGUI>((int)Texts.T_PreviousDamageNum).text = Define.AddUnit(LevelController.data.MaxTraining.Value, 3, 2);
            Get<TextMeshProUGUI>((int)Texts.T_CurrentDamageNum).text = "0";
            Get<TextMeshProUGUI>((int)Texts.T_NeedDamage).text = Define.AddUnit(DbTrainingGroundLevel.Get(1).Damage, 3, 2);
        }

        private Vector3 _levelUpParticleScale = new(0.254f, 0.254f, 0.254f);
        private static readonly int giveItem = Animator.StringToHash("GiveItem");

        public void WhenDamageChanged()
        {
            var damage = PlayController.I.damage.Value;
            Get<TextMeshProUGUI>((int) Texts.T_CurrentDamageNum).text = Define.AddUnit(damage, 3, 2);
            while (!_isMax && damage > _nextLevelDamage)
            {
                _curLevel++;
                SetLevel();
                WhenLevelUp();

                // 보상이 있는 경우 몬스터 연출 및 리워드 로그
                if (_curLevel > LevelController.data.TrainingGroundStage.Value)
                {
                    var reward = DbTrainingGroundReward.Get(_curLevel);
                    Manager.UI.RewardLog.Add(reward.RewardType, reward.RewardCount, reward.RewardId);
                    var monster = Manager.Field.GetFirst();
                    if (monster != null && !_giveItemAnimating)
                    {
                        _giveItemAnimating = true;
                        monster.gameObject.GetOrAddComponent<AnimationEventSetter>().SetAction(ShowRewards);
                        monster.animator.SetTrigger(giveItem);
                    }
                }
                
                // 최대레벨이면 더이상 데미지 체크 안하도록
                if (_curLevel == DbTrainingGroundLevel.Count)
                {
                    _isMax = true;
                }
            }

            if (!_isNewRecord && damage > LevelController.data.MaxTraining.Value)
            {
                _isNewRecord = true;
                Get<TextMeshProUGUI>((int) Texts.T_CurrentDamageNum).transform
                    .DOPunchScale(new (0.5f, 0.5f, 0.5f), 1, 7, 0.802f).SetEase(Ease.OutQuad).SetLoops(1);
            }

            var needDamage = DbTrainingGroundLevel.Get(_curLevel+1).Damage;
            var prevNeedDamage = _curLevel == 0 ? 1 : DbTrainingGroundLevel.Get(_curLevel).Damage;
            Get<TextMeshProUGUI>((int)Texts.T_NeedDamage).text = Define.AddUnit(needDamage - damage, 3, 2);
            Get<SlicedFilledImage>((int)SlicedFilledImages.IMG_GreenProgress).fillAmount = 1 - (float)(damage-prevNeedDamage) / (float)(needDamage - prevNeedDamage);
            
            void WhenLevelUp()
            {
                Get<UIShiny>((int)UIShinies.IMG_LevelBG).Play();
                
                var particleObj = Manager.Resource.InstantiateParticle("Particles/LevelUpCylinderGreen", 1,  Manager.Player.Root);
                particleObj.transform.localPosition = Define.Zero3;
                particleObj.transform.localScale = _levelUpParticleScale;
                var particle = particleObj.GetComponent<ParticleSystem>();
                particle.Simulate( 0.0f, true, true );
                particle.Play();
            }

            void ShowRewards()
            {
                var max = 15 / (_curLevel - _prevLevel);
                for (var idx = _prevLevel+1; idx <= _curLevel; ++idx)
                {
                    var reward = DbTrainingGroundReward.Get(idx);
                    var needLong = reward.RewardCount <= 5;
                    var count = needLong ? 5 : Random.Range(Math.Min(5, max), Math.Min(7, max));
                    while (count-- > 0)
                    {
                        Manager.Resource.Instantiate("Particles/RewardItem", 3, Manager.EffectParent)
                            .GetOrAddComponent<TrainingRewardItem>().Set(reward, needLong);
                    }
                }
                _prevLevel = _curLevel;
                _giveItemAnimating = false;
            }
        }

        private void SetLevel()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _curLevel);
            if (_curLevel == DbTrainingGroundLevel.Count) return;
            _nextLevelDamage = DbTrainingGroundLevel.Get(_curLevel+1).Damage;
        }

        private void WhenTimeChanged()
        {
            var time = PlayController.data.TimeLimit.Value;
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = string.Format(LocalString.Get(210373), time.ToString());
            Get<SlicedFilledImage>((int) SlicedFilledImages.IMG_TimeProgressBar).fillAmount = time / _timeLimit;

            if (time == 10)
            {
                Manager.UI.ShowSceneUI<UI_TrainingGroundCount>().Count(10, () => {});
            }
        }
        
        private void OpenRewardInfo()
        {
            Manager.UI.ShowPopupUI<UI_TrainingGroundReward>().Set();
        }

        private void OnEnable()
        {
            _curLevel = 0;
            _prevLevel = LevelController.data.TrainingGroundStage.Value;
            _isNewRecord = false;
            _timeManager?.Reconnect();
            _damageManager?.Reconnect();
            if (_isInit)
            {
                Set();
                Get<TextMeshProUGUI>((int) Texts.T_CurrentDamageNum).transform.localScale = Define.One;
            }
        }

        private void OnDisable()
        {
            _timeManager?.Dispose();
            _damageManager?.Dispose();
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}