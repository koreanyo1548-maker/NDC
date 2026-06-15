using System.Collections.Generic;
using Controller.Infos;
using Data;
using Data.DbEquipment;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Data.Utils;
using ThirdParty;
using Random = UnityEngine.Random;
using Utils;

namespace Controller.Have
{
    public class SkillController: Singleton<SkillController>
    {
        private bool _haveAll;

        
        public SkillController()
        {
            DbUserSkill.ForEach(s => s.Count.ValueChanged += OnCountUpdated);
        }
        
        public void Init()
        {
            _haveAll = HaveAll();
        }

        public bool HaveAll()
        {
            return DbUserSkill.GetAll(w => w.Have.Value).Count == DbSkill.Count;
        }

        public long DecomposeAll()
        {
            var compose = 0L;

            DbUserSkill.ForEach(s =>
            {
                if (s.Awakening.Value == 5 && s.Count.Value > 0)
                {
                    compose += s.Decompose();
                }
            });
            
            return compose;
        }

        public int GetTotalAwakeningCount()
        {
            var count = 0;
            DbUserSkill.ForEach(a => count += a.Awakening.Value);
            return count;
        }
        
        private void OnCountUpdated(object sender, DbEventArgs e)
        {
            var skill = DbUserSkill.Get(e.Id);
            if (!skill.Have.Value)
            {
                skill.SetNew();
                skill.Have.Value = true;
                skill.GrowthIt();
                if (!_haveAll && HaveAll())
                {
                    //Analytics.sendEvent("ObtainSkillAll");
                    _haveAll = true;
                }
            }
        }       
        
        public List<IDbCanSummon> AddRandom(int count)
        {
            var totalCount = count;
            var added = new Dictionary<DbSkill, int>();
            var result = new List<IDbCanSummon>();
            var prMeta = DbSummonSkillProbability.Get(LevelController.I.GetSummonLevelForMeta(SummonType.Skill));
            while (count-- > 0)
            {
                var pick = GradeType.Normal;
                var gradeChoose = Random.Range(0, 1000000);

                var pr = 0;
                while (gradeChoose >= pr)
                {
                    pr += prMeta.GetPr(pick);
                    pick++;
                }
                var add = pick - 1;

                var skills = DbSkill.GradeToSkills[add];
                var numberChoose = Random.Range(0, skills.Count);
                var skill = skills[numberChoose];

                // log += $"[{totalCount - count}번] 등급: {add} (하위 {(1f * gradeChoose / 1000000):P4}), 선택: {numberChoose+1}    |    ";

                if (added.ContainsKey(skill)) added[skill]++;
                else added.Add(skill, 1);
                result.Add(skill);
            }

            foreach (var add in added)
            {
                var s = DbUserSkill.Get(add.Key.Id);
                s.Count.Value += add.Value;
            }
            QuestController.I.DoQuests(QuestType.SkillSummon, totalCount);
            return result;
        }
        
        public bool ASkillAwakened()
        {
            var awakened = false;
            DbUserSkill.ForEach(w =>
            {
                if (w.Awakening.Value > 0) awakened = true;
            });
            return awakened;
        }

        public void Add(int skillId, int count)
        {
            DbUserSkill.Get(skillId).Count.Value += count;
        }

    }

}