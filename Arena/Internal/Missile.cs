using Common;

namespace Arena.Internal
{
    internal class Missile : IReadonlyMissile
    {
        private const double Tolerance = 0.00001;

        private readonly Tick _launchTick;
        private readonly Tick _matchStart;

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
        int IReadonlyMissile.LaunchLocX
        {
            get { return (int) LaunchLocX; }
        }

        int IReadonlyMissile.LaunchLocY
        {
            get { return (int) LaunchLocY; }
        }

        public int Heading { get; private set; }
        public int Range { get; private set; }

        // Current location
        int IReadonlyMissile.LocX
        {
            get { return (int) LocX; }
        }

        int IReadonlyMissile.LocY
        {
            get { return (int) LocY; }
        }

        #endregion

        internal Missile(IReadonlyRobot robot, Tick matchStart, int id, double locX, double locY, int heading, int range)
        {
            _launchTick = Tick.Now;

            Robot = robot;
            _matchStart = matchStart;

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
            CurrentDistance += (ParametersSingleton.MissileSpeed*realStepTime)/1000.0;
            if (CurrentDistance > Range) // if missile goes too far, get it back :)
                CurrentDistance = Range;
            // Update location
            double newLocX, newLocY;
            Math.ComputePoint(LaunchLocX, LaunchLocY, CurrentDistance, Heading, out newLocX, out newLocY);
            LocX = newLocX;
            LocY = newLocY;

            //Log.WriteLine(Log.LogLevels.Debug, "Missile {0} location updated. CurrentDistance {1} LaunchX {2} LaunchY {3} LocX {4} LocY {5} Heading {6}", Id, CurrentDistance, LaunchLocX, LaunchLocY, LocX, LocY, Heading);
        }

        public void TargetReached()
        {
            //// Check speed
            //double elapsed = Tick.ElapsedMilliseconds(_launchTick);
            //double diffX = LocX - LaunchLocX;
            //double diffY = LocY - LaunchLocY;
            //double distance = System.Math.Sqrt(diffX*diffX + diffY*diffY);
            //double speed = distance/elapsed*1000.0; // in m/s
            //double tick = Tick.ElapsedSeconds(_matchStart);
            //Log.WriteLine(Log.LogLevels.Debug, "Missile {0} : {1:0.00}  | target reached. Speed {2:0.000} Distance {3:0.000} Range {4}  loc:{5:0.000},{6:0.000}", Id, tick, speed, distance, Range, LocX, LocY);

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

    internal class PreciseMissile : IReadonlyMissile
    {
        private const double Tolerance = 0.00001;

        private readonly IReadonlyRobot _robot;
        private readonly int _id;
        private readonly Tick _launchTick;
        private readonly Tick _matchStart;
        private readonly double _launchLocX;
        private readonly double _launchLocY;
        private readonly double _launchHeading;
        private readonly double _launchRange;
        private readonly double _cosDriveAngle;
        private readonly double _sinDriveAngle;

        private MissileStates _state;
        private double _remainingRange;
        private double _locX;
        private double _locY;
        private double _explodingTime;
        private Tick _explosionTick;

        public double ExplodingTime
        {
            get { return _explodingTime; }
        }

        public PreciseMissile(IReadonlyRobot robot, Tick matchStart, int id, double locX, double locY, double heading, double range)
        {
            _launchTick = Tick.Now;

            _robot = robot;
            _matchStart = matchStart;

            _id = id;

            _launchLocX = locX;
            _launchLocY = locY;
            _launchHeading = heading;
            _launchRange = range;

            _locX = locX;
            _locY = locY;
            _remainingRange = range;

            double radians = Math.ToRadians(heading);
            _cosDriveAngle = System.Math.Cos(radians);
            _sinDriveAngle = System.Math.Sin(radians);

            _state = MissileStates.Flying;
        }

        #region IReadonlyMissile

        public int Id
        {
            get { return _id; }
        }

        public IReadonlyRobot Robot
        {
            get { return _robot; }
        }

        public MissileStates State
        {
            get { return _state; }
        }

        public int LaunchLocX
        {
            get { return (int) System.Math.Round(_launchLocX); }
        }

        public int LaunchLocY
        {
            get { return (int) System.Math.Round(_launchLocY); }
        }

        public int Heading
        {
            get { return (int) System.Math.Round(_launchHeading); }
        }

        public int Range
        {
            get { return (int) System.Math.Round(_launchRange); }
        }

        public int LocX
        {
            get { return (int) System.Math.Round(_locX); }
        }

        public int LocY
        {
            get { return (int) System.Math.Round(_locY); }
        }

        #endregion

        public void Update(double dt)
        {
            double travelledDistance = dt*ParametersSingleton.MissileSpeed; // distance increase due to speed d = v.t
            if (_remainingRange > travelledDistance) // target not reached
            {
                _remainingRange -= travelledDistance; // update remaining distance to travel
                _locX += travelledDistance*_cosDriveAngle; // relocation along x axis
                _locY += travelledDistance*_sinDriveAngle; // relocation along y axis
            }
            else // target reached, explode
            {
                _explosionTick = Tick.Now;
                _explodingTime = _remainingRange/ParametersSingleton.MissileSpeed; // exploding time
                _locX += _remainingRange*_cosDriveAngle; // relocation along x axis
                _locY += _remainingRange*_sinDriveAngle; // relocation along y axis
                _state = MissileStates.Exploding;
            }

            // TODO: compute impact explosion time
            if (_locX < 0)
            {
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    _locY = _locY - _locX*_sinDriveAngle/_cosDriveAngle;
                _locX = 0;
            }
            else if (_locX > ParametersSingleton.ArenaSize)
            {
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    _locY = _locY + (ParametersSingleton.ArenaSize - _locX)*_sinDriveAngle/_cosDriveAngle;
                _locX = ParametersSingleton.ArenaSize;
            }
            if (_locY < 0)
            {
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    _locX = _locX - _locY*_cosDriveAngle/_sinDriveAngle;
                _locY = 0;
            }
            else if (_locY > ParametersSingleton.ArenaSize)
            {
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    _locX = _locX + (ParametersSingleton.ArenaSize - _locY)*_cosDriveAngle/_sinDriveAngle;
                _locY = ParametersSingleton.ArenaSize;
            }
        }

        public void ExplosionHandled()
        {
            _state = MissileStates.Exploded;
        }

        public void UpdateExploded()
        {
            if (Tick.ElapsedMilliseconds(_explosionTick) > ParametersSingleton.ExplosionDisplayDelay)
                _state = MissileStates.Deleted;
        }
    }
}
