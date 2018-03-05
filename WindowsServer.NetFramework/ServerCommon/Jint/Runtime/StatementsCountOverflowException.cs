﻿using System;

namespace WindowsServer.Jint.Runtime
{
    public class StatementsCountOverflowException : Exception 
    {
        public StatementsCountOverflowException() : base("The maximum number of statements executed have been reached.")
        {
        }
    }
}
