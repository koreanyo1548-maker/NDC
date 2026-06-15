using System;
using System.Collections.Generic;
using System.Numerics;
using Controller.Infos;
using Data.DbCommon;
using Data.DbEquipment;
using Data.DbRecord;
using Data.DbUser;
using Data.DbUser.Currency;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Newtonsoft.Json;
using ThirdParty;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Stores
{
    [Serializable]
    public class UserInfo
    {
        public static UserInfo saved = new();
        public Equipment equipment;
        public Record record;
        public List<Ability> ability;
        public List<Relic> relic;
        public CurrencyInfo currency;
        public Info info;
        public UserLog log;

        public UserInfo()
        {
        }
        
        public UserInfo(Equipment equipment, int newbieQuestDay, List<NewbieQuest> newbieQuest, List<Quest> quest, 
            List<Title> title, List<Ability> ability, List<Relic> relic, 
            CurrencyInfo currency, Info info, UserLog log)
        {
            this.equipment = new Equipment(equipment.weapons, equipment.accessories, equipment.skills, equipment.pets, equipment.necklaces);
            record = new Record(newbieQuestDay, newbieQuest, quest, title);
            this.ability = ability;
            this.relic = relic;
            this.currency = currency;
            this.info = info;
            this.log = log;
            CheckVersion();
            saved = this;
        }
        public UserInfo(Equipment equipment, Record record, 
            List<Ability> ability, List<Relic> relic, 
            CurrencyInfo currency, Info info, UserLog log)
        {
            this.equipment = new Equipment(equipment.weapons, equipment.accessories, equipment.skills, equipment.pets, equipment.necklaces);
            this.record = new Record(record.newbieQuestDay, record.newbieQuests, record.quests, record.titles);
            this.ability = ability;
            this.relic = relic;
            this.currency = currency;
            this.info = info;
            this.log = log;
            CheckVersion();
            saved = this;
        }

        private void CheckVersion()
        {
        }
        
        #region OnCreated
        
        public void WhenCreated(DateTime now)
        {
            saved = new();
            saved.equipment = new Equipment(new (), new(), new(), new (), new());
            saved.record = new Record(0, new(), new(), new());
            saved.ability = new();
            saved.relic = new();
            saved.currency = new(1, new());
            saved.info = new();
            saved.info.WhenCreated(now);
            saved.log = new();
        }
        #endregion
        
        #region OnLoad
        public List<DbUserWeapon> ConvertToWeapon()
        {
            var list = new List<DbUserWeapon>(equipment.weapons.Count);
            foreach (var w in equipment.weapons)
            {
                list.Add(new DbUserWeapon(w.id, w.cnt, w.aw, w.gr));
            }
            return list;
        }
        public List<DbUserAccessory> ConvertToAccessory()
        {
            var list = new List<DbUserAccessory>(equipment.accessories.Count);
            foreach (var a in equipment.accessories)
            {
                list.Add(new DbUserAccessory(a.id, a.cnt, a.aw, a.gr));
            }
            return list;
        }
        public List<DbUserSkill> ConvertToSkill()
        {
            var list = new List<DbUserSkill>(equipment.skills.Count);
            foreach (var s in equipment.skills)
            {
                if (!DbSkill.Have(s.id)) continue;
                list.Add(new DbUserSkill(s.id, s.cnt, s.aw, s.gr));
            }
            return list;
        }

        public List<DbUserNewbieQuest> ConvertToNewbieQuest()
        {
            if (record.newbieQuests == null) return null;
            var list = new List<DbUserNewbieQuest>(record.newbieQuests.Count);
            foreach (var t in record.newbieQuests)
            {
                list.Add(new DbUserNewbieQuest(t.id, t.cnt, t.isRewarded));
            }
            return list;
        }

        public int GetNewbieQuestDay()
        {
            return record.newbieQuestDay;
        }

        public List<DbUserQuest> ConvertToQuest(int dayDiff, bool isNewWeek)
        {
            var list = new List<DbUserQuest>(record.quests.Count);
            foreach (var q in record.quests)
            {
                if (DbQuest.Get(q.id) == null) continue;
                if (dayDiff > 0 && DbQuest.Get(q.id).Cycle == QuestCycleType.Daily)
                {
                    q.cnt = 0;
                    q.isRewarded = false;
                }

                if (isNewWeek && DbQuest.Get(q.id).Cycle == QuestCycleType.Weekly)
                {
                    q.cnt = 0;
                    q.isRewarded = false;
                }
                list.Add(new DbUserQuest(q.id, q.cnt, q.isRewarded, DbQuest.Get(q.id).ToDo));
            }
            return list;
        }

        public List<DbUserTitle> ConvertToTitle()
        {
            if (record.titles == null) return null;
            var list = new List<DbUserTitle>(record.titles.Count);
            foreach (var t in record.titles)
            {
                list.Add(new DbUserTitle(t.id, t.lv, t.doCnt));
            }
            return list;
        }

        public List<DbUserPet> ConvertToPet()
        {
            if (equipment.pets == null) return null;
            var list = new List<DbUserPet>(equipment.pets.Count);
            foreach (var p in equipment.pets)
            {
                list.Add(new DbUserPet(p.id, p.cnt, p.aw, p.gr));
            }
            return list;
        }

        public List<DbUserNecklace> ConvertToNecklaces()
        {
            var list = new List<DbUserNecklace>(equipment.necklaces.Count);
            foreach (var n in equipment.necklaces)
            {
                list.Add(new DbUserNecklace(n.id, n.cnt, n.aw, n.gr));
            }
            return list;
        }

        public List<DbUserAbility> ConvertToAbility()
        {
            if (ability.Count == 0) return null;
            var list = new List<DbUserAbility>(ability.Count);
            foreach (var a in ability)
            {
                list.Add(new DbUserAbility(a.id, a.isUsing, a.isLocked, a.option, a.optionGrade, a.rune));
            }
            return list;
        }
        public List<DbUserRelic> ConvertToRelic()
        {
            if (relic.Count == 0) return null;
            var list = new List<DbUserRelic>(relic.Count);
            foreach (var r in relic)
            {
                list.Add(new DbUserRelic(r.id, r.count, r.level));
            }
            return list;
        }
        #endregion

        #region OnSave
        public static string GetEquipments()
        {
            DbUserWeapon.ForEach(w =>
            {
                var weapon = saved.equipment.weapons.Find(s => s.id == w.Id);
                if (weapon != null) weapon.Set(w);
                else saved.equipment.weapons.Add(new Weapon(w));
            });
            DbUserAccessory.ForEach(a =>
            {
                var accessory = saved.equipment.accessories.Find(s => s.id == a.Id);
                if (accessory != null) accessory.Set(a);
                else saved.equipment.accessories.Add(new Accessory(a));
            });
            DbUserSkill.ForEach(sk =>
            {
                var skill = saved.equipment.skills.Find(s => s.id == sk.Id);
                if (skill != null) skill.Set(sk);
                else saved.equipment.skills.Add(new Skill(sk));
            });
            DbUserPet.ForEach(p =>
            {
                var pet = saved.equipment.pets.Find(s => s.id == p.Id);
                if (pet != null) pet.Set(p);
                else saved.equipment.pets.Add(new Pet(p));
            });
            DbUserNecklace.ForEach(n =>
            {
                var necklace = saved.equipment.necklaces.Find(s => s.id == n.Id);
                if (necklace != null) necklace.Set(n);
                else saved.equipment.necklaces.Add(new Necklace(n));
            });
            return JsonConvert.SerializeObject(saved.equipment);
        }
        public static string GetRecords()
        {
            saved.record.newbieQuestDay = NewbieQuestController.I.CurDay.Value;
            DbUserNewbieQuest.ForEach(q =>
            {
                if (saved.record.newbieQuests == null) saved.record.newbieQuests = new();
                var quest = saved.record.newbieQuests.Find(s => s.id == q.Id);
                if (quest != null) quest.Set(q);
                else saved.record.newbieQuests.Add(new NewbieQuest(q));
            });
            DbUserQuest.ForEach(q =>
            {
                var quest = saved.record.quests.Find(s => s.id == q.Id);
                if (quest != null) quest.Set(q);
                else saved.record.quests.Add(new Quest(q));
            });
            DbUserTitle.ForEach(t =>
            {
                var title = saved.record.titles.Find(s => s.id == t.Id);
                if (title != null) title.Set(t);
                else saved.record.titles.Add(new Title(t));
            });
            return JsonConvert.SerializeObject(saved.record);
        }

        public static string GetAbility()
        {
            DbUserAbility.ForEach(a =>
            {
                var ability = saved.ability.Find(s => s.id == a.Id);
                if (ability != null) ability.Set(a);
                else saved.ability.Add(new Ability(a));
            });
            return JsonConvert.SerializeObject(saved.ability);
        }

        public static string GetRelic()
        {
            DbUserRelic.ForEach(r =>
            {
                var relic = saved.relic.Find(s => s.id == r.Id);
                if (relic != null) relic.Set(r);
                else saved.relic.Add(new Relic(r));
            });
            return JsonConvert.SerializeObject(saved.relic);
        }

        public static string GetInfo()
        {
            return JsonConvert.SerializeObject(saved.info.Set());
        }

        public static string GetCurrency()
        {
            saved.currency.currency.Set(DbUserCurrency.Get(0));
            saved.currency.version = 1;
            return JsonConvert.SerializeObject(saved.currency);
        }
        #endregion
        
        
    }
    
    [Serializable]
    public class Equipment
    {
        public List<Necklace> necklaces;
        public List<Weapon> weapons;
        public List<Accessory> accessories;
        public List<Skill> skills;
        public List<Pet> pets;


        [JsonConstructor]
        public Equipment(List<Weapon> weapons, List<Accessory> accessories, List<Skill> skills,
            List<Pet> pets, List<Necklace> necklaces)
        {
            this.weapons = weapons;
            this.accessories = accessories;
            this.skills = skills;
            this.pets = pets;
            this.necklaces = necklaces;
        }
    }
    
    
    [Serializable]
    public class Weapon
    {
        public int id;
        public int cnt;
        public int aw;
        public int gr;

        public void Set(DbUserWeapon weapon)
        {
            id = weapon.Id;
            cnt = weapon.Count.Value;
            aw = weapon.Awakening.Value;
            gr = weapon.Growth.Value;
        }

        public Weapon(DbUserWeapon weapon)
        {
            Set(weapon);
        }

        [JsonConstructor]
        public Weapon(int id, int cnt, int aw, int gr)
        {
            this.id = id;
            this.cnt = cnt;
            this.aw = aw;
            this.gr = gr;
        }
    }
    
    [Serializable]
    public class Accessory
    {
        public int id;
        public int cnt;
        public int aw;
        public int gr;

        public void Set(DbUserAccessory accessory)
        {
            id = accessory.Id;
            cnt = accessory.Count.Value;
            aw = accessory.Awakening.Value;
            gr = accessory.Growth.Value;
        }

        public Accessory(DbUserAccessory accessory)
        {
            Set(accessory);
        }

        [JsonConstructor]
        public Accessory(int id, int cnt, int aw, int gr)
        {
            this.id = id;
            this.cnt = cnt;
            this.aw = aw;
            this.gr = gr;
        }
    }
    
    [Serializable]
    public class Skill
    {
        public int id;
        public int cnt;
        public int aw;
        public int gr;

        public void Set(DbUserSkill skill)
        {
            id = skill.Id;
            cnt = skill.Count.Value;
            aw = skill.Awakening.Value;
            gr = skill.Growth.Value;
        }

        public Skill(DbUserSkill skill)
        {
            Set(skill);
        }

        [JsonConstructor]
        public Skill(int id, int cnt, int aw, int gr)
        {
            this.id = id;
            this.cnt = cnt;
            this.aw = aw;
            this.gr = gr;
        }
    }  
    
    [Serializable]
    public class Title
    {
        public int id;
        public int doCnt;
        public int lv;

        public void Set(DbUserTitle title)
        {
            id = title.Id;
            doCnt = title.DoCount.Value;
            lv = title.Level.Value;
        }


        public Title(DbUserTitle title)
        {
            Set(title);
        }

        [JsonConstructor]
        public Title(int id, int doCnt, int lv)
        {
            this.id = id;
            this.doCnt = doCnt;
            this.lv = lv;
        }
    }
    
    [Serializable]
    public class Pet
    {
        public int id;
        public int cnt;
        public int aw;
        public int gr;

        public void Set(DbUserPet pet)
        {
            id = pet.Id;
            cnt = pet.Count.Value;
            aw = pet.Awakening.Value;
            gr = pet.Growth.Value;
        }

        public Pet(DbUserPet pet)
        {
            Set(pet);
        }

        [JsonConstructor]
        public Pet(int id, int cnt, int aw, int gr)
        {
            this.id = id;
            this.cnt = cnt;
            this.aw = aw;
            this.gr = gr;
        }
    } 
    
    [Serializable]
    public class Necklace
    {
        public int id;
        public int cnt;
        public int aw;
        public int gr;
 
        public void Set(DbUserNecklace pet)
        {
            id = pet.Id;
            cnt = pet.Count.Value;
            aw = pet.Awakening.Value;
            gr = pet.Growth.Value;
        }
 
        public Necklace(DbUserNecklace pet)
        {
            Set(pet);
        }
 
        [JsonConstructor]
        public Necklace(int id, int cnt, int aw, int gr)
        {
            this.id = id;
            this.cnt = cnt;
            this.aw = aw;
            this.gr = gr;
        }
    }

    [Serializable]
    public class Ability
    {
        public int id;
        public bool isUsing;
        public bool isLocked;
        public StatType option;
        public GradeType optionGrade;
        public StatType rune;

        public void Set(DbUserAbility ability)
        {
            id = ability.Id;
            isUsing = ability.IsUsing.Value;
            isLocked = ability.IsLocked.Value;
            option = ability.Option.Value;
            optionGrade = ability.OptionGrade;
            rune = ability.Rune;
        }

        public Ability(DbUserAbility ability)
        {
            Set(ability);
        }
        
        [JsonConstructor]
        public Ability(int id, bool isUsing, bool isLocked, StatType option, GradeType optionGrade, StatType rune)
        {
            this.id = id;
            this.isUsing = isUsing;
            this.isLocked = isLocked;
            this.option = option;
            this.optionGrade = optionGrade;
            this.rune = rune;
        }
    }

    [Serializable]
    public class Relic
    {
        public int id;
        public BigInteger count;
        public int level;

        public void Set(DbUserRelic relic)
        {
            id = relic.Id;
            count = relic.Count.Value;
            level = relic.Level.Value;
        }

        public Relic(DbUserRelic relic)
        {
            Set(relic);
        }

        [JsonConstructor]
        public Relic(int id, BigInteger count, int level)
        {
            this.id = id;
            this.count = count;
            this.level = level;
        }
    }
    [Serializable]
    public class Record
    {
        public int newbieQuestDay;
        public List<NewbieQuest> newbieQuests;
        public List<Quest> quests;
        public List<Title> titles;

        [JsonConstructor]
        public Record(int newbieQuestDay, List<NewbieQuest> newbieQuests, List<Quest> quests, List<Title> titles)
        {
            this.newbieQuestDay = newbieQuestDay;
            this.newbieQuests = newbieQuests;
            this.quests = quests;
            this.titles = titles;
        }
    }

    [Serializable]
    public class NewbieQuest
    {
        public int id;
        public int cnt;
        public bool isRewarded;
        
        public void Set(DbUserNewbieQuest quest)
        {
            id = quest.Id;
            cnt = quest.Count.Value;
            isRewarded = quest.IsRewarded.Value;
        }

        public NewbieQuest(DbUserNewbieQuest quest)
        {
            Set(quest);
        }

        [JsonConstructor]
        public NewbieQuest(int id, int cnt, bool isRewarded)
        {
            this.id = id;
            this.cnt = cnt;
            this.isRewarded = isRewarded;
        }
    }
    
    [Serializable]
    public class Quest
    {
        public int id;
        public int cnt;
        public bool isRewarded;

        public void Set(DbUserQuest quest)
        {
            id = quest.Id;
            cnt = quest.Count.Value;
            isRewarded = quest.IsRewarded.Value;
        }

        public Quest(DbUserQuest quest)
        {
            Set(quest);
        }

        [JsonConstructor]
        public Quest(int id, int cnt, bool isRewarded)
        {
            this.id = id;
            this.cnt = cnt;
            this.isRewarded = isRewarded;
        }
    }
    
    [Serializable]
    public class UserLog
    {
        public List<Tuple<DateTime, int, List<DbRewardBig>>> offlineReward;

        public UserLog()
        {
            offlineReward = new();
        }

        [JsonConstructor]
        public UserLog(List<Tuple<DateTime, int, List<DbRewardBig>>> offlineReward)
        {
            this.offlineReward = offlineReward;
        }

        public void Add(int time, List<DbRewardBig> rewards)
        {
            if (offlineReward.Count >= 20)
            {
                offlineReward.RemoveAt(0);
            }
            offlineReward.Add(new Tuple<DateTime, int, List<DbRewardBig>>(DateTime.UtcNow.AddHours(9), time, rewards));
            PlayFabManager.Store.SaveLog();
        }
    }
}