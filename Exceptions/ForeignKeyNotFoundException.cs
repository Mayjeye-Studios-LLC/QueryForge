using System;

namespace QueryForge.Exceptions
{
    public class ForeignKeyNotFoundException:Exception
    {
        public ForeignKeyNotFoundException(string message) : base(message)
        {
            
        }
    }
}