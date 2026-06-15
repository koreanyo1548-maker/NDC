using System;
using System.Collections.Generic;
using System.Linq;
using Cameras;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbDefinition;
using Data.DbPromote;
using Data.DbShop;
using Data.DbStage;

using Data.Utils;
using DG.Tweening;
using Exceptions;
using Fight.Logics;
using Fight.Logics.Spawners;
using Fight.Units;
using MEC;
using UIs.FieldMain;
using UIs.FieldMain.MainSkill;
using UIs.FieldMain.MainStage;
using UIs.Shop.EventPackage;
using UIs.StageResult;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace Managers.Game
{
    public class FieldManager
    {
        public bool IsInit;

        #region Screen Width & Height
        public float MaxX => _width;
        public float MinY => -_height;

        public float CenterX => _width * 0.5f;
        public float CenterY => -_height * 0.5f;

        private float _width = 24.32f;
        //private float _height = 8.33f; //6.84f;
        private float _height = 6f;

        private float _boundaryX = 24.29f;
        private float _boundaryY = -5.34f;
        
        #endregion
        

        #region Monsters & Map

        private int _nextMonsterId = 0;
        private Dictionary<int, Monster> _monsters = new();
        private GameObject _monsterParent;
        public GameObject MonsterParent
        {
            get
            {
                if (_monsterParent != null) return _monsterParent;

                _monsterParent = GameObject.Find("@Monster_Root");
                if (_monsterParent == null)
                {
                    _monsterParent = new GameObject {name = "@Monster_Root"};
                    IsInit = true;
                }

                return _monsterParent;
            }
        }

        private DieEffect _dieEffect;

        private GameObject _map;
        private SpriteColorSetter _spriteColorSetter;
        public SpriteColorSetter Map
        {
            get
            {
                if (_spriteColorSetter != null) return _spriteColorSetter;
                _spriteColorSetter = _map.GetComponent<SpriteColorSetter>();
                return _spriteColorSetter;
            }
        }

        #endregion
        

        #region Field Info

        public ControllerField<FieldType> CurField { get; private set; }
        public int CurStage { get; private set; }
        public UIField<bool> StageChanged { get; private set; }
        public IDbStage StageMeta => CurField.Value == FieldType.Stage ? LevelController.stageMeta : _stageMeta;
        private IDbStage _stageMeta;

        public string GetStageName()
        {
            switch (CurField.Value)
            {
                case FieldType.Stage:
                    if (StageMeta.GetStageType() == StageType.Boss) return LevelController.stageMeta.GetBossName(); 
                    return string.Format(LocalString.Get(210017), LevelController.data.Stage.Value);
                case FieldType.Promotion: case FieldType.Pet: case FieldType.Awakening: case FieldType.SkillGrowth:
                    return LocalString.Get(DbDungeonMeta.Get(CurField.Value).NameId);
                case FieldType.BlackMarket:
                case FieldType.Dia:
                    return string.Format(LocalString.Get(210019), Manager.Field.CurStage);
                default:
                    throw new NotDefinedFieldException(CurField.Value);
            }
        }
        #endregion
        

        #region Game Info
        
        public bool IsGameOver => _isGameOver;
        private bool _isGameOver = false;
        private bool _isBigWave;

        #endregion


        #region Tools

        private ISpawner _spawner;

        #endregion


        #region Logics

        private CoroutineHandle _resetHandle;
        private CoroutineHandle _startSpawnHandle;
        private CoroutineHandle _spawningHandle;

        #endregion

        
        #region Delayed Game Over

        private GameOverType _gameOverReason;
        private int _restartDelay;

        #endregion
        
        
        public FieldManager()
        {
            CurField = new ControllerField<FieldType>(FieldType.Stage);
            StageChanged = new UIField<bool>(false);
        }
        
        public void Init()
        {
            _dieEffect = GameObject.Find("@DieEffect").GetComponent<DieEffect>();
            _dieEffect.gameObject.SetActive(false);
            SetMap();
            _isGameOver = true;
        }
        
        public void MonsterDie(int id)
        {
            if (_isGameOver) return;
            lock (_monsters)
            {
                _monsters.Remove(id);
                PlayController.I.MonsterCountChanged(_monsters.Count);
            }
        }

        /// <param name="restartDelay"> -1인 경우는 해당 함수에서 싸움을 재시작 하지 않는다. (연출에서 재시작) </param>
        public void GameOver(GameOverType gameOverReason, float restartDelay, FieldType prevField)
        {
            if (_isGameOver && gameOverReason != GameOverType.StageMove) return;
            
            _isGameOver = true;
            PlayController.I.KillTimeLimit();

            Timing.KillCoroutines(_resetHandle);
            Timing.KillCoroutines(_startSpawnHandle);
            Timing.KillCoroutines(Define.KillWhenPlayerDieTag);
            if (prevField == FieldType.Pet) Manager.Bible.UseDone();
            Manager.Skill.ClearAll();
            
            var childs = _monsterParent.GetComponentsInChildren<Monster>();
            foreach (var monster in childs) monster.Clear();
            var isDie = Manager.Player.GameOver();
            Manager.UI.GetSceneUI<UI_MainStage>().WhenGameOver();
            
            if (gameOverReason == GameOverType.DungeonFail
                || (gameOverReason == GameOverType.Success && CurField.Value != FieldType.Stage)) 
                CurField.Value = FieldType.Stage;

            if (gameOverReason == GameOverType.Success)
            {
                var waitTime = LevelController.I.OnStageClear(prevField, CurStage);
                restartDelay = waitTime;
            }
            else
            {
                LevelController.I.OnStageFailed(prevField, gameOverReason);
            }
            
            FadeInOut(isDie);
            if (!isDie && restartDelay > 3f) restartDelay = 2;
            _resetHandle = Timing.CallDelayed(1, () => ResetGame(isDie));
            if (restartDelay != -1) _startSpawnHandle = Timing.CallDelayed(restartDelay, () => SpawnGame());
            Manager.UI.GetSceneUI<UI_MainSkill>().StopSkillTimers();
            Manager.Sound.PlayBGM(StageMeta.GetBGM());
        }

        
        #region Move Stage

        public void EnterDungeon(FieldType fieldType, int enterStage)
        {
            var prevField = CurField.Value;
            _stageMeta = fieldType == FieldType.Stage ? null : DbSelector.GetStage(fieldType, enterStage);
            CurStage = enterStage;
            CurField.Value = fieldType;
            GameOver(GameOverType.StageMove, fieldType == FieldType.Promotion || fieldType == FieldType.Training ? -1 : 3.25f, prevField);
        }

        public void GiveUpDungeon()
        {
            var prevField = CurField.Value;
            _stageMeta = null;
            CurField.Value = FieldType.Stage;
            GameOver(GameOverType.GiveUp, 3.25f, prevField);
        }

        public void MoveStageInDungeon(int enterStage)
        {
            _stageMeta = DbSelector.GetStage(CurField.Value, enterStage);
            CurStage = enterStage;

            lock (_monsters)
            {
                _monsters.Clear();
                
                Timing.KillCoroutines(_spawningHandle);
                Timing.KillCoroutines(_startSpawnHandle);
                Manager.UI.GetSceneUI<UI_MainStage>().WhenStageChanged();
                _startSpawnHandle = Timing.CallDelayed(0, () => SpawnGame(false, false, false));
            }
        }

        private void ResetGame(bool isDie)
        {
            lock (_monsters)
            {
                _monsters.Clear();
                
                PlayController.I.MonsterCountChanged(_monsters.Count);
                if (CurField.Value == FieldType.Pet) SpawnBible();

                SetMap();
                if (isDie)_resetHandle = Timing.CallDelayed(1.25f, SetPlayer);
                else SetPlayer();
            }
        }
        
        #endregion
        
        
        #region Spawning

        public void SpawnGame(bool isFirst = false, bool checkBoss = true, bool checkSkillPreset = true)
        {
            var childs = MonsterParent.GetComponentsInChildren<Monster>();
            foreach (var monster in childs) monster.Clear();
            
            if (checkBoss && StageMeta.GetStageType() == StageType.Boss && Manager.Field.CurField.Value == FieldType.Stage)
            {
                Manager.UI.ShowSingleUI<UI_BossStage>().SetToast(210030);
                return;
            }
            
            PlayController.I.OnStageRefreshed();
            
            _isGameOver = false;
            _isBigWave = false;

            _spawner = FightSelector.GetSpawner(CurField.Value, DbStageBase.Get(StageMeta.GetStageType()).IsCenterSpawn, _boundaryX, _boundaryY);
            
            _spawningHandle = Timing.RunCoroutine(_MonsterSpawnRoutine(), Define.KillWhenPlayerDieTag);
            if (checkSkillPreset) EquipController.I.ChangeCurSkillPreset(CurField.Value, StageMeta.GetStageType() == StageType.Boss);
            if (!isFirst)
            {
                StageChanged.Value = !StageChanged.Value;
                //.UI.GetSceneUI<UI_MainSkill>().RestartSkillTimers();
                Manager.UI.GetSceneUI<UI_MainStage>().WhenStageChanged();
            }
            
            SetBigWave();
        }

        private void SetMap()
        { 
            var scale = Mathf.Max(1, _width / 40f);
            
            if (_map != null) _map.GetComponent<SpriteColorSetter>().StopFading();
            Manager.Resource.Destroy(_map);
            _map = Manager.Resource.Instantiate("Backgrounds/Chapter" + StageMeta.GetBackground());
            _map.transform.position = new Vector3(CenterX, CenterY + 3.44f, 0);
            _map.transform.localScale = new Vector3(scale, scale, 1);
            _map.GetOrAddComponent<SpriteColorSetter>();
        }

        public void SetPlayer()
        {
            Manager.Player.Spawn(CenterX, StageMeta.IsBoss()? CenterY + 0.4f : CenterY+1.4f);
        }

        private void SpawnBible()
        {
            Manager.Resource.Instantiate("Characters/Bible", 1, MonsterParent.transform)
                .GetComponent<Bible>().Spawn(CenterX, CenterY+1.4f);
        }

        IEnumerator<float> _MonsterSpawnRoutine()
        {
            if (_isGameOver) yield break;
            var interval = StageMeta.GetSpawnCoolTime(_isBigWave);
            while (true)
            {
                SpawnMonsters();
                if (interval == -1) yield break;
                yield return Timing.WaitForSeconds(interval);
            }
        }

        private void SetBigWave()
        {
            if (CurField.Value != FieldType.Stage 
                || LevelController.data.Stage.Value <= LevelController.data.MaxStage.Value
                || StageMeta.GetStageType() == StageType.Boss)
                return;
            Timing.RunCoroutine(_BigWaveRoutine(), Define.KillWhenPlayerDieTag);
        }

        IEnumerator<float> _BigWaveRoutine()
        {
            yield return Timing.WaitForSeconds(DbPlay.Get(PlayType.BigWaveStartTime).Value);
            var toast = Manager.UI.ShowSingleUI<UI_Toast>();
            toast.SetText(200008);
            
            yield return Timing.WaitForSeconds(2);
            
            if (_isGameOver) yield break;
            _isBigWave = true;
            Timing.KillCoroutines(_spawningHandle);
            _spawningHandle = Timing.RunCoroutine(_MonsterSpawnRoutine(), Define.KillWhenPlayerDieTag);
            
            yield return Timing.WaitForSeconds(DbPlay.Get(PlayType.BigWaveDuration).Value);
            
            if (_isGameOver) yield break;
            _isBigWave = false;
            Timing.KillCoroutines(_spawningHandle);
            _spawningHandle = Timing.RunCoroutine(_MonsterSpawnRoutine(), Define.KillWhenPlayerDieTag);
        }

        private void SpawnMonsters()
        {
            lock (_monsters)
            {
                var maxCount = StageMeta.GetMaxMonsterCount();
                if (_monsters.Count + StageMeta.GetMinSpawnCount(_isBigWave) > maxCount) return;
                var count = StageMeta.GetSpawnCount(_isBigWave);
                var poses = _spawner.GetRandomPos(count);
            
                while (count-- > 0)
                {
                    if (_isGameOver) return;
                    var monsterMeta = StageMeta.GetRandomMonster();
                    if (++_nextMonsterId > 10000) _nextMonsterId = 0;
                    while (_monsters.ContainsKey(_nextMonsterId))
                    {
                        _nextMonsterId++;
                    }
                    _monsters.Add(_nextMonsterId, Manager.Resource.Instantiate
                            ("Characters/" + monsterMeta.Resource, 5, MonsterParent.transform)
                        .transform.GetChild(0).gameObject.GetOrAddComponent<Monster>());
                    var monster = _monsters[_nextMonsterId];
                    monster.Spawn(poses[count].x , poses[count].y );
                    monster.Init(_nextMonsterId, monsterMeta, StageMeta.IsBoss());
                }

                PlayController.I.MonsterCountChanged(_monsters.Count);
            
                Manager.Player.SetTarget();
            }
        }
        
        #endregion

        
        #region GetMonsters

        public Monster GetFirst()
        {
            lock (_monsters)
            {
                if (_monsters.Count == 0) return null;
                return _monsters.ElementAt(0).Value;
            }
        }
        public bool HaveMonster()
        {
            lock (_monsters)
            {
                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    return true;
                }

                return false;
            }
        }
        
        public Monster GetNearestMonster(Vector3 pos, float sqrThreshold, float sqrMaxFindRange = float.MaxValue)
        {
            lock (_monsters)
            {
                var sqrMin = float.MaxValue;
                var minId = -1;
                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    var sqrDis = (pos - monster.Value.Position()).sqrMagnitude;
                    if (sqrDis < sqrMin && sqrDis < sqrMaxFindRange)
                    {
                        sqrMin = sqrDis;
                        minId = monster.Key;
                    }

                    if (sqrMin < sqrThreshold)
                    {
                        return _monsters[minId];
                    }
                }
                
                if (minId == -1) return null;
                return _monsters[minId];
            }
        }

        public bool HaveMonsterInRange(float range)
        {
            lock (_monsters)
            {
                var sqr = range * range;
                var isAll = range < 0;

                var compare = Manager.Player.Position();

                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    if (isAll || (monster.Value.Position() - compare).sqrMagnitude < sqr)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Vector3 ForEachMonstersInRange(float range, TargetBaseType targetBase, int count, Action<Monster> toDo)
        {
            lock (_monsters)
            {
                if (_monsters.Count == 0) return default;
                            
                var sqr = range * range;
                var isAll = range < 0;
    
                var compare = GetTargetBasePosition(targetBase);
                
                if (count == -1)
                {
                    foreach (var monster in _monsters)
                    {
                        if (monster.Value.IsDead) continue;
                        if (isAll || (monster.Value.Position() - compare).sqrMagnitude < (monster.Value.isBoss ? sqr * 3.25f : sqr))
                        {
                            toDo(monster.Value);
                        }
                    }
                }
                else
                {
                    var monsters = new List<Monster>();
    
                    foreach (var monster in _monsters)
                    {
                        if (monster.Value.IsDead) continue;
                        if (isAll || (monster.Value.Position() - compare).sqrMagnitude < sqr)
                        {
                            monsters.Add(monster.Value);
                        }
    
                        if (monsters.Count == count) break;
                    }
    
                    if (monsters.Count == 0)
                    {
                        return compare;
                    }
    
                    var idx = 0;
                    while (monsters.Count < count)
                    {
                        monsters.Add(monsters[idx++]);
                        if (idx == monsters.Count) idx = 0;
                    }
    
                    foreach (var monster in monsters)
                    {
                        if (monster.IsDead) continue;
                        toDo(monster);
                    }
                }
                
                return compare;
            }
            
        } 
        
        public Vector3 ForEachMonstersInRange(float range, Transform targetBase, int count, Action<Monster> toDo)
        {
            lock (_monsters)
            {
                if (_monsters.Count == 0) return default;
                            
                var sqr = range * range;
                var isAll = range < 0;
                
                if (count == -1)
                {
                    foreach (var monster in _monsters)
                    {
                        if (monster.Value.IsDead) continue;
                        if (isAll || (monster.Value.Position() - targetBase.position).sqrMagnitude < (monster.Value.isBoss ? sqr * 3.25f : sqr))
                        {
                            toDo(monster.Value);
                        }
                    }
                }
                else
                {
                    var monsters = new List<Monster>();
    
                    foreach (var monster in _monsters)
                    {
                        if (monster.Value.IsDead) continue;
                        if (isAll || (monster.Value.Position() - targetBase.position).sqrMagnitude < sqr)
                        {
                            monsters.Add(monster.Value);
                        }
    
                        if (monsters.Count == count) break;
                    }
    
                    if (monsters.Count == 0)
                    {
                        return targetBase.position;
                    }
    
                    var idx = 0;
                    while (monsters.Count < count)
                    {
                        monsters.Add(monsters[idx++]);
                        if (idx == monsters.Count) idx = 0;
                    }
    
                    foreach (var monster in monsters)
                    {
                        if (monster.IsDead) continue;
                        toDo(monster);
                    }
                }
                return targetBase.position;
            }
        }

        private Vector3 GetTargetBasePosition(TargetBaseType baseType)
        {
            Vector3 compare;
            if (baseType == TargetBaseType.Target)
            {
                var nearest = GetNearestMonster(Manager.Player.Position(), 0.1f);
                if (nearest == null) compare = Manager.Player.Position();
                else compare = nearest.Position();
            }
            else
            {
                compare = Manager.Player.Position();
            }

            return compare;
        }

        public bool HaveMonsterInStraight(float range, float widthRange)
        {
            lock (_monsters)
            {
                var sqr = range * range;
                var isAll = range < 0;
                var mine = Manager.Player.Position();
                var you = GetNearestMonster(Manager.Player.Position(), 0.1f);
                if (you == null) return false;
                var yours = you.Position();
            
                var gradient = (yours.y - mine.y) / (yours.x - mine.x);
                var reverseGradient = (yours.x - mine.x) / (mine.y - yours.y);

                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    if (IsInLine(monster.Value.Position()) && (isAll || (monster.Value.Position() - mine).sqrMagnitude < sqr))
                    {
                        return true;
                    }
                }
                return false;
            
                bool IsInLine(Vector3 position)
                {
                    return Mathf.Pow(-gradient * position.x + position.y + gradient * mine.x - mine.y, 2)
                           <= Mathf.Pow(widthRange * 0.5f, 2) * (gradient * gradient + 1) &&
                           reverseGradient * (position.x - mine.x) + mine.y < position.y == mine.y < yours.y;
                }
            }
        }
        
        public void ForEachMonstersInNormalAttackRange(Vector3 myPosition, bool isTargetXLarge, float range, Action<Monster> toDo)
        {
            lock (_monsters)
            {
                var count = 0;
                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    if ((isTargetXLarge == monster.Value.Position().x > myPosition.x) && (monster.Value.Position() - myPosition).sqrMagnitude <= range)
                    {
                        count++;
                        toDo(monster.Value);
                        if (count == (int)DbPlay.Get(PlayType.MultipleAttackMaxCount).Value) break;
                    }
                }
            }
        }

        public Vector3 ForEachMonstersInStraight(float range, float widthRange, Vector3 reference, Action<Monster> toDo)
        {
            lock (_monsters)
            {
                var sqr = range * range;
                var isAll = range < 0;
                var mine = Manager.Player.Position();
                
                Vector3 yours;
                if (reference == default)
                {
                    var you = GetNearestMonster(Manager.Player.Position(), 0.1f);
                    if (you == null) return default;
                    yours = you.Position();
                }
                else
                {
                    yours = reference;
                }
    
                var gradient = (yours.y - mine.y) / (yours.x - mine.x);
                var reverseGradient = (yours.x - mine.x) / (mine.y - yours.y);
    
                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    if (IsInLine(monster.Value.Position()) && (isAll || (monster.Value.Position() - mine).sqrMagnitude < (monster.Value.isBoss ? sqr * 3.25f : sqr)))
                    {
                        toDo(monster.Value);
                    }
                }
                return yours;
                
                bool IsInLine(Vector3 position)
                {
                   return Mathf.Pow(-gradient * position.x + position.y + gradient * mine.x - mine.y, 2)
                            <= Mathf.Pow(widthRange * 0.5f, 2) * (gradient * gradient + 1) &&
                           reverseGradient * (position.x - mine.x) + mine.y < position.y == mine.y < yours.y;
                }
            }
        }
        
        public Vector3 ForEachNearestMonsters(int count, Action<Monster> toDo)
        {
            lock (_monsters)
            {
                if (_monsters.Count == 0) return default;
                            
                var monsters = new List<Monster>();
                var exceptions = new List<int>();
    
                Monster nearest = null;
                while (monsters.Count < count && HaveMonsterAlive())
                {
                    nearest = GetNearestMonsterExcept(Manager.Player.Position(), 0.1f, exceptions);
                    if (nearest == null)
                    {
                        exceptions.Clear();
                        continue;
                    }
                    monsters.Add(nearest);
                    exceptions.Add(nearest.id);
                }
    
                foreach (var monster in monsters)
                {
                    if (monster.IsDead) continue;
                    toDo(monster);
                }
    
                return nearest == null ? default : nearest.Position();
                
                bool HaveMonsterAlive()
                {
                    if (_monsters.Count == 0) return false;
                    foreach (var m in _monsters)
                    {
                        if (!m.Value.IsDead)
                            return true;
                    }
                    return false;
                }
            }
        }
        
        private Monster GetNearestMonsterExcept(Vector3 pos, float sqrThreshold, List<int> exceptions)
        {
            lock (_monsters)
            {
                var sqrMin = float.MaxValue;
                var minId = -1;
                foreach (var monster in _monsters)
                {
                    if (monster.Value.IsDead) continue;
                    if (exceptions.Contains(monster.Key)) continue;
                    if (monster.Value.IsDead) continue;
                    var sqrDis = (pos - monster.Value.Position()).sqrMagnitude;
                    if (sqrDis < sqrMin)
                    {
                        sqrMin = sqrDis;
                        minId = monster.Key;
                    }

                    if (sqrMin < sqrThreshold)
                    {
                        return _monsters[minId];
                    }
                }
            
                if (minId == -1) return null;
                return _monsters[minId];
            }
        }

        public Vector3 GetInnerDiff(Vector3 diff, Vector3 position)
        {
            var final = diff + position;
            if (final.x < 0) diff.x = -position.x;
            else if (final.x > _boundaryX) diff.x = _boundaryX - position.x;
            if (final.y < _boundaryY) diff.y = _boundaryY - position.y;
            else if (final.y > 0) diff.y = -position.y;

            return diff;
        }

        #endregion
        
        private void FadeInOut(bool isDie)
        {
            _dieEffect.Effect(isDie);
        }
    }
}
