using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.DataBase
{
    public class SingleSaasDbSelector<T> : SaasDbSelector<T>
    {
        public string ConnectionString { get; private set; }

        public SingleSaasDbSelector(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public override string GetConnectionString(T arg)
        {
            return this.ConnectionString;
        }
    }
}
