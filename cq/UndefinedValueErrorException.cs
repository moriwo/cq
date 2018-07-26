using System;

namespace cq
{
    public class UndefinedValueErrorException : System.Exception
    {
        public string Index { get; private set; }
        
        public UndefinedValueErrorException(string index)
        {
            Index = index;
        }
    }
}