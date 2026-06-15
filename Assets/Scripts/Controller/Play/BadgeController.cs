using System.Collections.Generic;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbPromote;
using Data.DbRecord;
using Data.DbShop;
using Data.DbUser.Equipment;
using Data.DbUser.Etc;
using Data.DbUser.Progress;
using Data.Utils;
using Utils;

namespace Controller.Play
{
    public class BadgeController : Singleton<BadgeController>
    {
        public static DbUserBadge data = DbUserBadge.Get(0);

        public BadgeController()
        {
            data.Stats.Value = GetAllStatBadges();
            data.Quests.Value = GetAllQuestBadges();
            data.Weapons.Value = GetAllWeaponsBadges();
            data.Accessories.Value = GetAllAccessoriesBadges();
            data.Relics.Value = GetAllRelicsBadges();
            data.Skills.Value = GetAllSkillsBadges();
            data.Summon.Value = LevelController.I.CanAnyGetSummonReward();
            data.Necklace.Value = NecklaceController.I.CheckCanUpgrade();

            OnLevelPointUpdated(null, null);
            OnExpUpdated(null, null);
            OnDungeonUpdated(null, null);
            OnSalaryOrPromotionUpdated(null, null);
            OnInGameShopsUpdated(null, null);
            OnTitleUpdated(null, null);
            OnBookUpdated(null, null);
            OnAttendUpdated(null, null);

            CurrencyController.I.GetMoneyModel(CurrencyType.Gold).ValueChanged += OnStatUpdated;
            CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).ValueChanged += OnLevelPointUpdated;
            foreach (var ticket in CurrencyController.data.Tickets)
            {
                ticket.Value.ValueChanged += OnDungeonUpdated;
            }
            CurrencyController.data.Buy.ValueChanged += OnInGameShopsUpdated;
            LevelController.data.Promotion.ValueChanged += OnSalaryOrPromotionUpdated;
            LevelController.data.MaxStage.ValueChanged += OnSalaryOrPromotionUpdated;
            LevelController.data.Exp.ValueChanged += OnExpUpdated;
            LevelController.data.WeaponSummonLevel.ValueChanged += OnSummonUpdated;
            LevelController.data.WeaponSummonReward.ValueChanged += OnSummonUpdated;
            LevelController.data.AccessorySummonLevel.ValueChanged += OnSummonUpdated;
            LevelController.data.AccessorySummonReward.ValueChanged += OnSummonUpdated;
            LevelController.data.SkillSummonLevel.ValueChanged += OnSummonUpdated;
            LevelController.data.SkillSummonReward.ValueChanged += OnSummonUpdated;
            NecklaceController.I.CanUpgrade.ValueChanged += OnNecklaceUpdated;

            DbUserTitle.ForEach(t =>
            {
                if (t.Level.Value == 0)
                {
                    t.DoCount.ValueChanged += OnTitleUpdated;
                    t.Level.ValueChanged += OnTitleUpdated;
                }
            });
            DbUserQuest.ForEach(q => q.Count.ValueChanged += OnQuestUpdated);
            DbUserQuest.ForEach(q => q.Meta.Cycle != QuestCycleType.Repeat,
                q => q.IsRewarded.ValueChanged += OnQuestUpdated);
            DbUserWeapon.ForEach(w =>
            {
                if (!w.Have.Value) w.Have.ValueChanged += OnWeaponUpdated;
            });
            DbUserAccessory.ForEach(a =>
            {
                if (!a.Have.Value) a.Have.ValueChanged += OnAccessoryUpdated;
            });
            DbUserRelic.ForEach(r =>
            {
                r.Count.ValueChanged += OnRelicUpdated;
            });
            DbUserSkill.ForEach(s => 
            {
                if (!s.Have.Value) s.Have.ValueChanged += OnSkillUpdated;
            });

            for (var idx = CurrencyType.NormalBook1; idx <= CurrencyType.NormalBook4; ++idx)
            {
                CurrencyController.I.GetBookModel(idx).ValueChanged += OnBookUpdated;
            }

            var bookshelves = CurrencyController.data.BookShelves.Value;
            foreach (var bookshelf in bookshelves)
            {
                bookshelf.Value.HaveBook.ValueChanged += OnBookUpdated;
                bookshelf.Value.CanOpen.ValueChanged += OnBookUpdated;
            }
        }

        public void OnStatUpdated(object sender, DbEventArgs e)
        {
            DbGoldStat.ForEach(SetNeedBadge);

            void SetNeedBadge(DbGoldStat stat)
            {
                if (data.Stats.Value[stat.Index].Value == (StatController.I.Get(stat.Id).CanLevelUp(1, true) > 0)) return;
                data.Stats.SetValue(stat.Index, !data.Stats.Value[stat.Index].Value);
            }
        }

        private void OnLevelPointUpdated(object sender, DbEventArgs e)
        {
            if (data.LevelPoint.Value == CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).Value > 0) return;
            data.LevelPoint.Value = !data.LevelPoint.Value;
        }

        private void OnExpUpdated(object sender, DbEventArgs e)
        {
            if (data.LevelUp.Value == LevelController.I.CanLevelUp()) return;
            data.LevelUp.Value = !data.LevelUp.Value;
        }
        
        private void OnDungeonUpdated(object sender, DbEventArgs e)
        {
            var haveDungeon = false;
            foreach (var ticket in CurrencyController.data.Tickets)
            {
                if (ticket.Value.Value > 0)
                {
                    haveDungeon = true;
                    break;
                }
            }
            if (data.Dungeon.Value == haveDungeon) return;
            data.Dungeon.Value = !data.Dungeon.Value;
        }

        private void OnSalaryOrPromotionUpdated(object sender, DbEventArgs e)
        {
            var promotionMeta = DbPromotion.Get(LevelController.data.Promotion.Value + 1);
            var canChallengePromotion = promotionMeta != null &&
                                        promotionMeta.IsOnUI &&
                                        promotionMeta.UnlockCondition <= LevelController.data.MaxStage.Value;
            var update = canChallengePromotion;
            if (data.Promotion.Value == update) return;
            data.Promotion.Value = update;
        }

        private void OnSummonUpdated(object sender, DbEventArgs e)
        {
            if (data.Summon.Value == LevelController.I.CanAnyGetSummonReward()) return;
            data.Summon.Value = !data.Summon.Value;
        }

        private void OnNecklaceUpdated(object sender, DbEventArgs e)
        {
            if (data.Necklace.Value == NecklaceController.I.CanUpgrade.Value) return;
            data.Necklace.Value = !data.Necklace.Value;
        }
        
        private void OnTitleUpdated(object sender, DbEventArgs e)
        {
            var have = false;
            DbUserTitle.ForEach(t =>
            {
                have = have || t.CanHave;
            });
            if (have != data.Title.Value) data.Title.Value = have;
        }
        
        private void OnQuestUpdated(object sender, DbEventArgs e)
        {
            var q = DbUserQuest.Get(e.Id);
            var canRewarded = q.CanRewarded;
            if (canRewarded == data.Quests.Value[e.Id].Value) return;
            data.Quests.SetValue(e.Id, canRewarded);
        }
        
        private void OnWeaponUpdated(object sender, DbEventArgs e)
        {
            data.Weapons.SetValue(e.Id-1, true);
            DbUserWeapon.Get(e.Id).Have.ValueChanged -= OnWeaponUpdated;
        }
        
        private void OnAccessoryUpdated(object sender, DbEventArgs e)
        {
            data.Accessories.SetValue(e.Id-1, true);
            DbUserAccessory.Get(e.Id).Have.ValueChanged -= OnWeaponUpdated;
        }
        
        private void OnRelicUpdated(object sender, DbEventArgs e)
        {
            data.Relics.SetValue(e.Id-1, DbUserRelic.Get(e.Id).Count.Value > 0);
        }

        public void OnAttendUpdated(object sender, DbEventArgs e)
        {
            var canAttend = AttendController.I.CanRewarded.Value;
            if (canAttend != data.Attend.Value) data.Attend.Value = canAttend;
        }
        
        private void OnSkillUpdated(object sender, DbEventArgs e)
        {
            data.Skills.SetValue(e.Id-1, true);
            DbUserSkill.Get(e.Id).Have.ValueChanged -= OnWeaponUpdated;
        }
        private void OnInGameShopsUpdated(object sender, DbEventArgs e)
        {
            var have = false;
            DbInGameShop.ForEach(AddNeedBadge);

            void AddNeedBadge(DbInGameShop inGameShop)
            {
                have = have || (inGameShop.PriceType == CurrencyType.Ad && CurrencyController.I.CanBuy(inGameShop));
            }

            data.InGameShop.Value = have;
        }

        public void OnBookUpdated(object sender, DbEventArgs e)
        {
            for (var idx = CurrencyType.NormalBook1; idx <= CurrencyType.NormalBook4; ++idx)
            {
                if (CurrencyController.I.GetBookModel(idx).Value > 0)
                {
                    data.Book.Value = true;
                    return;
                }
            }
            
            var haveAnyBook = false;
            
            for (var idx = CurrencyType.SealedBook1; idx <= CurrencyType.SealedBook4; ++idx)
            {
                if (CurrencyController.I.GetBookModel(idx).Value > 0)
                {
                    haveAnyBook = true;
                    break;
                }
            }
            
            var bookshelves = CurrencyController.data.BookShelves.Value;
            for (var idx = 0; idx < bookshelves.Count; ++idx)
            {
                var bookshelf = bookshelves[idx].Value;
                if (CurrencyController.I.Have(CurrencyType.Bookshelf1 + idx))
                {
                    if (haveAnyBook && (bookshelf.LeftTime <= 0 || !bookshelf.HaveBook.Value))
                    {
                        data.Book.Value = true;
                        return;
                    }

                    if (bookshelf.CanOpen.Value)
                    {
                        data.Book.Value = true;
                        return;
                    }
                }
                
            }

            data.Book.Value = false;
        }
        
        private void CheckWeapon(int id)
        {
            data.Weapons.SetValue(id - 1, false);
        }
        private void CheckAccessory(int id)
        {
            data.Accessories.SetValue(id - 1, false);
        }
        public void CheckSkill(int id)
        {
            data.Skills.SetValue(id - 1, false);
        }
        
        public bool IsStatBadgeOn()
        {
            var count = data.Stats.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Stats.Value[idx].Value) return true;
            }

            return false;
        }

        public bool IsQuestBadgeOn(int id)
        {
            return data.Quests.Value[id].Value;
        }
        
        public bool IsQuestBadgeOn()
        {
            var count = data.Quests.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Quests.Value[idx].Value) return true;
            }

            return false;
        }
        
        public bool IsWeaponBadgeOn()
        {
            var count = data.Weapons.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Weapons.Value[idx].Value) return true;
            }

            return false;
        }
        
        public bool IsAccessoryBadgeOn()
        {
            var count = data.Accessories.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Accessories.Value[idx].Value) return true;
            }

            return false;
        }
        
        public bool IsRelicBadgeOn()
        {
            var count = data.Relics.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Relics.Value[idx].Value) return true;
            }

            return false;
        }
        
        public bool IsSkillBadgeOn()
        {
            var count = data.Skills.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (data.Skills.Value[idx].Value) return true;
            }

            return false;
        }

        public bool IsQuestBadgeOn(QuestCycleType cycle)
        {
            var badges = data.Quests.Value;
            for (var idx = 0; idx < badges.Count; ++idx)
            {
                if (badges[idx].Value && DbQuest.Get(idx).Cycle.Equals(cycle)) return true;
            }

            return false;
        }
        
        private static List<DbField<bool>> GetAllStatBadges()
        {
            var badges = new List<DbField<bool>>();
            DbGoldStat.ForEach(AddNeedBadge);

            void AddNeedBadge(DbGoldStat stat)
            {
                badges.Add(new DbField<bool>(StatController.I.Get(stat.Id).CanLevelUp(1, true) == 1, 0, data));
            }

            return badges;
        }
        
        private static List<DbField<bool>> GetAllQuestBadges()
        {
            var badges = new List<DbField<bool>>();
            var count = DbQuest.Count;
            for (var id = 0; id < count; ++id)
            {
                AddNeedBadge(DbQuest.Get(id));
            }

            void AddNeedBadge(DbQuest quest)
            {
                badges.Add(new DbField<bool>(DbUserQuest.Get(quest.Id).CanRewarded, 0, data));
            }

            return badges;
        }
        
        private static List<DbField<bool>> GetAllWeaponsBadges()
        {
            var badges = new List<DbField<bool>>();
            DbUserWeapon.ForEach(AddNeedBadge);

            void AddNeedBadge(DbUserWeapon weapon)
            {
                badges.Add(new DbField<bool>(false, weapon.Id, data));
            }

            return badges;
        }
        
        private static List<DbField<bool>> GetAllAccessoriesBadges()
        {
            var badges = new List<DbField<bool>>();
            DbUserAccessory.ForEach(AddNeedBadge);

            void AddNeedBadge(DbUserAccessory accessory)
            {
                badges.Add(new DbField<bool>(false, accessory.Id, data));
            }

            return badges;
        }

        private static List<DbField<bool>> GetAllRelicsBadges()
        {
            var badges = new List<DbField<bool>>();
            DbUserRelic.ForEach(AddNeedBadge);

            void AddNeedBadge(DbUserRelic relic)
            {
                badges.Add(new DbField<bool>(relic.Count.Value > 0, relic.Id, data));
            }

            return badges;
        }
        
        private static List<DbField<bool>> GetAllSkillsBadges()
        {
            var badges = new List<DbField<bool>>();
            var prevId = 1;
            DbUserSkill.ForEach(AddNeedBadge);

            void AddNeedBadge(DbUserSkill skill)
            {
                for (var idx = prevId; idx < skill.Id; ++idx)
                {
                    badges.Add(new DbField<bool>(false, idx, data));
                }
                badges.Add(new DbField<bool>(false, skill.Id, data));
                prevId = skill.Id + 1;
            }

            return badges;
        }

        public void CheckEquipment(EquipmentType type, int id)
        {
            if (type == EquipmentType.Weapon)
            {
                if (data.Weapons.Value[id-1].Value) CheckWeapon(id);
            }
            else if (type == EquipmentType.Accessory)
            {
                if (data.Accessories.Value[id-1].Value) CheckAccessory(id);
            }
        }

        public bool IsEquipment(EquipmentType type, int id)
        {
            if (type == EquipmentType.Weapon)
            {
                return data.Weapons.Value[id - 1].Value;
            }

            if (type == EquipmentType.Accessory)
            {
                return data.Accessories.Value[id - 1].Value;
            }

            return data.Skills.Value[id - 1].Value;
        }

        public void CheckAllWeapon()
        {
            var count = data.Weapons.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                data.Weapons.SetValue(idx, false);
            }
        }
        
        public void CheckAllAccessory()
        {
            var count = data.Accessories.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                data.Accessories.SetValue(idx, false);
            }
        }
        
        public void CheckAllSkills()
        {
            var count = data.Skills.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                data.Skills.SetValue(idx, false);
            }
        }
    }
}