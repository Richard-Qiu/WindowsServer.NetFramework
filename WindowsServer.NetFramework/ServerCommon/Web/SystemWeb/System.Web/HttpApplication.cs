using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System;

namespace WindowsServer.Web
{
	internal class HttpApplication
	{
		internal static Type LoadType (string typeName)
		{
			return LoadType (typeName, false);
		}
		
		internal static Type LoadType (string typeName, bool throwOnMissing)
		{
			Type type = Type.GetType (typeName);
			if (type != null)
				return type;

			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly ass in assemblies) {
				type = ass.GetType (typeName, false);
				if (type != null)
					return type;
			}

			if (type != null)
				return type;
			if (throwOnMissing)
				throw new TypeLoadException (String.Format ("Type '{0}' cannot be found", typeName));
			
			return null;
		}

		internal static Type LoadType <TBaseType> (string typeName, bool throwOnMissing)
		{
			Type ret = LoadType (typeName, throwOnMissing);

			if (typeof (TBaseType).IsAssignableFrom (ret))
				return ret;

			if (throwOnMissing)
				throw new TypeLoadException (String.Format ("Type '{0}' found but it doesn't derive from base type '{1}'.", typeName, typeof (TBaseType)));

			return null;
		}
		
	}

}
