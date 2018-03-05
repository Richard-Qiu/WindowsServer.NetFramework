using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServer.DataBase
{
    public class DbExecutionParameters
    {
        public int? ConnectionTimeout { get; set; }
        public int? CommandTimeout { get; set; }
    }
}
