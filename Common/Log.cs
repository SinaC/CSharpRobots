using System;

namespace Common
{
    public static class Log
    {
        public enum LogLevels
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("CSharpRobots");

        public static void Initialize(string path, string file, string fileTarget = "logfile")
        {
            string logfile = System.IO.Path.Combine(path, file);
            NLog.Targets.FileTarget target = NLog.LogManager.Configuration.FindTargetByName(fileTarget) as NLog.Targets.FileTarget;
            if (target == null)
                throw new ApplicationException(String.Format("Couldn't find target {0} in NLog config", fileTarget));
            target.FileName = logfile;
        }

        public static void WriteLine(LogLevels level, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.fff} - {1} - {2}", DateTime.Now, level, String.Format(format, args));
            switch (level)
            {
                case LogLevels.Debug:
                    Logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    Logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    Logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    Logger.Error(format, args);
                    break;
            }
        }
    }
}
