using System;

namespace cq
{
    public class UndefinedValueErrorException : Exception
    {
        public string Index { get; }
        
        public UndefinedValueErrorException(string index)
        {
            Index = index;
        }
    }
}