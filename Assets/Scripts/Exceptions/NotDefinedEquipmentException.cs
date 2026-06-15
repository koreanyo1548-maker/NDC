using System;
using Data;

namespace Exceptions
{
    public class NotDefinedEquipmentException: Exception
    {
        public NotDefinedEquipmentException(CurrencyType equipment)
            : base($"{equipment} is not defined in this function")
        {
        }
        public NotDefinedEquipmentException(EquipmentType equipment)
            : base($"{equipment} is not defined in this function")
        {
        }
    }
}