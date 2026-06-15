using Controller.Infos;
using Data.DbRecord;
using Data.DbUser.Progress;
using TMPro;
using UIBases;
using Utils;

namespace UIs.Character.Title
{
    public class UI_Title: UI_Base
    {
        private EventsManager _titleEventManager;
        
        enum Texts
        {
            T_MyTitleInfo,
            T_MyTitleLevel
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            _titleEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenTitleChanged,
                updatedField = new[] {EquipController.data.Title}
            });

            WhenTitleChanged();
            return true;
        }
        
        private void WhenTitleChanged()
        {
            var equipped = EquipController.data.Title.Value;
            var titleMeta = DbTitle.Get(equipped);
            Get<TextMeshProUGUI>((int) Texts.T_MyTitleInfo).text =
                titleMeta == null ? LocalString.Get(210046) :
                titleMeta.GetNameWithColor();
            var title = DbUserTitle.Get(equipped);
            Get<TextMeshProUGUI>((int) Texts.T_MyTitleLevel).text = title == null ? string.Empty : string.Format(LocalString.Get(210041), title.Level.Value);
        }
        
        private void OnDisable()
        {
            _titleEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _titleEventManager?.Reconnect();
        }
    }
}