using System.Collections.Generic;
using Data.Editor.EDbPetInfo;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Pet")]
    public class EDbPets: ScriptableObject
    {
        [SerializeField] public List<EDbPet> Pet;
        [SerializeField] public List<EDbPetAwakening> PetAwakening;
        [SerializeField] public List<EDbPetAwakeningMaterial> PetAwakeningMaterial;
        [SerializeField] public List<EDbBook> Book;
        [SerializeField] public List<EDbPetGrowthMaterial> PetGrowthMaterial;
        [SerializeField] public List<EDbBibleLevel> BibleLevel;
        
    }
}