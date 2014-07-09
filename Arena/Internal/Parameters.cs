﻿using System.Collections.Generic;

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
        public const int StepDelay = 15;
        public const int ArenaSize = 1000;
        public const double CollisionDistance = 1;
        public const int CollisionDamage = 2;
        public const int ExplosionDisplayDelay = 500;
        // Missile
        public const int MissileSpeed = 300; // in m/s
        // Robot
        public const double TrigonometricBias = 100000;
        public const double MaxSpeed = 30; // in m/s
        public const int MaxDamage = 100;
        public const int MaxResolution = 20; // in degrees
        public const int MaxCannonRange = 700; // in meters
        public const int MaxAcceleration = 5; // acceleration factor in m/s
        public const int MaxTurnSpeed = 50; // maximum speed for direction change

        private ParametersSingleton()
        {
            _parameters = new Dictionary<string, int>
                {
                    {"ArenaSize", ArenaSize},
                    {"CollisionDamage", CollisionDamage},
                    {"MissileSpeed", MissileSpeed},
                    {"MaxSpeed", (int)MaxSpeed},
                    {"MaxDamage", MaxDamage},
                    {"MaxResolution", MaxResolution},
                    {"MaxCannonRange", MaxCannonRange},
                    {"MaxAcceleration", MaxAcceleration},
                    {"MaxTurnSpeed", MaxTurnSpeed},
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
