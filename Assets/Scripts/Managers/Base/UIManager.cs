using System.Collections.Generic;
using Cameras;
using Controller;
using Controller.Have;
using Controller.Play;
using Data;
using DG.Tweening;
using MEC;
using TMPro;
using UIBases;
using UIs.Character;
using UIs.Character.Ability;
using UIs.Dungeon.LevelEntrance;
using UIs.Dungeon.StageEntrance;
using UIs.Dungeon.TrainingGround;
using UIs.Etc;
using UIs.Etc.Warning;
using UIs.Guide;
using UIs.RewardLog;
using UIs.Summon;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers.Base
{
    public class UIManager
    {
        private int _order = 20;
        private int _tmpAddedOrder = 0;

        private List<UI_Popup> _popups = new();
        private List<UI_Scene> _sceneUIs = new();
        private Dictionary<string, UI_Scene> _singleUIs = new();

        private float _matchWidthOrHeight;

        public int Order => _order;
        public GameObject Root
        {
            get
            {
                if (_root == null)
                {
                    _root = GameObject.Find("@UI_Root");
                    if (_root == null)
                    {
                        _root = new GameObject {name = "@UI_Root"};
                    }

                    _matchWidthOrHeight = Define.GetCanvasRatio();
                }
                return _root;
            }
        }
        private GameObject _root;
        
        
        private Camera UICamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
                }
                return _uiCamera;
            }
        }
        private Camera _uiCamera;
        
        public UI_RewardLog RewardLog
        {
            get
            {
                if (_rewardLog == null) _rewardLog = GetSceneUI<UI_RewardLog>();
                return _rewardLog;
            }
        }
        private UI_RewardLog _rewardLog;
        
        public int PopupCount => _popups.Count;


        public void SetPowerSavingTimer(bool doNotRestart = false)
        {
            Timing.KillCoroutines(Define.KillPowerSavingTimer);
            if (!doNotRestart) Timing.RunCoroutine(_PowerSavingRoutine(), Define.KillPowerSavingTimer);
        }

        public void StartPowerSaving()
        {
            var go = Manager.Resource.Instantiate("UI/Popup/UI_PowerSaving", 1, Root.transform);
            go.GetOrAddComponent<UI_PowerSaving>();
            go.SetActive(true);
        }

        private IEnumerator<float> _PowerSavingRoutine()
        {
            if (SettingController.data.IsAutoPowerSaving.Value)
            {   
                yield return Timing.WaitForSeconds(300);
                if (SettingController.data.IsAutoPowerSaving.Value) StartPowerSaving();
            }
        }

        public void SetCanvas(GameObject go, bool needRaycast, bool sort = true)
        {
            var canvas = Util.GetOrAddComponent<Canvas>(go);
            if (needRaycast) Util.GetOrAddComponent<GraphicRaycaster>(go);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal |
                                              AdditionalCanvasShaderChannels.Tangent |
                                              AdditionalCanvasShaderChannels.TexCoord1;
            canvas.overrideSorting = true;

            var scaler = Util.GetOrAddComponent<CanvasScaler>(go);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Define.Resolution;
            scaler.matchWidthOrHeight = _matchWidthOrHeight;
            if (!sort) canvas.sortingOrder = 0;

            if (go.transform.Find("SafeArea") != null)
            {
                var safeArea = Screen.safeArea;
                var anchorMin = safeArea.position;
                var anchorMax = safeArea.position + safeArea.size;
            
                var rect = go.transform.Find("SafeArea").GetComponent<RectTransform>();
                 anchorMin.x = rect.anchorMin.x;
                 anchorMax.x = rect.anchorMax.x;
                
                 anchorMin.y /= Screen.height;
                 anchorMax.y /= Screen.height;
                
                 rect.anchorMin = anchorMin;
                 rect.anchorMax = anchorMax;

                 if (go.transform.Find("TopUnsafeArea") != null)
                 {
                     var unsafeRect = go.transform.Find("TopUnsafeArea").GetComponent<RectTransform>();
                     anchorMin.y = anchorMax.y;
                     anchorMax.y = 1;
                     unsafeRect.anchorMin = anchorMin;
                     unsafeRect.anchorMax = anchorMax;
                 }
                 
                 if (go.transform.Find("BottomUnsafeArea") != null)
                 {
                     var unsafeRect = go.transform.Find("BottomUnsafeArea").GetComponent<RectTransform>();
                     anchorMax.y = anchorMin.y;
                     anchorMin.y = 0;
                     unsafeRect.anchorMin = anchorMin;
                     unsafeRect.anchorMax = anchorMax;
                 }
            }
        }
        public T ShowSceneUI<T>(string name = null) where T : UI_Scene
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            var go = Manager.Resource.Instantiate($"UI/Scene/{name}", 1, Root.transform);
            var sceneUI = Util.GetOrAddComponent<T>(go);
            _sceneUIs.Add(sceneUI);

            return sceneUI;
        }
        
        public void CloseSingleUI(UI_Scene scene)
        {
            Manager.Resource.Destroy(scene.gameObject);
        }

        public T ShowSingleUI<T>(string name = null) where T : UI_Scene
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            if (_singleUIs.TryGetValue(name, out var targetUI))
            {
                var ui = targetUI.gameObject.GetOrAddComponent<T>();
                ui.gameObject.SetActive(true);
                return ui;
            }

            var go = Manager.Resource.Instantiate($"UI/Scene/{name}", 1, Root.transform);
            var singleUI = Util.GetOrAddComponent<T>(go);
            _singleUIs.Add(name, singleUI);
            
            return singleUI;
        }

        public T GetSceneUI<T>() where T : UI_Scene
        {
            T ui = null;
            _sceneUIs.Find(s => s.TryGetComponent(out ui));
            return ui;
        }    
        
        public T GetPopupUI<T>() where T : UI_Popup
        {
            T ui = null;
            _popups.Find(p => p.TryGetComponent(out ui));
            return ui;
        }        
        
        public T ShowPopupUI<T>(string name = null) where T : UI_Popup
        {
            if (_popups.Count == 0) CameraController.I.OnOffUI(true);
            SetPowerSavingTimer();
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            var go = Manager.Resource.Instantiate($"UI/Popup/{name}", 1, Root.transform);
            var popup = Util.GetOrAddComponent<T>(go);
            _popups.Add(popup);

            go.GetOrAddComponent<Canvas>().sortingOrder = _order + _tmpAddedOrder;
            _order++;
            
            Manager.Guide.Check(popup);
            return popup;
        }

        public void SetTmpOrder(int order)
        {
            _tmpAddedOrder = order;
        }
        
        public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            var go = Manager.Resource.Instantiate($"UI/SubItem/{name}");
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localScale = Define.One;
            }

            return Util.GetOrAddComponent<T>(go);
        }
        
        public T MakeWorldSpace<T>(Transform parent = null, string name = null) where T : UI_Base
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            var go = Manager.Resource.Instantiate($"UI/WorldSpace/{name}");
            if (parent != null)
                go.transform.SetParent(parent);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            return Util.GetOrAddComponent<T>(go);
        }

        public void CloseSceneUI(UI_Scene scene)
        {
            _sceneUIs.Remove(scene);
            Manager.Resource.Destroy(scene.gameObject);
        }


        public bool CanClosePopup = true;
        public bool ClosePopupUI()
        {
            SetPowerSavingTimer();
            if (_popups.Count == 0)
            {
                return false;
            }

            var popup = _popups[^1];
            if (!CanClosePopup && popup is not UI_Quit) return false;
            if (!popup.CanClose())
            {
                return true;
            }
            _popups.RemoveAt(_popups.Count-1);
            popup.WhenPopupClosed();
            Manager.Resource.Destroy(popup.gameObject);
            popup = null;
            _order--;
            if (_popups.Count == 0)
            {
                CameraController.I.OnOffUI(false);
                Manager.Guide.CheckCurQuest();
            }
            else Manager.Guide.Check(_popups[^1]);
            
            return true;
        }

        public void CloseAllPopupUI(bool haveCloseSound = true)
        {
            if (haveCloseSound && _popups.Count > 0) Manager.Sound.PlaySFX(SFXType.UI_Close);
            while (_popups.Count > 0) ClosePopupUI();
        }

        public void Clear()
        {
            CloseAllPopupUI();
            _sceneUIs.Clear();
        }
        
        public void OpenDungeonEntrance(FieldType field, bool forceRefresh)
        {
            if (forceRefresh)
            {
                if (Manager.UI.GetPopupUI<UI_SummonResult>() != null) return;
                var abilityUI = Manager.UI.GetPopupUI<UI_Character>();
                if (abilityUI != null && abilityUI.IsChangingAbility()) return;
            }
            switch (field)
            {
                case FieldType.Awakening: case FieldType.Pet: case FieldType.Stage: case FieldType.SkillGrowth:
                    Manager.UI.ShowPopupUI<UI_DungeonStage>().SetFieldType(field, forceRefresh);
                    break;
                case FieldType.BlackMarket: case FieldType.Dia:
                    Manager.UI.ShowPopupUI<UI_DungeonLevel>().SetFieldType(field, forceRefresh);
                    break;
                case FieldType.Training:
                    Manager.UI.ShowPopupUI<UI_DungeonTrainingGround>().SetFieldType(field, forceRefresh);
                    break;
            }
        }
    }
}