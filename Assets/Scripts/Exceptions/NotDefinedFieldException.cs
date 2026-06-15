using System;
using Data;
using Utils;

namespace Exceptions
{
    public class NotDefinedFieldException : Exception
    {
        public NotDefinedFieldException(FieldType field)
            : base($"{field} is not defined in this function")
        {
        }
    }
}