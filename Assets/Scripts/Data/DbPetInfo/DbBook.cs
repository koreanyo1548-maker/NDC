using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.DbDefinition;
using Data.Utils;
using ThirdParty;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbBook: DbModel<DbBook, CurrencyType>
    {
        public int NameId;
        public int Time;
        public List<DbProbability<GradeType>> Pets;
        public List<string> Pet;
        public int MinPetCount;
        public int MaxPetCount;
        public int MinStoneCount;
        public int MaxStoneCount;
        
        
        public override void Load()
        {
            fileName = "Book";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.Pets = new List<DbProbability<GradeType>>();
                foreach (var t in q.Pet) q.Pets.Add(new DbProbability<GradeType>(t));
                q.Pet.Clear();
            });
        }

        public string GetMinGradeName()
        {
            return LocalString.Get(DbGrade.Get(Pets[0].category).NameId);
        }
        
        public string GetMaxGradeName()
        {
            return LocalString.Get(DbGrade.Get(Pets[^1].category).NameId);
        }
        
        public int GetStoneCount()
        {
            return Random.Range(MinStoneCount, MaxStoneCount+1);
        }

        /// <returns>펫 아이디, 펫 개수</returns>
        public Dictionary<int, int> GetPets(int bookCount)
        {
            // var log = $"[책] {Id}를 {bookCount}번 해제    |    ";
            Dictionary<int, int> pets = new();
            var count = 0;
            while (bookCount-- > 0)
            {
                var addCount = Random.Range(MinPetCount, MaxPetCount);
                count += addCount;
                // log += $"[{bookCount}번 책] {addCount}개 얻음    |    ";
            }
            // log += $"= 총 {count}개 얻음    |    ";
            var prCount = Pets.Count;
            var getCount = 0;
            while (getCount < count)
            {
                var ran = Random.Range(0, 100);
                var add = 0;
                for (var idx = 0; idx < prCount; ++idx)
                {
                    add += Pets[idx].probability;
                    if (ran < add)
                    {
                        var canBe = DbPet.GradeToPets[Pets[idx].category];
                        var pick = Random.Range(0, canBe.Count);
                        var select = canBe[pick];
                        // log += $"[{getCount}번 펫] 등급: {Pets[idx].category} (하위 {(1f * ran / 100):P0}), 선택: {pick+1}    |    ";
                        if (pets.ContainsKey(select.Id)) pets[select.Id]++;
                        else pets.Add(select.Id, 1);
                        getCount++;
                        break;
                    }
                }
            }

            // PlayFabManager.Store.SetLog(log);
            return pets;
        }
    }
}