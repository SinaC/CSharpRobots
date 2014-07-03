using System;
using System.Diagnostics;

namespace Common.Clock
{
    public class Tick
    {
        private readonly DateTime _value;

        private Tick()
        {
            _value = HiResDateTime.UtcNow;
        }

        public static Tick Now
        {
            get { return new Tick(); }
        }

        public static double TotalSeconds(Tick t1, Tick t2)
        {
            return (t1._value - t2._value).TotalSeconds;
        }

        public static double ElapsedSeconds(Tick t)
        {
            return (HiResDateTime.UtcNow - t._value).TotalSeconds;
        }

        public static double TotalMilliseconds(Tick t1, Tick t2)
        {
            return (t1._value - t2._value).TotalMilliseconds;
        }

        public static double ElapsedMilliseconds(Tick t)
        {
            return (HiResDateTime.UtcNow - t._value).TotalMilliseconds;
        }
    }

    //http://stackoverflow.com/questions/1416139/how-to-get-timestamp-of-tick-precision-in-net-c
    internal static class HiResDateTime
    {
        private static DateTime _startTime;
        private static Stopwatch _stopWatch;
        private static readonly TimeSpan MaxIdle = TimeSpan.FromSeconds(10);

        public static DateTime UtcNow
        {
            get
            {
                if (_stopWatch == null || _startTime.Add(MaxIdle) < DateTime.UtcNow)
                    Reset();
                return _startTime.AddTicks(_stopWatch.Elapsed.Ticks);
            }
        }

        private static void Reset()
        {
            _startTime = DateTime.UtcNow;
            _stopWatch = Stopwatch.StartNew();
        }
    }
}
