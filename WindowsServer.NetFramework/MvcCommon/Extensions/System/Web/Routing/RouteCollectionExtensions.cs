using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace System.Web.Routing
{
    public static class RouteCollectionExtensions
    {
        public static void IgnoreRouteForCommonFiles(this RouteCollection routes, params string[] extraFileNames)
        {
            List<string> fileNames = new List<string>();
            fileNames.Add("clientaccesspolicy.xml");
            fileNames.Add("favicon.ico");
            fileNames.AddRange(extraFileNames);
            IgnoreRouteForFiles(routes, fileNames.ToArray());
        }

        public static void IgnoreRouteForFiles(this RouteCollection routes, params string[] fileNames)
        {
            for (int i = 0; i < fileNames.Length; i++)
            {
                routes.IgnoreRoute("{*ignoreFile}", new { ignoreFile = @"(.*/)?" + fileNames[i] + "(/.*)?" });
            }
        }

    }
}
