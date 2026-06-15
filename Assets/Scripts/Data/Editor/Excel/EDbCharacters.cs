using System.Collections.Generic;
using Data.DbCharacter;
using Data.Editor.EDbCharacter;
using UnityEngine;

namespace Data.Editor.Excel
{
	[ExcelAsset(ExcelName = "Character")]
	public class EDbCharacters : ScriptableObject
	{
		[SerializeField] public List<EDbGoldStat> GoldStat;
		[SerializeField] public List<EDbAttackLevel> AttackLevel;
		[SerializeField] public List<EDbHpLevel> HpLevel;
		[SerializeField] public List<EDbHpRecoveryLevel> HpRecoveryLevel;
		[SerializeField] public List<EDbCriticalAttackBonusLevel> CriticalAttackBonusLevel;
		[SerializeField] public List<EDbCriticalProbabilityLevel> CriticalProbabilityLevel;
		[SerializeField] public List<EDbAttackBonusLevel> AttackBonusLevel;
		[SerializeField] public List<EDbHpBonusLevel> HpBonusLevel;
		[SerializeField] public List<EDbBossAttackBonusLevel> BossAttackBonusLevel;
		[SerializeField] public List<EDbLevelPoint> LevelPoint;
		[SerializeField] public List<EDbCharacterLevel> CharacterLevel;
	}

}
