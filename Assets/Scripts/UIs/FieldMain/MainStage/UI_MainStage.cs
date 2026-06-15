using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbStage;
using Managers;
using TMPro;
using UIBases;
using UIs.Dungeon.TrainingGround;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.FieldMain.MainStage
{
    public class UI_MainStage: UI_Scene, ILanguageSet
    {
        private float _fullTime;

        private EventsManager _monsterCountManager;
        private EventsManager _killCountManager;
        
        private Vector3 _stageProgressPosition;
        private Vector3 _stageLoopPosition;
        
        enum GameObjects
        {
            CurrentStage,
            StageClear,
            GreenBar,
            RedBar,
            M_ToastMask,
            IMG_ProfileBg,
            B_PromotionExit
        }

        enum Transforms
        {
            B_Next,
            IMG_StageBG
        }

        enum SlicedFilledImages
        {
            IMG_TimeProgressBar,
            IMG_GreenProgress
        }

        enum Sliders
        {
            Slider_RedProgress
        }

        enum Images
        {
            IMG_Recommend,
            IMG_Profile,
            B_Next
        }

        enum Texts
        {
            T_Time,
            T_Mission,
            T_Stage,
            T_Count,
            T_Recommend
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            transform.GetComponent<Canvas>().sortingOrder = 1;

            Bind<GameObject>(typeof(GameObjects));
            Bind<SlicedFilledImage>(typeof(SlicedFilledImages));
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Slider>(typeof(Sliders));
            
            Get<GameObject>((int)GameObjects.CurrentStage).SetActive(false);
            Get<GameObject>((int)GameObjects.StageClear).SetActive(false);

            if (Get<GameObject>((int) GameObjects.M_ToastMask) == null)
            {
                ManualAdd(typeof(GameObject), (int)GameObjects.M_ToastMask, transform.Find("SafeArea").Find("M_ToastMask").gameObject);
                ManualAdd(typeof(TextMeshProUGUI), (int)Texts.T_Mission, transform.Find("SafeArea").Find("M_ToastMask").Find("IMG_Cover").Find("T_Mission").GetComponent<TextMeshProUGUI>());
            }
            
            Get<Transform>((int)Transforms.B_Next).gameObject.BindEvent(Functions.TrueCondition, OnNextStageButtonClicked);
            Get<GameObject>((int)GameObjects.B_PromotionExit).BindEvent(Functions.TrueCondition, OnExitPromotionButtonClicked);
            
            PlayController.data.RedBarProgress.ValueChanged += (_, _) => WhenRedProgressChanged();
            PlayController.data.TimeLimit.ValueChanged += (_, _) => WhenTimeChanged();
            _killCountManager = new EventsManager(this, new EventsManager.Config
            {
                updatedField = new[] {PlayController.data.KillCount},
                handler = WhenGreenProgressChanged
            });
            _monsterCountManager = new EventsManager(this, new EventsManager.Config
            {
                updatedField = new[] {PlayController.data.MonsterCount},
                handler = WhenMonsterCountChanged
            });

            _stageProgressPosition = Get<Transform>((int) Transforms.IMG_StageBG).localPosition;
            _stageLoopPosition = _stageProgressPosition;
            _stageLoopPosition.x = 0;

            WhenGreenProgressChanged();
            WhenTimeChanged();
            WhenRedProgressChanged();

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            return true;
        }

        public void WhenGameOver()
        {
            if (!_isInit) Init();
            Get<GameObject>((int)GameObjects.StageClear).SetActive(false);
            gameObject.SetActive(false);
            _currentMax = 0;
        }

        public void WhenStageChanged()
        {
            if (!_isInit) Init();
            var field = Manager.Field.CurField.Value;

            if (field == FieldType.Training)
            {
                gameObject.SetActive(false);
                Manager.UI.ShowSceneUI<UI_MainStage_Training>();
                return;
            }
            gameObject.SetActive(true);
            
            var useRed = Manager.Field.StageMeta.GetStageType() == StageType.Boss || field == FieldType.Pet 
                || field == FieldType.BlackMarket || field == FieldType.Dia;
            var isDungeon = field != FieldType.Stage && field != FieldType.Promotion; 
            var isStageCleared = LevelController.I.GetIsStageClear();
            // Manager.UI.GetSceneUI<UI_MainSkill>().ResetDamage();

            Get<Transform>((int) Transforms.IMG_StageBG).localPosition =
                isStageCleared ? _stageLoopPosition : _stageProgressPosition;
            
            Get<GameObject>((int)GameObjects.B_PromotionExit).SetActive(field == FieldType.Promotion);
            Get<GameObject>((int)GameObjects.CurrentStage).SetActive(!isStageCleared);
            Get<GameObject>((int)GameObjects.StageClear).SetActive(isStageCleared);
            Get<Image>((int)Images.B_Next).material = Define.GetUIMaterial(isStageCleared && LevelController.data.MaxStage.Value == DbStageLevel.Count); 
            Get<TextMeshProUGUI>((int) Texts.T_Stage).text = Manager.Field.GetStageName();
            var recommend = Manager.Field.StageMeta.GetPower();
            var color = recommend > TotalStatController.Power.Value ? "<color=#E76A6A>" : "<color=#FFF8AA>";
            Get<Image>((int)Images.IMG_Recommend).color = recommend > TotalStatController.Power.Value ? Define.ColorE76A6A : Define.ColorFFF8AA;
            Get<TextMeshProUGUI>((int)Texts.T_Recommend).text = color + Define.AddUnit(recommend, 3, 2) + "</color>";
            Get<GameObject>((int)GameObjects.GreenBar).SetActive(!useRed);
            Get<GameObject>((int)GameObjects.RedBar).SetActive(useRed);
            Get<GameObject>((int)GameObjects.M_ToastMask).SetActive(!isStageCleared);
            Get<GameObject>((int)GameObjects.IMG_ProfileBg).SetActive(useRed || isDungeon);

            var checkKillCount = !useRed;
            if (checkKillCount) _killCountManager.Reconnect();
            else _killCountManager.Dispose();
            
            var checkMonsterCount = field == FieldType.BlackMarket;
            if (checkMonsterCount) _monsterCountManager.Reconnect();
            else _monsterCountManager.Dispose();
            Get<Slider>((int)Sliders.Slider_RedProgress).handleRect.gameObject.SetActive(checkMonsterCount);
            
            if (useRed || isDungeon) Get<Image>((int) Images.IMG_Profile).sprite = 
                Manager.Resource.Load<Sprite>(isDungeon ? "UI_DungeonProfile_" + field : "UI_BossProfile_" + Manager.Field.StageMeta.GetBossResource());

            if (!isStageCleared)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Mission).text =
                    useRed
                        ? LocalString.Get(210020)
                        : string.Format(LocalString.Get(210021), Manager.Field.StageMeta.GetStageGoalCount());
            }
            
            _fullTime = DbStageBase.Get(Manager.Field.StageMeta.GetStageType()).TimeLimit;

            gameObject.SetActive(true);
        }

        private void WhenGreenProgressChanged()
        {
            if (!Get<GameObject>((int)GameObjects.GreenBar).activeSelf) return;
            
            Get<SlicedFilledImage>((int) SlicedFilledImages.IMG_GreenProgress).fillAmount = 1f * PlayController.data.KillCount.Value / Manager.Field.StageMeta.GetStageGoalCount();
            Get<TextMeshProUGUI>((int) Texts.T_Count).text =
                PlayController.data.KillCount.Value + "/" + Manager.Field.StageMeta.GetStageGoalCount();
        }

        private void WhenRedProgressChanged()
        {
            var maxHp = PlayController.data.RedBarMaxProgress;
            if (maxHp == 0) return;
            var hp = PlayController.data.RedBarProgress.Value;
            var percent = (float)hp / (float)maxHp;
            Get<Slider>((int) Sliders.Slider_RedProgress).value = percent;
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = percent.ToString("P0") + " " + Define.AddUnit(hp, 3, 2);
        }

        private int _currentMax = 0;
        private void WhenMonsterCountChanged()
        {
            var count =  PlayController.data.MonsterCount.Value;
            if (_currentMax < count) _currentMax = count;
            Get<Slider>((int) Sliders.Slider_RedProgress).value = _currentMax == 0 ? 0 : 1f * count / _currentMax;
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = string.Format(LocalString.Get(210244), count);

            if (count == 0) _currentMax = 0;
        }
            
        private void WhenTimeChanged()
        {
            var time = PlayController.data.TimeLimit.Value;
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = time.ToString();
            Get<SlicedFilledImage>((int) SlicedFilledImages.IMG_TimeProgressBar).fillAmount = time / _fullTime;
        }
        
        private void OnNextStageButtonClicked(PointerEventData eventData)
        {
            //LevelController.I.GoNextStage(2);
            if (LevelController.data.MaxStage.Value == DbStageLevel.Count)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200057);
            }
            else
            {
                SettingController.I.SetAutoProgress(true);
            }
        }

        private void OnExitPromotionButtonClicked(PointerEventData eventData)
        { 
            Manager.Field.GiveUpDungeon();
        }

        public void RemoveTrainingStage()
        {
            Manager.Resource.Destroy(Manager.UI.GetSceneUI<UI_MainStage_Training>().gameObject);
            var countUI = Manager.UI.GetSceneUI<UI_TrainingGroundCount>();
            if (countUI != null) Manager.Resource.Destroy(countUI.gameObject);
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Stage).text = Manager.Field.GetStageName();
        }
        
    }
}