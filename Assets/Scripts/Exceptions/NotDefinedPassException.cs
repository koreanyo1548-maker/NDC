using System;
using Data;

namespace Exceptions
{
    public class NotDefinedPassException: Exception
    {
        public NotDefinedPassException(PassType pass)
            : base($"{pass} is not defined in this function")
        {
        }
    }
}