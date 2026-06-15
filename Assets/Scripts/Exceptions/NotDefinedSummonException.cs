using System;
using Data;

namespace Exceptions
{
    public class NotDefinedSummonException: Exception
    {
        public NotDefinedSummonException(SummonType summon)
            : base($"{summon} is not defined in this function")
        {
        }
    }
}