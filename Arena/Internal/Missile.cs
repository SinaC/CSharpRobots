using Common.Clock;

namespace Arena.Internal
{
    internal class Missile : IReadonlyMissile
    {
        public static readonly int MissileSpeed = 300; // in m/s

        private readonly Tick _launchTick;

        // When a missile has exploded, it stays in state Explosed during x milliseconds
        private Tick _explosionTick;

        // Current distance
        public double CurrentDistance { get; private set; }

        public double LaunchLocX { get; private set; }
        public double LaunchLocY { get; private set; }

        // Current location
        public double LocX { get; private set; }
        public double LocY { get; private set; }

        #region IReadonlyMissile

        // Id
        public int Id { get; private set; }

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

        internal Missile(IReadonlyRobot robot, int id, double locX, double locY, int heading, int range)
        {
            _launchTick = Tick.Now;

            Robot = robot;

            Id = id;

            LaunchLocX = locX;
            LaunchLocY = locY;
            Heading = heading;
            Range = range;

            LocX = locX;
            LocY = locY;
            CurrentDistance = 0;

            State = MissileStates.Flying;
        }

        public void UpdatePosition(double realStepTime)
        {
            // Update distance
            CurrentDistance += (MissileSpeed * realStepTime) / 1000.0;
            if (CurrentDistance > Range) // if missile goes too far, get it back :)
                CurrentDistance = Range;
            // Update location
            double newLocX, newLocY;
            Common.Helpers.Math.ComputePoint(LaunchLocX, LaunchLocY, CurrentDistance, Heading, out newLocX, out newLocY);
            LocX = newLocX;
            LocY = newLocY;

            //System.Diagnostics.Debug.WriteLine("Missile {0} location updated. CurrentDistance {1} LaunchX {2} LaunchY {3} LocX {4} LocY {5} Heading {6}", Id, CurrentDistance, LaunchLocX, LaunchLocY, LocX, LocY, Heading);
        }

        public void TargetReached()
        {
            // Check speed
            double elapsed = Tick.ElapsedMilliseconds(_launchTick);
            double diffX = LocX - LaunchLocX;
            double diffY = LocY - LaunchLocY;
            double distance = System.Math.Sqrt(diffX*diffX + diffY*diffY);
            double speed = distance/elapsed*1000.0; // in m/s
            System.Diagnostics.Debug.WriteLine("Missile {0} target reached. Speed {1} Distance {2} Range {3}", Id, speed, distance, Range);

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
