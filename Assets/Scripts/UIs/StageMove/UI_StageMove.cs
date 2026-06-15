using System.Collections.Generic;
using Controller;
using Controller.Infos;
using dynamicscroll;
using Managers;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.StageMove
{
    public class UI_StageMove: UI_Popup
    {
        private EventsManager _maxStageChangedHandler;

        private DynamicScroll<StageMoveItem, UI_StageMove_Item> _stages;
        
        private List<StageMoveItem> _stageData = new();
        

        enum DynamicScrollRects
        {
            V_Stage
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<DynamicScrollRect>(typeof(DynamicScrollRects));

            SetStages();
            
            _stages = new DynamicScroll<StageMoveItem, UI_StageMove_Item>();
            _stages.spacing = 0;
            _stages.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.V_Stage), _stageData,
                -1,"Prefabs/UI/SubItem/UI_StageMove_Item");

            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);

            _maxStageChangedHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenStageChanged,
                updatedField = new [] {LevelController.data.MaxStage}
            });

            return true;
        }

        private void WhenStageChanged()
        {
            var haveNew = SetStages();
            if (haveNew) _stages.ChangeList(_stageData);
        }

        public void Open()
        {
            if (!_isInit) Init();

            WhenStageChanged();
            var maxStage = LevelController.data.MaxStage.Value;
            var curStage = LevelController.data.Stage.Value;
            var curIndex = maxStage + 1 == curStage ? 0 : GetStageCount() - (curStage + 9) / 10;
            _stages.MoveToIndex(curIndex, 0.1f);
            _stages.ForceUpdateList();
        }


        private bool SetStages()
        {
            var count = GetStageCount();
            var haveNew = _stageData.Count < count;
            for (var idx = _stageData.Count; idx < count; ++idx) _stageData.Add(new StageMoveItem());
            return haveNew;
        }

        private int GetStageCount()
        {
            var maxStage = LevelController.data.MaxStage.Value;
            return (maxStage + 9) / 10 + 1;
        }
        
        public override void WhenPopupClosed()
        {
            
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }
        
        private void OnEnable()
        {
            _maxStageChangedHandler?.Reconnect();
        }

        private void OnDisable()
        {
            _maxStageChangedHandler?.Dispose();
        }
    }
}