using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CSharpRobotsWPF
{
    public static class LoadRobots
    {
        public static List<Type> LoadRobotsFromPath(string path)
        {
            List<Type> types = new List<Type>();
            Type robotSDKType = typeof (SDK.Robot);
            foreach (string file in Directory.EnumerateFiles(path, "*.dll"))
            {
                Assembly assembly = Assembly.LoadFile(file);
                if (assembly != null)
                    types.AddRange(assembly.GetExportedTypes().Where(x => x.IsSubclassOf(robotSDKType) && !x.IsDefined(typeof(SDK.BrokenAttribute))));
            }
            return types;
        }

        public static List<Type> LoadRobotsFromFile(string filename)
        {
            List<Type> types = new List<Type>();
            Type robotSDKType = typeof (SDK.Robot);
            Assembly assembly = Assembly.LoadFile(filename);
            if (assembly != null)
                types.AddRange(assembly.GetExportedTypes().Where(x => x.IsSubclassOf(robotSDKType) && !x.IsDefined(typeof(SDK.BrokenAttribute))));
            return types;
        }

        public static Type LoadRobotFromPath(string path, string robotName)
        {
            Type robotSDKType = typeof(SDK.Robot);

            foreach (string file in Directory.EnumerateFiles(path, "*.dll"))
            {
                Assembly assembly = Assembly.LoadFile(file);

                Type type = assembly.GetExportedTypes().FirstOrDefault(x => x.Name == robotName && x.IsSubclassOf(robotSDKType) && !x.IsDefined(typeof(SDK.BrokenAttribute)));
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
