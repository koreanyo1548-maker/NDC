using System;

namespace Exceptions
{
    public class NotDefinedValueException: Exception
    {
        public NotDefinedValueException(string value)
            : base($"{value} is not defined value")
        {
        }
    }
}