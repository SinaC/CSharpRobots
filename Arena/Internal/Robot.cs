using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Clock;
using SDK;

// TODO: known bug: acceleration is handled poorly: solution use double for speed instead of int and modify UpdateSpeed

namespace Arena.Internal
{
    internal class Robot : ISDKRobot, IReadonlyRobot
    {
        public static readonly double TrigonometricBias = 100000;
        public static readonly double MaxSpeed = 30; // in m/s
        public static readonly int MaxDamage = 100;
        public static readonly int MaxResolution = 20; // in degrees
        public static readonly int MaxCannonRange = 700; // in meters
        public static readonly int MaxAcceleration = 5; // acceleration factor
        public static readonly int MaxTurnSpeed = 50; // maximum speed for direction change

        private CancellationTokenSource _cancellationTokenSource;
        private Task _mainTask;

        private Arena _arena;
        private SDK.Robot _userRobot;
        private Tick _matchStart;
        
        // These values are modified by Drive and used to compute acceleration/range/speed/location
        private int _desiredHeading;
        private int _desiredSpeed;
        //
        private double _acceleration; // Linear acceleration in m/s
        // Following values are needed to avoid precision problem while computing new location
        private double _originX; // X-component before changing heading
        private double _originY; // Y-component before changing heading
        private double _currentDistance; // Distance traveled in this heading

        public Tick LastMissileLaunchTick { get; private set; }

        public double RawLocX { get; private set; }
        public double RawLocY { get; private set; }

        public Robot()
        {
            State = RobotStates.Created;
        }

        public void Initialize(SDK.Robot userRobot, Arena arena, int id, int team, int locX, int locY)
        {
            _userRobot = userRobot;
            _userRobot.SDK = this;
            _arena = arena;
            Id = id;
            Team = team;
            RawLocX = locX;
            RawLocY = locY;
            Damage = 0;
            Speed = 0;

            Heading = 0;
            _desiredHeading = 0;
            _desiredSpeed = 0;
            _originX = locX;
            _originY = LocY;
            _currentDistance = 0;

            State = RobotStates.Initialized;

            System.Diagnostics.Debug.WriteLine("Robot {0} | {1} initialized.", Id, Team);
        }

        public void Initialize(SDK.Robot userRobot, Arena arena, int id, int team, int locX, int locY, int heading, int speed)
        {
            _userRobot = userRobot;
            _userRobot.SDK = this;
            _arena = arena;
            Id = id;
            Team = team;
            RawLocX = locX;
            RawLocY = locY;
            Damage = 0;
            Speed = speed;

            Heading = heading;
            _desiredHeading = heading;
            _desiredSpeed = speed;
            _originX = locX;
            _originY = LocY;
            _currentDistance = 0;

            State = RobotStates.Initialized;

            System.Diagnostics.Debug.WriteLine("Robot {0} | {1} initialized.", Id, Team);
        }

        public void Start(Tick matchStart)
        {
            try
            {
                _matchStart = matchStart;
                State = RobotStates.Starting;

                _cancellationTokenSource = new CancellationTokenSource();
                _mainTask = Task.Factory.StartNew(MainLoop, _cancellationTokenSource.Token);

                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} started.", Id, Team);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while starting Robot {0} | {1}. {2}", Id, Team, ex);
            }
        }

        public void Stop()
        {
            // Check if task has been started
            if (_mainTask == null)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} stopping.", Id, Team);
                State = RobotStates.Stopping;

                _cancellationTokenSource.Cancel();

                Task.WaitAll(_mainTask);
            }
            catch (AggregateException ex)
            {
                foreach (Exception inner in ex.InnerExceptions)
                {
                    if (inner is TaskCanceledException)
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} cancelled successfully.", Id, Team);
                    else if (inner is ThreadAbortException)
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} aborted successfully.", Id, Team);
                    else
                        System.Diagnostics.Debug.WriteLine("Exception while stopping Robot {0} | {1}. {2}", Id, Team, ex);
                }
            }
            finally
            {
                State = RobotStates.Stopped;
                _mainTask = null;

                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} stopped.", Id, Team);
            }
        }

        public void UpdateSpeed(double realStepTime)
        {
            // Update speed, moderated by acceleration
            if (Speed != _desiredSpeed)
            {
                if (Speed > _desiredSpeed) // Slowing
                {
                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} slowing. Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);

                    _acceleration -= MaxAcceleration;
                    if (_acceleration < _desiredSpeed)
                    {
                        Speed = _desiredSpeed;
                        _acceleration = _desiredSpeed;
                    }
                    else
                        Speed = (int)_acceleration;

                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} slowed. Updated Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);
                }
                else // Accelerating
                {
                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} accelerating. Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);

                    _acceleration += MaxAcceleration;
                    if (_acceleration > _desiredSpeed)
                    {
                        Speed = _desiredSpeed;
                        _acceleration = _desiredSpeed;
                    }
                    else
                        Speed = (int)_acceleration;

                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} accelerated. Updated Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);
                }
            }
        }

        public void UpdateHeading()
        {
            // Update heading, allow change below a certain speed
            if (Heading != _desiredHeading)
            {
                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} updating heading. Heading {2} Desired Heading {3} RawLocX {4} RawLocY {5} Speed {6}", Id, Team, Heading, _desiredHeading, RawLocX, RawLocY, Speed);

                if (Speed <= MaxTurnSpeed)
                {
                    Heading = _desiredHeading;
                    _currentDistance = 0;
                    _originX = RawLocX;
                    _originY = RawLocY;

                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} heading updated. Heading {2} Desired Heading {3} OriginX {4} OriginY {5} Speed {6}", Id, Team, Heading, _desiredHeading, _originX, _originY, Speed);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Robot {0} | {1} moving too fast, cannot update Heading", Id, Team);
                    _desiredSpeed = 0;
                }
            }
        }

        public void UpdateLocation(double realStepTime)
        {
            // Update distance traveled on this heading, x, y
            if (Speed > 0)
            {
                //CurrentDistance += ((MaxSpeed*Speed/100.0)*Arena.StepDelay)/1000.0; // new speed is considered only once by simulation step   GRRRRRR 2 hours lost to find we have to compute real elapsed time between 2 steps because System.Timers.Timer is not precise enough
                // Speed is in percentage of MaxSpeed, realStepTime is in milliseconds and MaxSpeed is in m/s
                _currentDistance += ((MaxSpeed * Speed / 100.0) * realStepTime) / 1000.0;
                double newX, newY;
                Common.Helpers.Math.ComputePoint(_originX, _originY, _currentDistance, Heading, out newX, out newY);
                RawLocX = newX;
                RawLocY = newY;

                //System.Diagnostics.Debug.WriteLine("Robot {0} | {1} location updated. CurrentDistance {2} OriginX {3} OriginY {4} RawLocX {5} RawLocY {6} Heading {7} Speed {8}", Id, Team, _currentDistance, _originX, _originY, RawLocX, RawLocY, Heading, Speed);
            }
        }

        public void TakeDamage(int damage)
        {
            Damage += damage;
            System.Diagnostics.Debug.WriteLine("Robot {0} | {1} takes {2} damage => {3} total damage", Id, Team, damage, Damage);
            if (Damage >= MaxDamage)
            {
                State = RobotStates.Destroyed;
                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} destroyed", Id, Team);
            }
        }

        public void Collision(int damage)
        {
            TakeDamage(damage);
            Speed = 0;
            _desiredSpeed = 0;
        }

        public void CollisionWall(int damage, double newLocX, double newLocY)
        {
            Collision(damage);
            RawLocX = newLocX;
            RawLocY = newLocY;
        }

        #region IReadonlyRobot + ISDKRobot

        public int Id { get; private set; }
        
        public int LocX
        {
            get { return (int)RawLocX; }
        }

        public int LocY
        {
            get { return (int)RawLocY; }
        }

        public int Damage { get; private set; }

        #endregion

        #region IReadonlyRobot

        public RobotStates State { get; private set; }

        public int Team { get; private set; }

        public int Heading { get; private set; }

        public int Speed { get; private set; }

        #endregion

        #region ISDKRobot

        public double Time
        {
            get { return Tick.ElapsedMilliseconds(_matchStart) / 1000.0; }
        }

        public int Cannon(int degrees, int range)
        {
            //System.Diagnostics.Debug.WriteLine("Robot {0} | {1}. Cannon {2} {3}.", Id, Team, degrees, range);

            degrees = FixDegrees(degrees);
            range = FixCannonRange(range);
            if (LastMissileLaunchTick != null && Tick.ElapsedSeconds(LastMissileLaunchTick) < 1)
                return 0; // reload
            LastMissileLaunchTick = Tick.Now;
            int result = _arena.Cannon(this, LastMissileLaunchTick, degrees, range);
            Thread.Sleep(1);
            return result;
        }

        public void Drive(int degrees, int speed)
        {
            //System.Diagnostics.Debug.WriteLine("Robot {0} | {1}. Drive {2} {3}.", Id, Team, degrees, speed);

            degrees = FixDegrees(degrees);
            speed = FixSpeed(speed);
            _desiredHeading = degrees;
            _desiredSpeed = speed;
            _arena.Drive(this, degrees, speed);
            Thread.Sleep(1);
        }

        public int Scan(int degrees, int resolution)
        {
            //System.Diagnostics.Debug.WriteLine("Robot {0} | {1}. Scan {2} {3}.", Id, Team, degrees, resolution);

            degrees = FixDegrees(degrees);
            resolution = FixResolution(resolution);
            int result = _arena.Scan(this, degrees, resolution);
            Thread.Sleep(1);
            return result;
        }

        public int FriendsCount
        {
            get { return _arena.TeamCount(this); }
        }

        #region Math

        public int Rand(int limit)
        {
            return _arena.Random.Next(limit);
        }

        public int Sqrt(int value)
        {
            return (int) Math.Sqrt(value);
        }

        public int Sin(int degrees)
        {
            return (int) (Math.Sin(Common.Helpers.Math.ToRadians(degrees))*TrigonometricBias);
        }

        public int Cos(int degrees)
        {
            return (int) (Math.Cos(Common.Helpers.Math.ToRadians(degrees))*TrigonometricBias);
        }

        public int Tan(int degrees)
        {
            return (int) (Math.Tan(Common.Helpers.Math.ToRadians(degrees))*TrigonometricBias);
        }

        public int ATan(int value)
        {
            return (int) Common.Helpers.Math.ToDegrees(Math.Atan(value/TrigonometricBias));
        }

        public double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        public double Sin(double radians)
        {
            return Math.Sin(radians);
        }

        public double Cos(double radians)
        {
            return Math.Cos(radians);
        }

        public double Tan(double radians)
        {
            return Math.Tan(radians);
        }

        public double ATan(double value)
        {
            return Math.Atan(value);
        }

        public double Deg2Rad(double degrees)
        {
            return Common.Helpers.Math.ToRadians(degrees);
        }

        public double Rad2Deg(double radians)
        {
            return Common.Helpers.Math.ToDegrees(radians);
        }

        public double Abs(double value)
        {
            return Math.Abs(value);
        }

        public double Round(double value)
        {
            return Math.Round(value);
        }

        public double Exp(double power)
        {
            return Math.Exp(power);
        }

        public double Log(double value)
        {
            return Math.Log(value);
        }

        #endregion

        #endregion

        private void MainLoop()
        {
            try
            {
                // We cannot be sure, user's main loop is stopped when Damage == 100 or when asked to stopped
                // So, we have to abort the thread even if it's not recommended
                using (_cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                {
                    State = RobotStates.Running;
                    _userRobot.Main();
                }
            }
            catch (ThreadAbortException)
            {
                System.Diagnostics.Debug.WriteLine("ThreadAbortException with Robot {0} | {1}.", Id, Team);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception with Robot {0} | {1}. {2}", Id, Team, ex);
            }
            finally
            {
                State = RobotStates.Stopped;

                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} stopped.", Id, Team);
            }
        }
        
        private static int FixDegrees(int degrees)
        {
            if (degrees < 0)
                degrees = -degrees;
            if (degrees >= 360)
                degrees %= 360;
            return degrees;
        }

        private static int FixCannonRange(int range)
        {
            if (range < 0)
                range = 0;
            if (range > MaxCannonRange)
                range = MaxCannonRange;
            return range;
        }

        private static int FixResolution(int resolution)
        {
            if (resolution < 0)
                resolution = 0;
            if (resolution > MaxResolution)
                resolution = MaxResolution;
            return resolution;
        }

        private static int FixSpeed(int speed)
        {
            if (speed < 0)
                speed = 0;
            if (speed > 100)
                speed = 100;
            return speed;
        }
    }
}
