using System;

namespace ConnectFour.Persistence
{
    internal class SaveFileDataAcessException : Exception
    {
        public SaveFileDataAcessException() { }
        public SaveFileDataAcessException(string message) : base(message) { }
    }
}
