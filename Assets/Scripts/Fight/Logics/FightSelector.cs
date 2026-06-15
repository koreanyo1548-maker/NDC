using System;
using System.Collections.Generic;
using Data;
using Exceptions;
using Fight.Logics.Actors;
using Fight.Logics.Spawners;
using Fight.Logics.TargetSelectors;
using Fight.Units;

namespace Fight.Logics
{
    public static class FightSelector
    {
        enum SpawnerType
        {
            Normal,
            Center,
            Side
        }

        private static Dictionary<SpawnerType, ISpawner> _spawners = new();
        public static ISpawner GetSpawner(FieldType fieldType, bool isCenter, float boundaryX, float boundaryY)
        {
            if (isCenter) return Get(SpawnerType.Center, typeof(CenterSpawner));
            switch (fieldType)
            {
                case FieldType.Stage: 
                case FieldType.Promotion:
                case FieldType.Awakening: 
                case FieldType.SkillGrowth:
                case FieldType.BlackMarket:
                case FieldType.Dia:
                case FieldType.Training:
                    return Get(SpawnerType.Normal, typeof(NormalSpawner));
                case FieldType.Pet:
                    return Get(SpawnerType.Side, typeof(SideSpawner));
                default: 
                    throw new NotDefinedFieldException(fieldType);
            }

            ISpawner Get(SpawnerType type, Type spawner)
            {
                if (_spawners.TryGetValue(type, out var spawner1)) return spawner1;

                _spawners.Add(type, Activator.CreateInstance(spawner, boundaryX, boundaryY) as ISpawner);
                return _spawners[type];
            }
        }
        
        enum TargetSelectorType
        {
            Player,
            Bible,
            NearToPlayer,
            NearToBible
        }
        
        private static Dictionary<TargetSelectorType, ITargetSelector> _targetSelectors = new();

        public static ITargetSelector GetTargetSelector(FieldType fieldType, bool isMonster)
        {
            switch (fieldType)
            {
                case FieldType.Stage: 
                case FieldType.Promotion:
                case FieldType.Awakening: 
                case FieldType.SkillGrowth:
                case FieldType.BlackMarket:
                case FieldType.Dia:
                case FieldType.Training:
                    if (isMonster) return Get(TargetSelectorType.Player, typeof(PlayerSelector));
                    return Get(TargetSelectorType.NearToPlayer, typeof(NearPlayerSelector));
                case FieldType.Pet:
                    if (isMonster) return Get(TargetSelectorType.Bible, typeof(BibleSelector));
                    return Get(TargetSelectorType.NearToBible, typeof(NearBibleSelector));
                default:
                    throw new NotDefinedFieldException(fieldType);
            }

            ITargetSelector Get(TargetSelectorType type, Type spawner)
            {
                if (_targetSelectors.TryGetValue(type, out var selector)) return selector;
                
                _targetSelectors.Add(type, Activator.CreateInstance(spawner) as ITargetSelector);
                return _targetSelectors[type];
            }
        }
        

        public static IActor GetActor(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Stage: 
                case FieldType.Promotion:
                case FieldType.Awakening: 
                case FieldType.SkillGrowth:
                case FieldType.BlackMarket:
                case FieldType.Dia:
                case FieldType.Pet:
                    return Get(typeof(FightingMonsterActor));
                case FieldType.Training:
                    return Get(typeof(TrainingMonsterActor));
                default:
                    throw new NotDefinedFieldException(fieldType);
            }

            IActor Get( Type actor)
            {
                return Activator.CreateInstance(actor) as IActor;
            }
        }
    }
}