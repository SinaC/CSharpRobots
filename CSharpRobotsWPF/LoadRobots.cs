using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SDK;

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

                foreach (Type type in assembly.GetExportedTypes().Where(x => x.IsSubclassOf(robotSDKType)))
                {
                    types.Add(type);
                }
            }
            return types;
        }
    }
}
