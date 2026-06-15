using ThirdParty;
using TMPro;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_Dungeon_Item_Training: UI_Dungeon_Item
    {
        private TextMeshProUGUI _ranking;

        public override bool Init()
        {
            if (!base.Init()) return false;

            _ranking = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Ranking", true);
            SetRanking();
            
            return true;
        }

        private void SetRanking()
        {
            PlayFabManager.Leaderboard.GetMyTrainingRanking(ranking => _ranking.text = ranking == 0 ? LocalString.Get(210410) : ranking.ToString());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isInit)
            {
                SetRanking();
            }
        }
    }
}