using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using WindowsServer.Log;

namespace WindowsServer.Http
{
    internal class HttpExceptionLogger : IExceptionLogger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task LogAsync(ExceptionLoggerContext context, System.Threading.CancellationToken cancellationToken)
        {
            if (context.Exception != null)
            {
                if (context.Request != null)
                {
                    var owinContext = context.Request.Properties["MS_OwinContext"] as OwinContext;
                    if (owinContext != null)
                    {
                        EnvironmentHelper.SetApplicationInternalException(owinContext.Environment, context.Exception);
                    }
                    else
                    {
                        _logger.ErrorException("Uncaught exception is detected, but owinContext is null.", context.Exception);
                    }
                }
                else
                {
                    _logger.ErrorException("Uncaught exception is detected, but context.Request is null.", context.Exception);
                }
            }
            else
            {
                _logger.ErrorException("Uncaught exception is detected, but context.Request is null.", context.Exception);
            }
            await Task.FromResult(0);
        }

    }
}
