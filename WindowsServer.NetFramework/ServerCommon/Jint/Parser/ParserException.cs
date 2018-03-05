using System;

namespace WindowsServer.Jint.Parser
{
    public class ParserException : Exception
    {
        public int Column;
        public string Description;
        public int Index;
        public int LineNumber;
#pragma warning disable 0108
        public string Source;
#pragma warning restore 0108

        public ParserException(string message) : base(message)
        {
        }
    }
}