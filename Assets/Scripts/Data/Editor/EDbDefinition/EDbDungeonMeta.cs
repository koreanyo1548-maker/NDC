using System;

namespace Data.Editor.EDbDefinition
{
    [Serializable]
    public class EDbDungeonMeta
    {
        public FieldType Id;
        public int NameId;
        public CurrencyType Use;
        public CurrencyType AdUse;
        public string Resource;
    }
}