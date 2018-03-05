using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.ApiProxy.Models;

namespace WindowsServer.ApiProxy.Controllers
{
    internal static class Formats
    {
        public static readonly InvokeContext.Format BatchInvokeResult = new InvokeContext.Format()
        {
            Trace = true,
            InvokeItems = new InvokeItem.Format()
            {
                Ready = true,
                Completed = true,
                Response = new InvokeResponse.Format()
                {
                    StatusCode = true,
                    Headers = true,
                    Body = true,
                },
            },
        };

        public static readonly InvokeContext.Format BatchInvokeResultWithRequest = new InvokeContext.Format()
        {
            Trace = true,
            InvokeItems = new InvokeItem.Format()
            {
                Ready = true,
                Completed = true,
                Request = new InvokeRequest.Format()
                {
                    Url = true,
                    Method = true,
                    Headers = true,
                    Body = true,
                },
                Response = new InvokeResponse.Format()
                {
                    StatusCode = true,
                    Headers = true,
                    Body = true,
                },
            },
        };
    }
}
