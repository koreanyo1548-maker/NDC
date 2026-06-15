using System;
using Data;

namespace Exceptions
{
    public class NotDefinedLockConditionException: Exception
    {
        public NotDefinedLockConditionException(LockConditionType lockCondition)
            : base($"{lockCondition} is not defined in this function")
        {
        }
    }
}