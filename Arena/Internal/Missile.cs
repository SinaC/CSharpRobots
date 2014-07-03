using Common.Clock;

namespace Arena.Internal
{
    internal class Missile : IReadonlyMissile
    {
        public static readonly int MissileSpeed = 300; // in m/s

        private Arena _arena;

        // When a missile has exploded, it stays in state Explosed during x milliseconds
        private Tick _explosionTick;

        // Current distance
        internal double CurrentDistance { get; private set; }

        public double LaunchLocX { get; private set; }
        public double LaunchLocY { get; private set; }

        // Current location
        public double LocX { get; private set; }
        public double LocY { get; private set; }

        #region IReadonlyMissile

        // State
        public MissileStates State { get; private set; }

        // Robot shooting the missile
        public IReadonlyRobot Robot { get; private set; }

        // Launch location, heading, range
        int IReadonlyMissile.LaunchLocX { get { return (int)LaunchLocX; } }
        int IReadonlyMissile.LaunchLocY { get { return (int)LaunchLocY; } }

        public int Heading { get; private set; }
        public int Range { get; private set; }

        // Current location
        int IReadonlyMissile.LocX { get { return (int) LocX; } }
        int IReadonlyMissile.LocY { get { return (int)LocY; } }

        #endregion

        internal Missile(Arena arena, IReadonlyRobot robot, double locX, double locY, int heading, int range)
        {
            _arena = arena;

            Robot = robot;
            
            LaunchLocX = locX;
            LaunchLocY = locY;
            Heading = heading;
            Range = range;

            LocX = locX;
            LocY = locY;
            CurrentDistance = 0;

            State = MissileStates.Flying;
        }

        public void UpdatePosition()
        {
            // Update distance
            CurrentDistance += (MissileSpeed*_arena.StepDelay)/1000.0;
            if (CurrentDistance > Range) // if missile goes too far, get it back :)
                CurrentDistance = Range;
            // Update location
            double newLocX, newLocY;
            Common.Helpers.Math.ComputePoint(LaunchLocX, LaunchLocY, CurrentDistance, Heading, out newLocX, out newLocY);
            LocX = newLocX;
            LocY = newLocY;
        }

        public void TargetReached()
        {
            State = MissileStates.Exploding;
        }

        public void CollisionWall(double newLocX, double newLocY)
        {
            State = MissileStates.Exploding;
            LocX = newLocX;
            LocY = newLocY;
        }

        public void UpdateExploding()
        {
            _explosionTick = Tick.Now;
            State = MissileStates.Exploded;
        }

        public void UpdateExploded(int explosionDisplayDelay)
        {
            if (Tick.ElapsedMilliseconds(_explosionTick) > explosionDisplayDelay)
                State = MissileStates.Deleted;
        }
    }
}
