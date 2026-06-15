using Data.DbDefinition;
using Data.DbDungeon;
using DG.Tweening;
using Managers;
using UnityEngine;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class TrainingRewardItem: MonoBehaviour
    {
        private bool _isInit;
        private SpriteRenderer _itemImg;
        
        private void Init()
        {
            _isInit = true;
           _itemImg = transform.Find("IMG_Item").GetComponent<SpriteRenderer>(); 
        }
        
        public void Set(DbTrainingGroundReward reward, bool needLong)
        {
            if (!_isInit) Init();
            _itemImg.color = Color.white;
            _itemImg.sprite = DbCurrency.Get(reward.RewardType).GetResource(reward.RewardId);
            
            var monsterPosition = Manager.Field.GetFirst().Position();
            monsterPosition.y += 0.3f;
            transform.position = monsterPosition;
            
            monsterPosition.x += Random.Range(0, 2) == 0 ? Random.Range(0.2f, 0.7f) : Random.Range(-0.7f, -0.2f);
            monsterPosition.y += Random.Range(-0.235f, -0.611f);
            
            var middlePosition = Vector3.Lerp(transform.position, monsterPosition, Random.Range(0.7f, 0.8f));
            transform.DOJump(middlePosition, 1f, 1, Random.Range(0.4f, 0.5f)).OnComplete(() =>
            {
                transform.DOJump(monsterPosition, 0.2f, 1, Random.Range(0.25f, 0.35f)).OnComplete(() =>
                {
                    _itemImg.DOFade(0, needLong ? 0.6f : 0.3f).OnComplete(() =>
                    {
                        Manager.Resource.Destroy(gameObject);
                    });
                });
            });
        }
    }
}