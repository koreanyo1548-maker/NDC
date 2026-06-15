using Managers;
using MEC;
using UIBases;
using UIs.Dungeon;
using UIs.FieldMain;
using UnityEngine;
using Data;
using UIs.Dungeon.StageEntrance;

namespace UIs.Guide
{
    public class UI_Guide_Dungeon: UI_Guide
    {
        protected FieldType _fieldType;
        private int _idx;
        
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.Field.CurField.Value != FieldType.Stage)
            {
                Close();
                return;
            }

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainRight>().Get<RectTransform>("B_Dungeon");
                Set(indicate);
                _idx = 1;
                return;
            }

            var stage = Manager.UI.GetPopupUI<UI_DungeonStage>();
            if (stage != null)
            {
                _idx = 2;
                Next(stage);
                return;
            }

            var dungeon = Manager.UI.GetPopupUI<UI_Dungeon>();
            if (dungeon != null)
            {
                _idx = 1;
                Next(dungeon);
                return;
            }
            
            Close();
        }

        public override void Next(UI_Base popup)
        {
            if ((_idx == 1 && popup is not UI_Dungeon) || (_idx == 2 && popup is not UI_DungeonStage))
            {
                Close();
                return;
            }
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
                var indicate = _idx == 1
                    ? popup.Get<RectTransform>("UI_Dungeon_Item_" + _fieldType.ToString())
                    : ((UI_DungeonStage) popup).CurDungeon == _fieldType ? popup.Get<Transform>("Buttons").Find("B_Enter").GetComponent<RectTransform>()
                        : popup.Get<RectTransform>("B_Close");
                Set(indicate, false, _idx != 1);
                _idx = _idx == 1 ? 2 : 1;
            }, gameObject);
           
        }
    }
}