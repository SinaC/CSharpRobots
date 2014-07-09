using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common;
using SDK;
using Math = System.Math;

namespace Arena.Internal
{
    internal class Robot : ISDKRobot, IReadonlyRobot
    {
        private CancellationTokenSource _cancellationTokenSource;
        //private readonly ManualResetEvent _stopEvent;
        private Task _mainTask;
        private CountdownEvent _syncCountdownEvent;

        private readonly Random _random;

        private Arena _arena;
        private SDK.Robot _userRobot;
        private Tick _matchStart;

        private int _damage;

        // These values are modified by Drive and used to compute acceleration/range/speed/location
        private int _desiredHeading;
        private int _desiredSpeed;
        //
        private double _acceleration; // Linear acceleration in percentage of maximum speed (represent current speed while accelerating or slowing)
        // Following values are needed to avoid precision problem while computing new location
        private double _originX; // X-component before changing heading
        private double _originY; // Y-component before changing heading
        private double _currentDistance; // Distance traveled in this heading
        public Tick LastMissileLaunchTick { get; private set; }

        public double RawLocX { get; private set; }
        public double RawLocY { get; private set; }

        public RobotStatistics Statistics { get; private set; }

        public Robot()
        {
            _random = new Random();

            //_stopEvent = new ManualResetEvent(false);

            Statistics = new RobotStatistics();

            State = RobotStates.Created;
        }

        public void Initialize(SDK.Robot userRobot, Arena arena, string teamName, int id, int team, int locX, int locY)
        {
            Initialize(userRobot, arena, teamName, id, team, locX, locY, 0, 0);
        }

        public void Initialize(SDK.Robot userRobot, Arena arena, string teamName, int id, int team, int locX, int locY, int heading, int speed)
        {
            _userRobot = userRobot;
            _userRobot.SDK = this;
            _arena = arena;
            TeamName = teamName;
            Id = id;
            Team = team;
            RawLocX = locX;
            RawLocY = locY;
            _damage = 0;
            Speed = speed;

            Heading = heading;
            _desiredHeading = heading;
            _desiredSpeed = speed;
            _originX = locX;
            _originY = LocY;
            _currentDistance = 0;

            State = RobotStates.Initialized;

            Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} initialized.", Id, Team);
        }

        public void Start(Tick matchStart, CountdownEvent syncCountdownEvent)
        {
            try
            {
                //_stopEvent.Reset();

                _syncCountdownEvent = syncCountdownEvent;
                _matchStart = matchStart;
                State = RobotStates.Starting;

                _cancellationTokenSource = new CancellationTokenSource();
                _mainTask = Task.Factory.StartNew(MainLoop, _cancellationTokenSource.Token);

                Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} started.", Id, Team);
            }
            catch (Exception ex)
            {
                Common.Log.WriteLine(Common.Log.LogLevels.Error, "Exception while starting Robot {0} | {1}. {2}", Id, Team, ex);
            }
        }

        public void Stop()
        {
            // Check if task has been started
            if (_mainTask == null)
                return;

            try
            {
                Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} stopping.", Id, Team);
                State = RobotStates.Stopping;

                _cancellationTokenSource.Cancel();
                //_stopEvent.Set();

                _mainTask.Wait(1000);
            }
            catch (AggregateException ex)
            {
                foreach (Exception inner in ex.InnerExceptions)
                {
                    if (inner is TaskCanceledException)
                        Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} cancelled successfully.", Id, Team);
                    else if (inner is ThreadAbortException)
                        Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} aborted successfully.", Id, Team);
                    else
                        Common.Log.WriteLine(Common.Log.LogLevels.Info, "Exception while stopping Robot {0} | {1}. {2}", Id, Team, ex);
                }
            }
            finally
            {
                State = RobotStates.Stopped;
                _mainTask = null;

                Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} stopped.", Id, Team);
            }
        }

        public void UpdateSpeed(double realStepTime)
        {
            // Update speed, moderated by acceleration
            if (Speed != _desiredSpeed)
            {
                if (Speed > _desiredSpeed) // Slowing
                {
                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} slowing. Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);

                    _acceleration -= ((ParametersSingleton.MaxAcceleration*realStepTime)/1000.0)*(100.0/ParametersSingleton.MaxSpeed);
                    if (_acceleration < _desiredSpeed)
                    {
                        Speed = _desiredSpeed;
                        _acceleration = _desiredSpeed;
                    }
                    else
                        Speed = (int)_acceleration;

                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} slowed. Updated Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);
                }
                else // Accelerating
                {
                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} accelerating. Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);

                    _acceleration += ((ParametersSingleton.MaxAcceleration*realStepTime)/1000.0)*(100.0/ParametersSingleton.MaxSpeed);
                    if (_acceleration > _desiredSpeed)
                    {
                        Speed = _desiredSpeed;
                        _acceleration = _desiredSpeed;
                    }
                    else
                        Speed = (int)_acceleration;

                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} accelerated. Updated Speed {2} Desired Speed {3} Acceleration {4}", Id, Team, Speed, _desiredSpeed, _acceleration);
                }
            }
        }

        public void UpdateHeading()
        {
            // Update heading, allow change below a certain speed
            if (Heading != _desiredHeading)
            {
                //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} updating heading. Heading {2} Desired Heading {3} RawLocX {4} RawLocY {5} Speed {6}", Id, Team, Heading, _desiredHeading, RawLocX, RawLocY, Speed);

                if (Speed <= ParametersSingleton.MaxTurnSpeed)
                {
                    Heading = _desiredHeading;
                    _currentDistance = 0;
                    _originX = RawLocX;
                    _originY = RawLocY;

                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} heading updated. Heading {2} Desired Heading {3} OriginX {4} OriginY {5} Speed {6}", Id, Team, Heading, _desiredHeading, _originX, _originY, Speed);
                }
                else
                {
                    //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} moving too fast, cannot update Heading", Id, Team);
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
                _currentDistance += ((ParametersSingleton.MaxSpeed * Speed / 100.0) * realStepTime) / 1000.0;
                double newX, newY;
                Common.Math.ComputePoint(_originX, _originY, _currentDistance, Heading, out newX, out newY);
                RawLocX = newX;
                RawLocY = newY;

                //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} location updated. CurrentDistance {2} OriginX {3} OriginY {4} RawLocX {5} RawLocY {6} Heading {7} Speed {8}", Id, Team, _currentDistance, _originX, _originY, RawLocX, RawLocY, Heading, Speed);
            }
        }

        public void TakeDamage(int damage)
        {
            Statistics.Increment("DAMAGE_TAKEN");
            Damage += damage;
            Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1} takes {2} damage => {3} total damage", Id, Team, damage, Damage);
            if (Damage >= ParametersSingleton.MaxDamage)
            {
                State = RobotStates.Destroyed;
                Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} destroyed", Id, Team);
            }
        }

        public void Collision(int damage)
        {
            Statistics.Increment("COLLISION");
            TakeDamage(damage);
            Speed = 0;
            _desiredSpeed = 0;
        }

        public void CollisionWall(int damage, double newLocX, double newLocY)
        {
            Statistics.Increment("COLLISION_WALL");
            Collision(damage);
            RawLocX = newLocX;
            RawLocY = newLocY;
        }

        #region IReadonlyRobot + ISDKRobot

        public int Id { get; private set; }

        public int LocX
        {
            get
            {
                return (int)RawLocX;
            }
        }

        public int LocY
        {
            get
            {
                return (int)RawLocY;
            }
        }

        public int Damage
        {
            get
            {
                return _damage;
            }
            private set
            {
                _damage = value;
            }
        }

        public int Speed { get; private set; }

        #endregion

        #region IReadonlyRobot

        public RobotStates State { get; private set; }

        public int Team { get; private set; }

        public string TeamName { get; private set; }

        public int Heading { get; private set; }

        public int CannonCount { get; private set; }

        IReadOnlyDictionary<string, int> IReadonlyRobot.Statistics
        {
            get { return Statistics.Values; }
        }

        #endregion

        #region ISDKRobot

        public double Time
        {
            get
            {
                return Tick.ElapsedSeconds(_matchStart);
            }
        }

        public int Cannon(int degrees, int range)
        {
            Statistics.Increment("CANNON");

            // not needed anymore Thread.Sleep(1); // give time to others
            //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}. Cannon {2} {3}.", Id, Team, degrees, range);
            if (_damage > 100) // too damaged
                return 0;

            degrees = FixDegrees(degrees);
            range = FixCannonRange(range);
            double elapsed = LastMissileLaunchTick == null ? double.MaxValue : Tick.ElapsedSeconds(LastMissileLaunchTick);
            if (elapsed < 1) // reload
            {
                Statistics.Increment("CANNON_RELOAD");
                return 0;
            }

            //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0}[{1}] shooting interval {2}", TeamName, Id, elapsed);
            CannonCount++;
            LastMissileLaunchTick = Tick.Now;
            int result = _arena.Cannon(this, LastMissileLaunchTick, degrees, range);
            return result;
        }

        public void Drive(int degrees, int speed)
        {
            Statistics.Increment("DRIVE");
            // not needed anymore Thread.Sleep(1); // give time to others
            //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}. Drive {2} {3}.", Id, Team, degrees, speed);

            degrees = FixDegrees(degrees);
            speed = FixSpeed(speed);

            _desiredHeading = degrees;
            _desiredSpeed = speed;
            _arena.Drive(this, degrees, speed);
        }

        public int Scan(int degrees, int resolution)
        {
            Statistics.Increment("SCAN");

            // not needed anymore Thread.Sleep(1); // give time to others
            //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}. Scan {2} {3}.", Id, Team, degrees, resolution);
            if (_damage > 100) // too damaged
                return 0;

            degrees = FixDegrees(degrees);
            resolution = FixResolution(resolution);
            int result = _arena.Scan(this, degrees, resolution);
            return result;
        }

        public int FriendsCount
        {
            get { return _arena.TeamCount(this); }
        }

        public IReadOnlyDictionary<string, int> Parameters
        {
            get { return ParametersSingleton.Instance.Parameters; }
        }

        #region Math

        public int Rand(int limit)
        {
            return _random.Next(limit);
        }

        public int Sqrt(int value)
        {
            return (int) Math.Sqrt(value);
        }

        public int Sin(int degrees)
        {
            return (int) (Math.Sin(Common.Math.ToRadians(degrees))*ParametersSingleton.TrigonometricBias);
        }

        public int Cos(int degrees)
        {
            return (int) (Math.Cos(Common.Math.ToRadians(degrees))*ParametersSingleton.TrigonometricBias);
        }

        public int Tan(int degrees)
        {
            return (int) (Math.Tan(Common.Math.ToRadians(degrees))*ParametersSingleton.TrigonometricBias);
        }

        public int ATan(int value)
        {
            return (int) Common.Math.ToDegrees(Math.Atan(value/ParametersSingleton.TrigonometricBias));
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
            return Common.Math.ToRadians(degrees);
        }

        public double Rad2Deg(double radians)
        {
            return Common.Math.ToDegrees(radians);
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

        public double ATan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        #endregion

        #endregion

        private void MainLoop()
        {
            try
            {
                // We cannot be sure, user's main loop is stopped when Damage == 100 or when asked to stopped
                // So, we have to abort the thread even if it's not recommended
                //using (_cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                {
                    Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}  signaling/waiting CountdownEvent", Id, Team);
                    // Decrement CountdownEvent and wait on it until every robot has started
                    if (!_syncCountdownEvent.Signal())
                    {
                        Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}  waiting other robots", Id, Team);
                        _syncCountdownEvent.Wait();
                    }
                    Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Robot {0} | {1}  every robot has been signaled", Id, Team);

                    Stopwatch sw = new Stopwatch();

                    _userRobot.Init();

                    State = RobotStates.Running;
                    //_userRobot.Main();
                    while(true)
                    {
                        //
                        sw.Reset();
                        sw.Start();
                        
                        // Step is called only if robot is alive
                        if (Damage < ParametersSingleton.MaxDamage)
                            _userRobot.Step();
                        //

                        sw.Stop();
                        double elapsed = sw.ElapsedMilliseconds;
                        int sleepTime = (int)(ParametersSingleton.StepDelay - elapsed);
                        if (sleepTime < 0)
                            sleepTime = 1;
                        //Common.Log.WriteLine(Common.Log.LogLevels.Debug, "Elapsed {0:0.0000} -> Sleep {1}", elapsed, sleepTime);
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                        //bool stopAsked = _stopEvent.WaitOne(sleepTime);
                        //if (stopAsked)
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            Common.Log.WriteLine(Common.Log.LogLevels.Info, "Task cancelled. Stopping robot main loop");
                            break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Common.Log.WriteLine(Common.Log.LogLevels.Info, "ThreadAbortException with Robot {0} | {1}.", Id, Team);
            }
            catch (Exception ex)
            {
                Common.Log.WriteLine(Common.Log.LogLevels.Error, "Exception with Robot {0} | {1}. {2}", Id, Team, ex);
            }
            finally
            {
                State = RobotStates.Stopped;

                Common.Log.WriteLine(Common.Log.LogLevels.Info, "Robot {0} | {1} task stopped.", Id, Team);
            }
        }
        
        private static int FixDegrees(int degrees)
        {
            if (degrees < 0)
                degrees = 360+degrees;
            if (degrees >= 360)
                degrees %= 360;
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
