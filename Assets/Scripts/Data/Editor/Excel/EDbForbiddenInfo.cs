using System;
using System.Collections.Generic;
using Data.Editor.EDbEvent;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Forbidden")]
    public class EDbForbiddenInfo: ScriptableObject
    {
        [SerializeField] public List<EDbForbidden> Forbidden;
    }
    
    [Serializable]
    public class EDbForbidden
    {
        public int Id;
        public string Word;
    }
}