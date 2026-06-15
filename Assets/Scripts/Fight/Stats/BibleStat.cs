using System.Numerics;
using Controller;
using Controller.Infos;
using Data.DbPetInfo;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Stats
{
    public class BibleStat : HpStat
    {
        private EventsManager _statManager;
        
        public BibleStat(MonoBehaviour owner)
        {
            _isBoss = true;
            hpBar = Util.FindChild(owner.gameObject, "Hp", true).transform;
            hpBarParent = hpBar.parent;
            _hpRight = hpBarParent.localScale;
            _hpLeft = new Vector3(-_hpRight.x, _hpRight.y);

            _statManager = new EventsManager(owner, new EventsManager.Config
            {
                handler = WhenBibleLevelChanged,
                updatedField = new []{ LevelController.data.BibleLevel }
            });
            
            WhenBibleLevelChanged();
        }

        private void WhenBibleLevelChanged()
        {
            ApplyMaxHp(LevelController.I.GetBibleHp(), true);
        }

        public void OnEnable()
        {
            _statManager?.Reconnect();
        }

        public void OnDisable()
        {
            _statManager.Dispose();
        }
    }
}