using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using WindowsServer.Web;
using System.Web.Configuration;
using WindowsServer.Configuration;
using WindowsServer.Web.Filters;
using System.Web.Http;
using WindowsServer.Web.Caching;
using WindowsServer.Caching;
using System.Net;
using WindowsServer.ApiProxy.Controllers;

namespace System.Web
{
    public static class HttpApplicationExtensions
    {
        private static List<string> s_allowOrigins = null;
        private static string s_allowMethods = string.Empty;
        private static string s_allowHeaders = string.Empty;

        public static void InitializeAll(this HttpApplication application)
        {
            application.RegisterGlobalExceptionHandler();
            GlobalConfiguration.Configuration.Filters.Add(new WebApiGlobalExceptionFilterAttribute());

            ConfigurationCenter.Initialize(true, HttpContext.Current.Server.MapPath("~"));
            application.RegisterGeneralControllers(RouteTable.Routes);
            CacheManager.Initialize(new AspNetCacheContainer(HttpContext.Current.Cache));

            GlobalFilters.Filters.Add(new CompressFilter());

            bool enableActionDiagnoseFilter = bool.Parse(ConfigurationCenter.Global["EnableActionDiagnoseFilter"] ?? bool.FalseString);
            if (enableActionDiagnoseFilter)
            {
                GlobalFilters.Filters.Add(new ActionDiagnoseFilter());
            }

            bool enableActionLogFilter = bool.Parse(ConfigurationCenter.Global["EnableActionLogFilter"] ?? bool.FalseString);
            if (enableActionLogFilter)
            {
                GlobalFilters.Filters.Add(new ActionLogFilter());
            }

            bool requestTrack = bool.Parse(ConfigurationCenter.Global["RequestTracker"] ?? bool.FalseString);
            if (requestTrack)
            {
                GlobalFilters.Filters.Add(new RequestTrackerFilter());
                GlobalConfiguration.Configuration.Filters.Add(new WebApiRequestTrackerFilter());
            }

            bool baiduYunGuanCe = bool.Parse(ConfigurationCenter.Global["EnableBaiduYunGuanCeFilter"] ?? bool.FalseString);
            if (baiduYunGuanCe)
            {
                GlobalFilters.Filters.Add(new BaiduYunguanceFilter());
            }

            bool apiProxy = bool.Parse(ConfigurationCenter.Global["ApiProxy"] ?? bool.FalseString);
            if (apiProxy)
            {
                ApiProxyController.Enabled = true;
            }

            bool ignoreSSLValidation = bool.Parse(ConfigurationCenter.Global["IgnoreServerCertificateValidation"] ?? bool.TrueString);
            if (ignoreSSLValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (mender, certificate, chain, sslPolicyErrors) => true;
            }

            s_allowOrigins = (ConfigurationCenter.Global["AllowOrigins"] ?? "")
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t != string.Empty)
                .ToList();
            s_allowMethods = (ConfigurationCenter.Global["AllowMethods"] ?? "GET,POST,PUT,PATCH,CREATE,DELETE,OPTIONS");
            s_allowHeaders = (ConfigurationCenter.Global["AllowHeaders"] ?? "AccessToken,Content-Type,Accept");

            if (s_allowOrigins.Any())
            {
                var corsFilter = new CorsFilter()
                {
                    AllowAnyOrigin = s_allowOrigins.Any(t => t == "*"),
                    AllowOrigins = s_allowOrigins,
                    AllowMethods = s_allowMethods,
                    AllowHeaders = s_allowHeaders,
                };
                GlobalFilters.Filters.Add(corsFilter);

                var webApiCorsFilter = new WebApiCorsFilter()
                {
                    AllowAnyOrigin = s_allowOrigins.Any(t => t == "*"),
                    AllowOrigins = s_allowOrigins,
                    AllowMethods = s_allowMethods,
                    AllowHeaders = s_allowHeaders,
                };
                GlobalConfiguration.Configuration.Filters.Add(webApiCorsFilter);
            }
        }

        public static void RegisterGlobalExceptionHandler(this HttpApplication application)
        {
            GlobalExceptionHandler.Initialize(application);
        }

        public static void RegisterGeneralControllers(this HttpApplication application, RouteCollection routes)
        {
            bool managementConsole = bool.Parse(ConfigurationCenter.Global["ManagementConsole"] ?? bool.FalseString);
            if (managementConsole)
            {
                routes.MapRoute("CacheDisplayCacheHitsStartDiagnosing", "ManagementConsole/CacheHits/StartDiagnosing", new { Controller = "CacheManagement", action = "StartDiagnosing" });
                routes.MapRoute("CacheDisplayCacheHitsStopDiagnosing", "ManagementConsole/CacheHits/StopDiagnosing", new { Controller = "CacheManagement", action = "StopDiagnosing" });
                routes.MapRoute("CacheDisplayCacheHits", "ManagementConsole/CacheHits", new { Controller = "CacheManagement", action = "DisplayCacheHits" });

                routes.MapRoute("FilesManagementBrowseDirectory", "ManagementConsole/Files/B", new { Controller = "FilesManagement", action = "BrowseDirectory" });
                routes.MapRoute("FilesManagementDownloadFile", "ManagementConsole/Files/D", new { Controller = "FilesManagement", action = "DownloadFile" });
                routes.MapRoute("FilesManagementZipDirectory", "ManagementConsole/Files/Z", new { Controller = "FilesManagement", action = "ZipDirectory" });

                routes.MapRoute("DBManagementBrowseConnections", "ManagementConsole/DB/BC", new { Controller = "DBManagement", action = "BrowseConnections" });
                routes.MapRoute("DBManagementGetTables", "ManagementConsole/DB/T", new { Controller = "DBManagement", action = "GetTables" });
                routes.MapRoute("DBManagementGetRows", "ManagementConsole/DB/R", new { Controller = "DBManagement", action = "GetRows" });

                routes.MapRoute("SettingsManagementBrowseSettings", "ManagementConsole/Settings/B", new { Controller = "SettingsManagement", action = "BrowseSettings" });

                routes.MapRoute("ManagementConsoleSignIn", "ManagementConsole/SignIn", new { Controller = "ManagementConsole", action = "SignIn" });
                routes.MapRoute("ManagementConsoleGetPortal", "ManagementConsole/Portal", new { Controller = "ManagementConsole", action = "GetPortal" });
            }

            bool requestTrack = bool.Parse(ConfigurationCenter.Global["RequestTracker"] ?? bool.FalseString);
            if (managementConsole || requestTrack)
            {
                routes.MapRoute("RequestTrackerBrowseRequest", "RequestTracker/BR/{id}", new { Controller = "RequestTracker", action = "BrowseRequest" });
                routes.MapRoute("RequestTrackerBrowseRequests", "RequestTracker/B", new { Controller = "RequestTracker", action = "BrowseRequests" });
            }

            bool deploymentInformation = bool.Parse(ConfigurationCenter.Global["DeploymentInformation"] ?? bool.TrueString);
            if (deploymentInformation)
            {
                routes.MapRoute("DeploymentGetInformation", "Deployment/Info", new { Controller = "Deployment", action = "GetInformation" });
            }
        }

        public static void ProcessCorsPreflightRequest(this HttpApplication application)
        {
            var req = HttpContext.Current.Request;

            if (req.HttpMethod == "OPTIONS")
            {
                var res = HttpContext.Current.Response;
                if (s_allowOrigins.Any())
                {
                    var requestOrigin = req.Headers["Origin"];
                    var origin = s_allowOrigins.Any(t => t == "*")
                        ? CorsConstants.AnyOrigin
                        : s_allowOrigins.Contains(requestOrigin, StringComparer.InvariantCultureIgnoreCase) ? requestOrigin : string.Empty;
                    if (!string.IsNullOrEmpty(origin))
                    {
                        res.AppendHeader(CorsConstants.AccessControlAllowOrigin, origin);
                        res.AppendHeader(CorsConstants.AccessControlAllowCredentials, "true");
                        res.AppendHeader(CorsConstants.AccessControlAllowMethods, s_allowMethods);
                        res.AppendHeader(CorsConstants.AccessControlAllowHeaders, s_allowHeaders);
                        res.StatusCode = 200;
                        res.End();
                        return;
                    }
                }
                res.StatusCode = 400;
                res.End();
            }
        }
    }
}
