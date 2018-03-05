using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Middlewares;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using WindowsServer.Configuration;
using WindowsServer.Caching;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using WindowsServer.Http;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

namespace Owin
{
    public static class IAppBuilderExtensions
    {
        public static IAppBuilder InitializeAll(this IAppBuilder builder)
        {
            ConfigurationCenter.Initialize();
            CacheManager.Initialize();

            builder = builder.UseExceptionHandler();

            //application.RegisterGeneralControllers(RouteTable.Routes);

            //GlobalFilters.Filters.Add(new CompressFilter());

            //bool enableActionDiagnoseFilter = bool.Parse(WebConfigurationManager.AppSettings["EnableActionDiagnoseFilter"] ?? bool.FalseString);
            //if (enableActionDiagnoseFilter)
            //{
            //    GlobalFilters.Filters.Add(new ActionDiagnoseFilter());
            //}

            //bool enableActionLogFilter = bool.Parse(WebConfigurationManager.AppSettings["EnableActionLogFilter"] ?? bool.FalseString);
            //if (enableActionLogFilter)
            //{
            //    GlobalFilters.Filters.Add(new ActionLogFilter());
            //}

            bool requestTrack = bool.Parse(ConfigurationCenter.Local["RequestTracker"] ?? bool.FalseString);
            if (requestTrack)
            {
                builder.Use<RequestTrackerMiddleware>();
            }

            //bool responseCompression = bool.Parse(ConfigurationCenter.Local["ResponseCompression"] ?? bool.TrueString);
            //if (responseCompression)
            //{
            //    builder.Use<StaticCompressionMiddleware>(new StaticCompressionOptions());
            //}

            builder = builder.UseStageMarker(PipelineStage.Authenticate);

            return builder;
        }

        public static IAppBuilder UseWebApiExceptionLogger(this IAppBuilder builder, HttpConfiguration webapiConfiguration)
        {
            webapiConfiguration.Services.Add(typeof(IExceptionLogger), new HttpExceptionLogger());
            //webapiConfiguration.MessageHandlers.Add(new ServerCompressionHandler(new DeflateCompressor()));
            return builder.UseWebApi(webapiConfiguration);
        }

        public static IAppBuilder UseExceptionHandler(this IAppBuilder builder)
        {
            return builder.Use<ExceptionHandlerMiddleware>();
        }
    }
}
