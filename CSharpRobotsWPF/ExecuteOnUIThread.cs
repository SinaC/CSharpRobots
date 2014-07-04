﻿using System;
using System.Windows.Threading;

namespace CSharpRobotsWPF
{
    public static class ExecuteOnUIThread
    {
        private static Dispatcher _uiDispatcher;

        public static void Initialize()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.Invoke(action, priority);
            }
            catch (Exception ex)
            {
                //Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }

        public static void InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.InvokeAsync(action, priority);
            }
            catch (Exception ex)
            {
                //Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }
    }
}
