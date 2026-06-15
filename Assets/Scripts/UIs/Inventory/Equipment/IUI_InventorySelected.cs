using Data;
using UIs.Utils;

namespace UIs.Inventory.Equipment
{
    public interface IUI_InventorySelected
    {
        public UIField<int> NowSelected();
        public void Set(EquipmentType type, int id);
        public UIField<int> GetPresetIdx();
    }
}