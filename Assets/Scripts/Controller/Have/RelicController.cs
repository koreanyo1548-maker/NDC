using System;
using System.Collections.Generic;
using Controller.Infos;
using Data;
using Data.DbEquipment;
using Data.DbRelicInfo;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Data.Stores;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Have
{
    public class RelicController : Singleton<RelicController>, IDbSummonProbability
    {
        private Dictionary<GradeType, int> _biasedProbability;
        private GradeType _least = GradeType.Mythic;
        private Dictionary<GradeType, List<int>> _gradeToSummoned;

        public void Init()
        {
            _biasedProbability = new();
            _gradeToSummoned = new();
            foreach (GradeType grade in Enum.GetValues(typeof(GradeType)))
            {
                _biasedProbability.Add(grade, 0);
                _gradeToSummoned.Add(grade, new());
            }

            DbUserRelic.ForEach(r =>
            {
                r.Level.ValueChanged += ReCalculateSummonInfo;
            });
            InitiateSummonInfo();
        }
        
        public List<IDbCanSummon> AddRandom(int count)
        {
            var totalCount = count;
            var added = new Dictionary<DbRelic, int>();
            var result = new List<IDbCanSummon>();
            while (count-- > 0)
            {
                var pick = GradeType.Normal;
                var gradeChoose = Random.Range(0, 1000000);

                var pr = 0;
                while (gradeChoose >= pr)
                {
                    pr += _biasedProbability[pick];
                    pick++;
                }
                var add = pick - 1;

                var numberChoose = Random.Range(0, _gradeToSummoned[add].Count);
                var relic = DbRelic.Get(_gradeToSummoned[add][numberChoose]);

                if (added.ContainsKey(relic)) added[relic]++;
                else added.Add(relic, 1);
                result.Add(relic);
            }

            foreach (var add in added)
            {
                var s = DbUserRelic.Get(add.Key.Id);
                s.Count.Value += add.Value;
            }
            QuestController.I.DoQuests(QuestType.RelicSummon, totalCount);
            return result;
        }

        public int GetTotalLevelCount()
        {
            var count = 0;
            DbUserRelic.ForEach(r => count += r.Level.Value);
            return count;
        }
        
        #region 확률 계산
        private void InitiateSummonInfo()
        {
            var maxLevel = DbRelicLevel.Count;
            // 소환 가능한 유물 (최대레벨 안찍은 유물) 등급별로 저장
            DbUserRelic.ForEach(r => 
            {
                if (r.Level.Value < maxLevel)
                {
                    var grade = DbRelic.Get(r.Id).Grade;
                    _gradeToSummoned[grade].Add(r.Id);
                }
            });
            
            if (!HaveAny()) WhenAllIsMaxLevel();
            
            // 가장 낮은 등급 저장
            SetLowestGrade();
            CalculateSummonPrForAll();
        }

        private void ReCalculateSummonInfo(object sender, DbEventArgs e)
        {
            if (DbUserRelic.Get(e.Id).Level.Value < DbRelicLevel.Count) return;
            
            var meta = DbRelic.Get(e.Id);
            
            // 소환 가능한 목록에서 제외
            _gradeToSummoned[meta.Grade].Remove(e.Id);
            
            // 해당 등급에 더이상 소환할 게 없을 경우
            if (_gradeToSummoned[meta.Grade].Count == 0)
            {
                // 모든 등급에 더이상 소환할 게 없는 경우
                if (!HaveAny())
                {
                    WhenAllIsMaxLevel();
                    CalculateSummonPrForAll();
                    return;
                }
                // 현재 가장 낮은 등급이면
                if (meta.Grade == _least) SetLowestGrade();
                    
                _biasedProbability[_least] += _biasedProbability[meta.Grade];
                _biasedProbability[meta.Grade] = 0;
            }
        }

        private void WhenAllIsMaxLevel()
        {
            // 모두 최대면 모두 뽑힐 수 있고 더이상 레벨 체크 하지 않음
            DbUserRelic.ForEach(r =>
            {
                _least = GradeType.Normal;
                var grade = DbRelic.Get(r.Id).Grade;
                _gradeToSummoned[grade].Add(r.Id);
                r.Level.ValueChanged -= ReCalculateSummonInfo;
            });
        }
        
        private void CalculateSummonPrForAll()
        {
            // 초기화
            foreach (var grade in _gradeToSummoned)
            {
                _biasedProbability[grade.Key] = 0;
            }
            // 등급별 확률 계산
            foreach (var grade in _gradeToSummoned)
            {
                // 소환할게 있으면 자기꺼에 확률 저장
                if (grade.Value.Count > 0) _biasedProbability[grade.Key] += DbSummonRelicProbability.Get(grade.Key).Probability;
                // 없으면 가장 낮은 등급에 확률 저장
                else _biasedProbability[_least] += DbSummonRelicProbability.Get(grade.Key).Probability;
            }
        }

        private void SetLowestGrade()
        {
            foreach (GradeType grade in Enum.GetValues(typeof(GradeType)))
            {
                if (_gradeToSummoned[grade].Count > 0)
                {
                    _least = grade;
                    break;
                }
            }
        }
        
        #endregion

        public int GetPr(int idx)
        {
            return _biasedProbability[GradeType.Normal + idx];
        }
        
        private bool HaveAny()
        {
            foreach (var summoned in _gradeToSummoned)
            {
                if (summoned.Value.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}