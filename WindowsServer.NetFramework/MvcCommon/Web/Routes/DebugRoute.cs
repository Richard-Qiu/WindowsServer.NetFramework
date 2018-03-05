using System.Web.Routing;

namespace WindowsServer.Web.Routes
{
    public class DebugRoute : Route
    {
        private static DebugRoute singleton = new DebugRoute();

        public static DebugRoute Singleton
        {
            get { return singleton; }
        }

        private DebugRoute()
            : base("{*catchall}", new DebugRouteHandler())
        { }
    }
}
