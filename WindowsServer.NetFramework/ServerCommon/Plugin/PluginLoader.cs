using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.Plugin
{
    public class PluginLoader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string _pluginsPath;

        public PluginLoader(string pluginsPath)
        {
            this._pluginsPath = pluginsPath;
        }

        public List<T> Load<T>() where T : class
        {
            var plugins = new List<T>();

            if (!Directory.Exists(_pluginsPath))
            {
                _logger.Warn("The plugins directory " + _pluginsPath + " does not exist.");
                return plugins;
            }

            var subDirectories = Directory.EnumerateDirectories(_pluginsPath);
            foreach (var subDirectory in subDirectories)
            {
                var name = Path.GetFileName(subDirectory);
                var dll = Path.Combine(subDirectory, name + ".dll");
                var pdb = Path.Combine(subDirectory, name + ".pdb");

                // If there is no such a DLL which has the same name with the containing directory, ignore
                if (!File.Exists(dll))
                {
                    continue;
                }

                // Load the dll and its symbol file if it exists
                Assembly assembly;
                var dllBytes = File.ReadAllBytes(dll);
                if (File.Exists(pdb))
                {
                    var pdbBytes = File.ReadAllBytes(pdb);
                    assembly = AppDomain.CurrentDomain.Load(dllBytes, pdbBytes);
                }
                else
                {
                    assembly = AppDomain.CurrentDomain.Load(dllBytes);
                }

                // Try to find the classes which implements 'T'
                var typeOfT = typeof(T);
                var types = assembly.DefinedTypes.Where(t =>
                    {
                        return ((!t.IsAbstract)
                            && (t.IsClass)
                            && (t.IsPublic)
                            && (t.ImplementedInterfaces.Any(i => i == typeOfT)));
                    }).ToList();
                if (types.Count <= 0)
                {
                    _logger.Warn("The assembly(" + dll + ") does not have any public class which implements the interface(" + typeOfT.ToString() + ").");
                    continue;
                }

                // Create instances
                foreach (var type in types)
                {
                    try
                    {
                        var plugin = assembly.CreateInstance(type.FullName) as T;
                        plugins.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("Failed to create plugin instance of type " + type.FullName + ". A parameterless constructor is needed for this plugin class.", ex);
                    }
                }
            }

            return plugins;
        }
    }
}
