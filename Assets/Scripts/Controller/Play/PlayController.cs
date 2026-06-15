using System.Collections.Generic;
using System.Numerics;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbStage;
using Data.DbUser.Etc;
using Data.Utils;
using Exceptions;
using Managers;
using MEC;
using UIs.FieldMain.MainSkill;
using Utils;

namespace Controller.Play
{
    public class PlayController: Singleton<PlayController>
    {
        public static DbUserPlay data = DbUserPlay.Get(0);

        private static CoroutineHandle _timeLimitHandle;

        public bool isPowerSave;
        public long powerSavingGold;
        public long powerSavingExp;
        public long powerSavingWeapon;
        public long powerSavingAccessory;
        public long powerSavingWeaponGrowth;
        public long powerSavingAccessoryGrowth;
        public long powerSavingBeadsOre;

        public long sequenceReward;
        public ControllerField<BigInteger> damage = new(0);

        public bool useAdTicket = false;
        
        public void PowerSaveOnOff(bool isOn)
        {
            isPowerSave = isOn;
            powerSavingExp = 0;
            powerSavingGold = 0;
            powerSavingWeapon = 0;
            powerSavingAccessory = 0;
            powerSavingWeaponGrowth = 0;
            powerSavingAccessoryGrowth = 0;
            powerSavingBeadsOre = 0;
        }


        private bool _readyToMoveNext = false;
        private int _curStage = -1;
        public void KillMonster(FieldType field, int stage)
        {
            var isClear = false;
            switch (field)
            {
                case FieldType.Stage:
                    QuestController.I.DoQuests(QuestType.MonsterKillCount);
                    if (Manager.Field.StageMeta.GetStageType() == StageType.Boss) QuestController.I.DoQuests(QuestType.BossKillCount);
                    data.KillCount.Value++;
                    isClear = data.CheckKillCount.Value && IsClear();
                    if (isPowerSave)
                    {
                        var stageReward = DbStageReward.Get(LevelController.data.Stage.Value);
                        powerSavingGold += CurrencyController.I.GetMonsterKillRewardGold(stageReward);
                        powerSavingWeaponGrowth += CurrencyController.I.GetMonsterKillRewardWGS(stageReward);
                        powerSavingAccessoryGrowth += CurrencyController.I.GetMonsterKillRewardAGS(stageReward);
                        powerSavingBeadsOre += CurrencyController.I.GetMonsterKillRewardBeadsOre(stageReward);
                        powerSavingExp += LevelController.I.GetMonsterKillReward();
                        powerSavingWeapon += WeaponController.I.GetMonsterKillReward();
                        powerSavingAccessory += AccessoryController.I.GetMonsterKillReward();
                    }
                    else
                    {
                        CurrencyController.I.GetMonsterKillReward();
                        LevelController.I.GetMonsterKillReward();
                        WeaponController.I.GetMonsterKillReward();
                        AccessoryController.I.GetMonsterKillReward();
                    }
                    CurrencyController.I.AddSoulPassProgress();
                    break;
                case FieldType.Awakening:
                case FieldType.Promotion:
                case FieldType.SkillGrowth:
                    data.KillCount.Value++;
                    isClear = IsClear();
                    break;
                case FieldType.Dia:
                case FieldType.BlackMarket:
                    sequenceReward += DbSelector.GetReward(field, stage).GetMonsterReward();
                    if (!Manager.Field.HaveMonster())
                    {
                        if (_readyToMoveNext && _curStage == stage) return; 
                        _readyToMoveNext = true;
                        _curStage = stage;
                        if (Manager.Field.CurStage == DbSelector.GetMaxStage(field))
                        {
                            Manager.Field.GameOver(GameOverType.DungeonFail, 3.25f, field);
                        }
                        else
                        {
                            Manager.Field.MoveStageInDungeon(Manager.Field.CurStage+1);
                        }
                    }
                    else
                    {
                        _readyToMoveNext = false;
                        _curStage = -1;
                    }
                    break;
                case FieldType.Pet:
                    isClear = IsClear();
                    break;
                default: throw new NotDefinedFieldException(field);
            }

            if (isClear)
            {
                data.CheckKillCount.Value = false;
                Manager.Field.GameOver(GameOverType.Success, 3.25f, Manager.Field.CurField.Value);
            }
        }

        private bool IsClear()
        {
            var goal = Manager.Field.StageMeta.GetStageGoalCount();
            return goal > 0 && data.KillCount.Value >= goal;
        }

        public void MonsterCountChanged(int count)
        {
            data.MonsterCount.Value = count;
        }

        public void KillTimeLimit()
        {
            Timing.KillCoroutines(_timeLimitHandle);
        }

        public void OnStageRefreshed()
        {
            Timing.KillCoroutines(_timeLimitHandle);
            
            data.KillCount.Value = 0;
            data.CheckKillCount.Value = Manager.Field.CurField.Value != FieldType.Stage || !LevelController.data.IsStageClear.Value;
            data.TimeLimit.Value = DbStageBase.Get(Manager.Field.StageMeta.GetStageType()).TimeLimit;
            _readyToMoveNext = false;
            _curStage = -1;

            if (data.CheckKillCount.Value)
            {
                _timeLimitHandle = Timing.RunCoroutine(_TimeLimitRoutine());
            }

            if (Manager.Field.StageMeta.IsBoss())
            {
                var hp = Manager.Field.StageMeta.GetMonsterHp() * (1000 - TotalStatController.I.GetStat(StatType.DebuffMonsterHp)) / 1000;
                data.RedBarMaxProgress = hp;
                data.RedBarProgress.Value = hp;
            }
            else if (Manager.Field.StageMeta.GetStageType() == StageType.Defense)
            {
                var hp = Manager.Bible.GetMaxHp();
                data.RedBarMaxProgress = hp;
                data.RedBarProgress.Value = hp;
            }
        }

        public void SetRedProgress(BigInteger hp)
        {
            data.RedBarProgress.Value = hp;
        }
        
        IEnumerator<float> _TimeLimitRoutine()
        {
            if (data.TimeLimit.Value < 0) yield break;
            
            while (data.TimeLimit.Value > 0)
            {
                yield return Timing.WaitForSeconds(1);
                data.TimeLimit.Value--;
            }

            var curField = Manager.Field.CurField.Value;
            switch (curField)
            {
                case FieldType.Stage:
                    Manager.Field.GameOver(GameOverType.StageFail, 3.25f, curField);
                    break;
                case FieldType.Awakening:
                case FieldType.SkillGrowth:
                case FieldType.Promotion:
                    Manager.Field.GameOver(GameOverType.DungeonFail, 3.25f, curField);
                    break;
                case FieldType.Pet:
                    Manager.Field.GameOver(GameOverType.Success, 3.25f, curField);
                    break;
                case FieldType.BlackMarket:
                case FieldType.Dia:
                    Manager.Field.GameOver(GameOverType.DungeonFail, 3.25f, curField);
                    break;
                case FieldType.Training:
                    Manager.Field.GetFirst().PlayAnimation("Die");
                    Manager.Player.SetWait();
                    Manager.UI.GetSceneUI<UI_MainSkill>().StopSkillTimers();
                    Timing.CallDelayed(2f, () => Manager.Field.GameOver(GameOverType.Success, -1, curField));
                    break;
                default:
                    throw new NotDefinedFieldException(curField);
            }
        }
    }
}