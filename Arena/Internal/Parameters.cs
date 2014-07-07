using System.Collections.Generic;

namespace Arena.Internal
{
    public sealed class ParametersSingleton
    {
        #region Singleton

        private static readonly ParametersSingleton SingletonInstance = new ParametersSingleton();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ParametersSingleton()
        {
        }
        
        public static ParametersSingleton Instance
        {
            get { return SingletonInstance; }
        }

        #endregion

        // Arena
        public static readonly int StepDelay = 15; // !!! this delay is not guaranteed by windows when using System.Timers.Timer (not used anymore), we have to compute real elapsed delay between 2 steps (http://stackoverflow.com/questions/3744032/why-are-net-timers-limited-to-15-ms-resolution)
        public static readonly int ArenaSize = 1000;
        public static readonly double CollisionDistance = 1;
        public static readonly int CollisionDamage = 2;
        public static readonly int ExplosionDisplayDelay = 500;
        // Missile
        public static readonly int MissileSpeed = 300; // in m/s
        // Robot
        public static readonly double TrigonometricBias = 100000;
        public static readonly double MaxSpeed = 30; // in m/s
        public static readonly int MaxDamage = 100;
        public static readonly int MaxResolution = 20; // in degrees
        public static readonly int MaxCannonRange = 700; // in meters
        public static readonly int MaxAcceleration = 5; // acceleration factor in m/s
        public static readonly int MaxTurnSpeed = 50; // maximum speed for direction change

        private ParametersSingleton()
        {
            _parameters = new Dictionary<string, int>
                {
                    {"ArenaSize", 1000},
                    {"CollisionDamage", 2},
                    {"MissileSpeed", 300},
                    {"MaxSpeed", 30},
                    {"MaxDamage", 100},
                    {"MaxResolution", 20},
                    {"MaxCannonRange", 700},
                    {"MaxAcceleration", 5},
                    {"MaxTurnSpeed", 50},
                    {"MaxExplosionRange", 40}
                };
        }

        private readonly Dictionary<string, int> _parameters;

        public IReadOnlyDictionary<string, int> Parameters
        {
            get { return _parameters; }
        }
    }
}
