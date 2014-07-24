using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common;
using SDK;

namespace Arena.Internal.JRobots
{
    internal class Robot : ISDKRobot, ISDKCheat, IReadonlyRobot
    {
        private const double Tolerance = 0.00001;

        private RobotStates _state;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _mainTask;
        private CountdownEvent _syncCountdownEvent;

        private IArenaRobotInteraction _arena;
        private SDK.Robot _userRobot;

        private int _id;
        private int _team;
        private string _teamName;
        private Tick _matchStart;

        private int _cannonCount;
        private int _damage;
        private double _locX;
        private double _locY;
        private double _currentSpeed; // in m/s
        private double _desiredSpeed; // in m/s
        private int _heading; // in degrees
        private double _driveAngle; // in radians
        private double _cosDriveAngle;
        private double _sinDriveAngle;
        private Tick _lastMissileLaunchTick;

        public RobotStatistics Statistics { get; private set; }

        public RobotStates State
        {
            get { return _state; }
        }

        public int Id
        {
            get { return _id; }
        }

        public int Team
        {
            get { return _team; }
        }

        public string TeamName
        {
            get { return _teamName; }
        }

        public double LocX
        {
            get { return _locX; }
        }

        public double LocY
        {
            get { return _locY; }
        }

        public int Damage
        {
            get { return _damage; }
        }

        public Robot()
        {
            Statistics = new RobotStatistics();

            _state = RobotStates.Created;
        }

        public void Initialize(SDK.Robot userRobot, IArenaRobotInteraction arena, string teamName, int id, int team, int locX, int locY)
        {
            Initialize(userRobot, arena, teamName, id, team, locX, locY, 0, 0);
        }

        public void Initialize(SDK.Robot userRobot, IArenaRobotInteraction arena, string teamName, int id, int team, int locX, int locY, int heading, int speed)
        {
            _userRobot = userRobot;
            _userRobot.SDK = this;
            if (ParametersSingleton.EnableCheat)
                _userRobot.Cheat = this; // uncomment this to activate cheats
            _arena = arena;
            _teamName = teamName;
            _id = id;
            _team = team;
            _locX = locX;
            _locY = locY;
            _damage = 0;
            _currentSpeed = speed;

            _heading = heading;
            _desiredSpeed = speed;

            _state = RobotStates.Initialized;

            Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] initialized.", _teamName, _id);
        }

        public void Start(Tick matchStart, CountdownEvent syncCountdownEvent)
        {
            try
            {
                //_stopEvent.Reset();

                _syncCountdownEvent = syncCountdownEvent;
                _matchStart = matchStart;
                _state = RobotStates.Starting;

                _cancellationTokenSource = new CancellationTokenSource();
                _mainTask = Task.Factory.StartNew(MainLoop, _cancellationTokenSource.Token);

                Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] started.", _teamName, _id);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception while starting Robot {0}[{1}]. {2}", _teamName, _id, ex);
            }
        }

        public void Stop()
        {
            // Check if task has been started
            if (_mainTask == null)
                return;

            try
            {
                Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] stopping.", _teamName, _id);
                _state = RobotStates.Stopping;

                _cancellationTokenSource.Cancel();
                //_stopEvent.Set();

                _mainTask.Wait(1000);
            }
            catch (AggregateException ex)
            {
                foreach (Exception inner in ex.InnerExceptions)
                {
                    if (inner is TaskCanceledException)
                        Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] cancelled successfully.", _teamName, _id);
                    else if (inner is ThreadAbortException)
                        Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] aborted successfully.", _teamName, _id);
                    else
                        Log.WriteLine(Log.LogLevels.Info, "Exception while stopping Robot {0}[{1}]. {2}", _teamName, _id, ex);
                }
            }
            finally
            {
                _state = RobotStates.Stopped;
                _mainTask = null;

                Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] stopped.", _teamName, _id);
            }
        }

        public void TakeDamage(int damage)
        {
            Statistics.Increment("DAMAGE_TAKEN");
            _damage += damage;
            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] takes {2} damage => {3} total damage", TeamName, Id, damage, Damage);
            if (_damage >= ParametersSingleton.MaxDamage)
            {
                _state = RobotStates.Destroyed;
                Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] destroyed", _teamName, _id);
            }
        }

        public void Update(double dt)
        {
            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] before: x:{2:0.000000} y:{3:0.000000} s:{4:0.000000} dt:{5:0.0000}", _teamName, _id, _locX, _locY, _currentSpeed, dt);
            //
            bool hit = ComputeCurrentLocation(dt, out _locX, out _locY, out _currentSpeed);
            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] after: x:{2:0.000000} y:{3:0.000000} s:{4:0.000000}", _teamName, _id, _locX, _locY, _currentSpeed);
            //
            if (hit)
            {
                _desiredSpeed = 0;
                TakeDamage(ParametersSingleton.CollisionDamage);
            }
        }

        public bool ComputeCurrentLocation(double dt, out double newLocationX, out double newLocationY, out double newSpeed)
        {
            double delta = dt * ParametersSingleton.MaxAcceleration; // speed increase due to acceleration v = a.t
            double travelledDistanceDueToAcceleration = 0.5 * delta * dt; // distance increase due to acceleration d = 0.5.a.t.t

            double travelledDistance = dt * _currentSpeed; // relocates due to its speed d = v.t
            double speedDiff = _desiredSpeed - _currentSpeed; // difference between desired and current speed
            if (System.Math.Abs(speedDiff) < Tolerance) // desired speed reached
            {
                newLocationX = _locX + travelledDistance * _cosDriveAngle; // increase along x axis
                newLocationY = _locY + travelledDistance * _sinDriveAngle; // increase along y axis
                newSpeed = _currentSpeed;
            }
            else
            {
                if (speedDiff > 0) // acceleration
                {
                    double nextSpeed = _currentSpeed + delta; // next speed
                    if (nextSpeed > _desiredSpeed) // overstep
                    {
                        double t1 = speedDiff / ParametersSingleton.MaxAcceleration; // time when we would overstep
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = _currentSpeed * t1 + 0.5 * ParametersSingleton.MaxAcceleration * t1 * t1 + _desiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = _locX + compositeTravelledDistance * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + compositeTravelledDistance * _sinDriveAngle; // relocation along y axis
                        newSpeed = _desiredSpeed; // no speed overstepping
                    }
                    else // no overstep
                    {
                        newLocationX = _locX + (travelledDistance + travelledDistanceDueToAcceleration) * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + (travelledDistance + travelledDistanceDueToAcceleration) * _sinDriveAngle; // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
                else // deacceleration
                {
                    double nextSpeed = _currentSpeed - delta; // next speed
                    if (nextSpeed < _desiredSpeed) // overstep
                    {
                        double t1 = -speedDiff / ParametersSingleton.MaxAcceleration; // time when we would overstep (sign change is faster than abs)
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = _currentSpeed * t1 - 0.5 * ParametersSingleton.MaxAcceleration * t1 * t1 + _desiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = _locX + compositeTravelledDistance * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + compositeTravelledDistance * _sinDriveAngle; // relocation along y axis
                        newSpeed = _desiredSpeed; // no speed overstepping
                    }
                    else
                    {
                        newLocationX = _locX + (travelledDistance - travelledDistanceDueToAcceleration) * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + (travelledDistance - travelledDistanceDueToAcceleration) * _sinDriveAngle; // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
            }

            // Check collision with wall
            bool hit = false;
            if (newLocationX < 0)
            {
                hit = true;
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    newLocationY = newLocationY - newLocationX * _sinDriveAngle / _cosDriveAngle;
                newLocationX = 0;
                newSpeed = 0;
            }
            else if (newLocationX > ParametersSingleton.ArenaSize)
            {
                hit = true;
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    newLocationY = newLocationY + (ParametersSingleton.ArenaSize - newLocationX) * _sinDriveAngle / _cosDriveAngle;
                newLocationX = ParametersSingleton.ArenaSize;
                newSpeed = 0;
            }
            if (newLocationY < 0)
            {
                hit = true;
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    newLocationX = newLocationX - newLocationY * _cosDriveAngle / _sinDriveAngle;
                newLocationY = 0;
                newSpeed = 0;
            }
            else if (newLocationY > ParametersSingleton.ArenaSize)
            {
                hit = true;
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    newLocationX = newLocationX + (ParametersSingleton.ArenaSize - newLocationY) * _cosDriveAngle / _sinDriveAngle;
                newLocationY = ParametersSingleton.ArenaSize;
                newSpeed = 0;
            }

            return hit;
        }

        #region ISDKRobot

        int ISDKRobot.Damage
        {
            get { return _damage; }
        }

        int ISDKRobot.LocX
        {
            get { return IntLocX; }
        }

        int ISDKRobot.LocY
        {
            get { return IntLocY; }
        }

        int ISDKRobot.Speed
        {
            get { return SpeedPercentage; }
        }

        double ISDKRobot.Time
        {
            get { return Tick.ElapsedSeconds(_matchStart); }
        }

        int ISDKRobot.Id
        {
            get { return _id; }
        }

        int ISDKRobot.Cannon(int degrees, int range)
        {
            Statistics.Increment("CANNON");

            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]. Cannon {2} {3}.", TeamName, Id, degrees, range);
            if (_damage > 100) // too damaged
                return 0;

            degrees = FixDegrees(degrees);
            range = FixCannonRange(range);
            double elapsed = _lastMissileLaunchTick == null ? double.MaxValue : Tick.ElapsedSeconds(_lastMissileLaunchTick);
            if (elapsed < 1) // reload
            {
                Statistics.Increment("CANNON_RELOAD");
                return 0;
            }

            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] shooting interval {2}", TeamName, Id, elapsed);
            _cannonCount++;
            _lastMissileLaunchTick = Tick.Now;
            int result = _arena.Cannon(this, _lastMissileLaunchTick, degrees, range);
            return result;
        }

        void ISDKRobot.Drive(int degrees, int speed)
        {
            Statistics.Increment("DRIVE");
            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]. Drive {2} {3}.", TeamName, Id, degrees, speed);

            degrees = FixDegrees(degrees);
            speed = FixSpeed(speed);

            if (_currentSpeed <= ParametersSingleton.MaxTurnSpeed) // Can change heading only if current speed is lower than max turn speed
            {
                _heading = degrees; // change heading
                _driveAngle = Common.Math.ToRadians(degrees);
                _cosDriveAngle = System.Math.Cos(_driveAngle);
                _sinDriveAngle = System.Math.Sin(_driveAngle);
            }
            // Change desired speed
            _desiredSpeed = speed * ParametersSingleton.MaxSpeed / 100.0; // speed is expressed in percentage

            // Speed boost if stopped and wants to move
            if (System.Math.Abs(_currentSpeed) < Tolerance && _desiredSpeed > 0)
                _currentSpeed = ParametersSingleton.MaxSpeed / 100.0; // 1% speed (0.06 sec)

            _arena.Drive(this, degrees, speed);
        }

        int ISDKRobot.Scan(int degrees, int resolution)
        {
            Statistics.Increment("SCAN");

            // not needed anymore Thread.Sleep(1); // give time to others
            //Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]. Scan {2} {3}.", TeamName, Id, degrees, resolution);
            if (_damage > 100) // too damaged
                return 0;

            degrees = FixDegrees(degrees);
            resolution = FixResolution(resolution);
            int result = _arena.Scan(this, degrees, resolution);
            return result;
        }

        int ISDKRobot.FriendsCount
        {
            get { return _arena.TeamCount(this); }
        }

        IReadOnlyDictionary<string, int> ISDKRobot.Parameters
        {
            get { return ParametersSingleton.Instance.Parameters; }
        }

        void ISDKRobot.LogLine(string format, params object[] args)
        {
            Log.WriteLine(Log.LogLevels.Debug, String.Format("Robot {0}[{1}] : {2}", _teamName, _id, String.Format(format, args)));
        }

        #region Math

        int ISDKRobot.Rand(int limit)
        {
            return _arena.Rand(limit);
        }

        int ISDKRobot.Sqrt(int value)
        {
            return (int)System.Math.Round(System.Math.Sqrt(value));
        }

        int ISDKRobot.Sin(int degrees)
        {
            return (int)System.Math.Round(System.Math.Sin(Common.Math.ToRadians(degrees)) * ParametersSingleton.TrigonometricBias);
        }

        int ISDKRobot.Cos(int degrees)
        {
            return (int)System.Math.Round(System.Math.Cos(Common.Math.ToRadians(degrees)) * ParametersSingleton.TrigonometricBias);
        }

        int ISDKRobot.Tan(int degrees)
        {
            return (int)System.Math.Round(System.Math.Tan(Common.Math.ToRadians(degrees)) * ParametersSingleton.TrigonometricBias);
        }

        int ISDKRobot.ATan(int value)
        {
            return (int)System.Math.Round(Common.Math.FixDegrees(Common.Math.ToDegrees(System.Math.Atan(value / ParametersSingleton.TrigonometricBias))));
        }

        double ISDKRobot.Sqrt(double value)
        {
            return System.Math.Sqrt(value);
        }

        double ISDKRobot.Sin(double radians)
        {
            return System.Math.Sin(radians);
        }

        double ISDKRobot.Cos(double radians)
        {
            return System.Math.Cos(radians);
        }

        double ISDKRobot.Tan(double radians)
        {
            return System.Math.Tan(radians);
        }

        double ISDKRobot.ATan(double value)
        {
            return System.Math.Atan(value);
        }

        double ISDKRobot.Deg2Rad(double degrees)
        {
            return Common.Math.ToRadians(degrees);
        }

        double ISDKRobot.Rad2Deg(double radians)
        {
            return Common.Math.ToDegrees(radians);
        }

        double ISDKRobot.Abs(double value)
        {
            return System.Math.Abs(value);
        }

        double ISDKRobot.Round(double value)
        {
            return System.Math.Round(value);
        }

        double ISDKRobot.Exp(double power)
        {
            return System.Math.Exp(power);
        }

        double ISDKRobot.Log(double value)
        {
            return System.Math.Log(value);
        }

        double ISDKRobot.ATan2(double y, double x)
        {
            return System.Math.Atan2(y, x);
        }

        #endregion

        #endregion

        #region IReadonlyRobot

        int IReadonlyRobot.Team
        {
            get { return _team; }
        }

        int IReadonlyRobot.Id
        {
            get { return _id; }
        }

        string IReadonlyRobot.TeamName
        {
            get { return _teamName; }
        }

        RobotStates IReadonlyRobot.State
        {
            get { return _state; }
        }

        int IReadonlyRobot.LocX
        {
            get { return IntLocX; }
        }

        int IReadonlyRobot.LocY
        {
            get { return IntLocY; }
        }

        int IReadonlyRobot.Heading
        {
            get { return _heading; }
        }

        int IReadonlyRobot.Speed
        {
            get { return SpeedPercentage; }
        }

        int IReadonlyRobot.Damage
        {
            get { return _damage; }
        }

        int IReadonlyRobot.CannonCount
        {
            get { return _cannonCount; }
        }

        IReadOnlyDictionary<string, int> IReadonlyRobot.Statistics
        {
            get { return Statistics.Values; }
        }

        #endregion

        #region ISDKCheat

        public void FindNearestEnemy(out double degrees, out double range, out double x, out double y)
        {
            _arena.FindNearestEnemy(this, out degrees, out range, out x, out y);
        }

        public void FireAt(double targetX, double targetY)
        {
            _arena.FireAt(this, targetX, targetY);
        }

        public void Teleport(double locX, double locY)
        {
            _locX = locX;
            _locY = locY;
        }

        #endregion

        private int SpeedPercentage
        {
            get { return (int)System.Math.Round(_currentSpeed * 100.0 / ParametersSingleton.MaxSpeed); }
        }

        private int IntLocX
        {
            get { return (int)System.Math.Round(_locX); }
        }

        private int IntLocY
        {
            get { return (int)System.Math.Round(_locY); }
        }

        private void MainLoop()
        {
            try
            {
                // We cannot be sure, user's main loop is stopped when Damage == 100 or when asked to stopped
                // So, we have to abort the thread even if it's not recommended
                //using (_cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                {
                    Stopwatch sw = new Stopwatch();

                    Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]  signaling/waiting CountdownEvent", _teamName, _id);
                    // Decrement CountdownEvent and wait on it until every robot has started
                    if (!_syncCountdownEvent.Signal())
                    {
                        Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]  waiting other robots", _teamName, _id);
                        _syncCountdownEvent.Wait();
                    }
                    _state = RobotStates.Running;
                    Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}]  every robot has been signaled", _teamName, _id);

                    _userRobot.Init();

                    //_userRobot.Main();
                    while (true)
                    {
                        //
                        sw.Reset();
                        sw.Start();

                        // Step is called only if robot is alive
                        if (_damage < ParametersSingleton.MaxDamage)
                            _userRobot.Step();
                        //

                        sw.Stop();
                        double elapsed = sw.ElapsedMilliseconds;
                        int sleepTime = (int)(ParametersSingleton.StepDelay - elapsed);
                        if (sleepTime < 0)
                            sleepTime = 1;
                        //Log.WriteLine(Log.LogLevels.Debug, "Elapsed {0:0.0000} -> Sleep {1}", elapsed, sleepTime);
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                        //bool stopAsked = _stopEvent.WaitOne(sleepTime);
                        //if (stopAsked)
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            Log.WriteLine(Log.LogLevels.Info, "Task cancelled. Stopping robot main loop");
                            break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Log.WriteLine(Log.LogLevels.Info, "ThreadAbortException with Robot {0}[{1}].", _teamName, _id);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception with Robot {0}[{1}]. {2}", _teamName, _id, ex);
            }
            finally
            {
                _state = RobotStates.Stopped;

                Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] task stopped.", _teamName, _id);
            }
        }

        private static int FixDegrees(int degrees)
        {
            degrees %= 360;
            if (degrees < 0)
                degrees += 360;
            return degrees;
        }

        private static int FixCannonRange(int range)
        {
            if (range < 0)
                range = 0;
            if (range > ParametersSingleton.MaxCannonRange)
                range = ParametersSingleton.MaxCannonRange;
            return range;
        }

        private static int FixResolution(int resolution)
        {
            if (resolution < 0)
                resolution = 0;
            if (resolution > ParametersSingleton.MaxResolution)
                resolution = ParametersSingleton.MaxResolution;
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
