using System.Collections.Generic;
using Data.Editor.EDbAbility;
using Data.Editor.EDbCharacter;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Ability")]
    public class EDbAbilities: ScriptableObject
    {
        [SerializeField] public List<EDbAbilityRune> AbilityRune;
        [SerializeField] public List<EDbAbilityOption> AbilityOption;
        [SerializeField] public List<EDbAbilityOptionSummon> AbilityOptionSummon;
    }
}