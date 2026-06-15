using System;
using Data;

namespace Exceptions
{
    public class NotDefinedCurrencyException: Exception
    {
        public NotDefinedCurrencyException(CurrencyType currency)
            : base($"{currency} is not defined in this function")
        {
        }
    }
}