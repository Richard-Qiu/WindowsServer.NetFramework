using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private Func<IDictionary<string, object>, Task> _next;

        public ExceptionHandlerMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            this._next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                await _next(environment);
            }
            catch (Exception exception)
            {
                var contextString = GetContextString(environment);
                _logger.Info("Uncaught exception is detected.[" + contextString + "]", exception);
            }

            var internalException = EnvironmentHelper.GetApplicationInternalException(environment);
            if (internalException != null)
            {
                var contextString = GetContextString(environment);
                _logger.Info("Uncaught exception is detected.[" + contextString + "]", internalException as Exception);
            }
        }

        private static string GetContextString(IDictionary<string, object> context)
        {
            return string.Format(
                "ClientIP:{0} RawUrl:{1} {2} UserAgent:{3}",
                EnvironmentHelper.GetServerRemoteIpAddress(context, string.Empty),
                EnvironmentHelper.GetRequestMethod(context),
                EnvironmentHelper.GetRequestPathWithQueryString(context),
                EnvironmentHelper.GetRequestUserAgent(context));
        }

    }
}
