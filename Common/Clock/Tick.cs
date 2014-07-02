using System;

namespace Common.Clock
{
    public class Tick
    {
        private readonly DateTime _value;

        private Tick()
        {
            _value = DateTime.Now;
        }

        public static Tick Now
        {
            get { return new Tick(); }
        }

        public static int TotalSeconds(Tick t1, Tick t2)
        {
            return (int) (t1._value - t2._value).TotalSeconds;
        }

        public static int ElapsedSeconds(Tick t)
        {
            return (int) (DateTime.Now - t._value).TotalSeconds;
        }
    }
}
