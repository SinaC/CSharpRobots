using Common;

namespace Arena.Internal.JRobots
{
    internal class Missile : IReadonlyMissile
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
        private readonly double _explosionX;
        private readonly double _explosionY;

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

        public Missile(IReadonlyRobot robot, Tick matchStart, int id, double locX, double locY, double heading, double range)
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

            _explosionX = _launchLocX + range*_cosDriveAngle;
            _explosionY = _launchLocY + range*_sinDriveAngle;

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

        public double ExplosionX
        {
            get { return _explosionX; }
        }

        public double ExplosionY
        {
            get { return _explosionY; }
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
