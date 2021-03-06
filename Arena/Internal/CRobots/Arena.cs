﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Arena.Internal.CRobots
{
    /*
        Arena: 
            The battlefield is a square with a side of 1 Km (1000 meters). When a robot hits the walls of this square, it earns 2 damage points out of a total amount of 100 and the engine stops. It's not the worst thing, but it's not a good one, so avoid the walls of the battlefield. When a robot reaches 100 damage points, it's disabled and loses the game.µ
        Matches:
            There are three types of play:
            Single Match: Two robots fight one against the other.
            Double Match: Two couples of robots fight one against the other. This type of play is more difficult than the previous because it's not simple to distinguish the friend from the enemies.
            Team Match: Four team of eight robots each fight one against all the others.
            All matches last for 180 seconds. The robot or the team that disables all the other robots in the battlefield wins.
        Engine
            Robots have an engine and they can run everywhere in the battlefield. The maximum speed of the robots is 30 m/s (meters per second), i.e. 100 Km/h, and the acceleration is 5 m/s2. This means that a robot needs six seconds to reach the maximum speed.
            When the engine has 0% power, the robot stops with a deceleration of 5 m/s2, while a 100% power gives the maximum speed.
            When a robot hits the walls, the engine reaches 0% power and speed suddenly falls to 0 m/s.
        Cannon
            Robots have a cannon. This cannon fires missiles. The robot can point the cannon all around and can fire all the missiles it wants, but there is a reload time of 1 second.
        Missiles
            Missiles have a range of 700 meters and a speed of 300 m/s, so they need 2 seconds and a third to reach the maximum range. The speed of the missile is independent from the speed of the robot, so it's always 300 m/s. When a missile explodes, it gives damage points to all the robots nearby (remember that 100 damage points disable a robot). Damage points depend on the distance of the robot from the explosion. This is the correspondence
            5 meters 10 damage points
            20 meters 5 damage points
            40 meters 3 damage points
            If a robot fires a missile within a circle of 5 meters radius, it gives itself 10 damage points, so it's better to fire the missiles far away.
        Scanner
            Robots use a scanner to find other robots. It scans the battlefield with a resolution from 1 to 20 degrees. Scanning the battlefield, the robot receives the distance of the nearest robot (both friend or enemy) or zero if there is no one in that sector.
     */

    internal class Arena : IReadonlyArena, IArenaRobotInteraction
    {
        private struct DamageByRange
        {
            public int Range;
            public int Damage;
        }

        private readonly DamageByRange[] _damageByRanges = new[]
            {
                // Must be sorted on Range
                new DamageByRange // Direct
                    {
                        Range = 5,
                        Damage = 10
                    },
                new DamageByRange // Near
                    {
                        Range = 20,
                        Damage = 5,
                    },
                new DamageByRange // Far
                    {
                        Range = 40,
                        Damage = 2
                    }
            };

        private CancellationTokenSource _cancellationTokenSource;
        private CountdownEvent _robotSyncCountdownEvent; // used to synchronize arena and robots
        private Task _mainTask;

        private readonly List<Robot> _robots;
        private readonly List<Missile> _missiles;
        private int _missileId;

        private int _robotCount;

        private Tick _lastStepTick;
        private int _stepCount;

        private readonly RandomUnique _randomUnique;
        private readonly Random _random;

        internal Arena()
        {
            // Initialize Random
            _randomUnique = new RandomUnique(0, ParametersSingleton.ArenaSize);
            _random = new Random();

            //
            _robots = new List<Robot>();
            _missiles = new List<Missile>();
            
            // Force thread pool to use 32+1 work threads (32 robots max + 1 for arena)
            int workerThreads, complete;
            ThreadPool.GetMinThreads(out workerThreads, out complete);
            ThreadPool.SetMinThreads(32+1, complete);

            //
            State = ArenaStates.Created;
        }

        #region IReadonlyArena

        public ArenaStates State { get; private set; }

        public ArenaModes Mode { get; private set; }

        public Tick MatchStart { get; private set; }

        int IReadonlyArena.ArenaSize
        {
            get { return ParametersSingleton.ArenaSize; }
        }

        public int RobotByTeam
        {
            get { return _robotCount; }
        }

        public event ArenaStartedHandler ArenaStarted;
        public event ArenaStoppedHandler ArenaStopped;
        public event ArenaStepHandler ArenaStep;

        public int WinningTeam { get; private set; }

        public string WinningTeamName { get; private set; }

        public double MatchTime { get; private set; }

        public IReadOnlyDictionary<string, int> Parameters
        {
            get { return ParametersSingleton.Instance.Parameters; }
        }

        public IReadOnlyCollection<IReadonlyRobot> Robots
        {
            //http://stackoverflow.com/questions/3128889/lock-vs-toarray-for-thread-safe-foreach-access-of-list-collection
            //http://stackoverflow.com/questions/24880268/ienumerable-vs-ireadonlycollection-vs-readonlycollection-for-exposing-a-list-mem
            //http://stackoverflow.com/questions/491375/readonlycollection-or-ienumerable-for-exposing-member-collections
            //http://stackoverflow.com/questions/491375/readonlycollection-or-ienumerable-for-exposing-member-collections/491591#491591
            get // Clone the list to avoid end-user to freeze arena with a Lock on Robots collection
            {
                List<IReadonlyRobot> copy;
                lock (_robots)
                {
                    copy = _robots.Select(x => x as IReadonlyRobot).ToList();
                }
                return copy;
            } 
        }

        public IReadOnlyCollection<IReadonlyMissile> Missiles
        {
            get
            {
                // Clone the list to avoid end-user to freeze arena with a Lock on Missiles collection
                List<IReadonlyMissile> copy;
                lock (_missiles)
                {
                    copy = _missiles.Select(x => x as IReadonlyMissile).ToList();
                }
                return copy;
            }
        }

        public void InitializeSolo(Type robotType, int locX, int locY, int heading, int speed)
        {
            Mode = ArenaModes.Solo;
            InitializeMatch(1, (t, i) => new Tuple<int, int>(locX, locY), robotType);
        }

        public void InitializeSingleMatch(Type team1, Type team2)
        {
            Mode = ArenaModes.Single;
            InitializeMatch(1, (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next()), team1, team2);
        }

        public void InitializeSingleMatch(Type team1, Type team2, int locX1, int locY1, int locX2, int locY2)
        {
            Mode = ArenaModes.Single;
            InitializeMatch(1, (t, i) => new Tuple<int, int>(t == 0 ? locX1 : locX2, t == 0 ? locY1 : locY2), team1, team2);
        }

        public void InitializeSingle4Match(Type team1, Type team2, Type team3, Type team4)
        {
            Mode = ArenaModes.Single4;
            InitializeMatch(1, (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next()), team1, team2, team3, team4);
        }

        public void InitializeDoubleMatch(Type team1, Type team2)
        {
            Mode = ArenaModes.Double;
            InitializeMatch(2, (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next()), team1, team2);
        }

        public void InitializeDouble4Match(Type team1, Type team2, Type team3, Type team4)
        {
            Mode = ArenaModes.Double4;
            InitializeMatch(2, (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next()), team1, team2, team3, team4);
        }

        public void InitializeTeamMatch(Type team1, Type team2, Type team3, Type team4)
        {
            Mode = ArenaModes.Team;
            InitializeMatch(8, (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next()), team1, team2, team3, team4);
        }

        public void InitializeFreeMode(int robotCount, Func<int, int, Tuple<int, int>> getCoordinatesFunc, params Type[] robotTypes)
        {
            Mode = ArenaModes.Free;
            Func<int, int, Tuple<int, int>> randomCoordinatesFunc = (t, i) => new Tuple<int, int>(_randomUnique.Next(), _randomUnique.Next());
            InitializeMatch(robotCount, getCoordinatesFunc ?? randomCoordinatesFunc, robotTypes);
        }
        
        public void StartMatch()
        {
            if (State == ArenaStates.Initialized)
            {
                // Initialize CountdownEvent to robot count, each robot main loop will decrement this value
                _robotSyncCountdownEvent = new CountdownEvent(_robots.Count);

                //
                State = ArenaStates.Starting;
                //
                MatchStart = Tick.Now;

                // Create task, cancellation token and start task
                _cancellationTokenSource = new CancellationTokenSource();
                _mainTask = new Task(MainLoop, _cancellationTokenSource.Token);
                _mainTask.Start();
            }
        }

        public void StopMatch()
        {
            StopMatch(ArenaStates.Stopped);
        }

        #endregion

        #region IArenaRobotInteraction

        public void FindNearestEnemy(IReadonlyRobot robot, out double degrees, out double range, out double x, out double y)
        {
            Robot r = robot as Robot;
            if (r == null)
                throw new ArgumentException("Invalid type for robot");

            degrees = 0;
            range = 0;
            x = 0;
            y = 0;

            double nearest = Double.MaxValue;
            foreach (Robot otherRobot in _robots.Where(t => t.Team != robot.Team))
            {
                double distance = Common.Math.Distance(r.LocX, r.LocY, otherRobot.LocX, otherRobot.LocY);
                if (distance < nearest)
                {
                    nearest = distance;
                    range = distance;
                    x = otherRobot.LocX;
                    y = otherRobot.LocY;
                    degrees = Common.Math.ToDegrees(System.Math.Atan2(otherRobot.LocY - r.LocY, otherRobot.LocX - r.LocX));
                }
            }
            degrees = Common.Math.FixDegrees(degrees);
        }

        public void FireAt(IReadonlyRobot robot, double targetX, double targetY)
        {
            Robot r = robot as Robot;
            if (r == null)
                throw new ArgumentException("Invalid type for robot");

            int heading = (int)System.Math.Round(Common.Math.ToDegrees(Common.Math.ComputeAngle(r.LocX, r.LocY, targetX, targetY)));
            int range = (int)System.Math.Round(Common.Math.Distance(r.LocX, r.LocY, targetX, targetY));
            lock (_missiles)
            {
                Missile missile = new Missile(robot, MatchStart, _missileId, r.LocX, r.LocY, heading, range);
                _missiles.Add(missile);
                _missileId++;
            }
        }

        public int Cannon(IReadonlyRobot robot, Tick launchTick, int degrees, int range)
        {
            Robot r = robot as Robot;
            if (r == null)
                throw new ArgumentException("Invalid type for robot");

            //Log.WriteLine(Log.LogLevels.Debug, "Launching missile from Robot {0}[{1}] to {2} {3}", robot.TeamName, robot.Id, degrees, range);
            lock (_missiles)
            {
                Missile missile = new Missile(robot, MatchStart, _missileId, r.LocX, r.LocY, degrees, range);
                _missiles.Add(missile);
                _missileId++;
            }
            return 1;
        }

        public void Drive(IReadonlyRobot robot, int degrees, int speed)
        {
            // NOP: managed in InternalRobot
        }

        public int Scan(IReadonlyRobot robot, int degrees, int resolution)
        {
            Robot r = robot as Robot;
            if (r == null)
                throw new ArgumentException("Invalid type for robot");

            //double nearest = Double.MaxValue;
            //Robot target = null;
            //foreach (Robot enemy in _robots.Where(x => x != robot && x.State == RobotStates.Running && x.Damage < ParametersSingleton.MaxDamage))
            //{
            //    // Sector method
            //    bool isInSector = Common.Math.IsInSector(robot.LocX, robot.LocY, degrees, resolution, enemy.LocX, enemy.LocY);
            //    if (isInSector)
            //    {
            //        double distance = Common.Math.Distance(robot.LocX, robot.LocY, enemy.LocX, enemy.LocY);
            //        if (distance < nearest)
            //        {
            //            nearest = distance;
            //            target = enemy;
            //        }
            //    }
            //}
            //if (target != null)
            //    Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] found Robot {2}[{3}]", robot.TeamName, robot.Id, target.Id, target.Team);
            //else
            //    Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] failed to find someone else", robot.TeamName, robot.Id);

            double nearest2 = Double.MaxValue;
            Robot target2 = null;
            foreach (Robot enemy in _robots.Where(x => x != robot && x.State == RobotStates.Running && x.Damage < ParametersSingleton.MaxDamage))
            {
                // Angle method
                double angleRadians = Common.Math.ComputeAngle(r.LocX, r.LocY, enemy.LocX, enemy.LocY);
                double angleDegrees = Common.Math.ToDegrees(angleRadians);
                double fixedAngle = Common.Math.FixDegrees(angleDegrees);
                double diff = System.Math.Abs(fixedAngle - degrees);
                if (diff <= resolution/2.0)
                {
                    double distance = Common.Math.Distance(r.LocX, r.LocY, enemy.LocX, enemy.LocY);
                    if (distance < nearest2)
                    {
                        nearest2 = distance;
                        target2 = enemy;
                    }
                }
            }

            ////Debug.Assert(target == target2 && System.Math.Abs(nearest-nearest2) < 0.0001);
            //if (target != target2 || System.Math.Abs(nearest - nearest2) > 0.0001)
            //{
            //    Log.WriteLine(Log.LogLevels.Error, "Different result for sector and angle method: {0:0.0000}|{1:0.0000}   param angle: {2} res: {3}", nearest, nearest2, degrees, resolution);
            //}

            //return target != null ? (int)System.Math.Round(nearest) : 0;
            return target2 != null ? (int)System.Math.Round(nearest2) : 0;
        }

        public int TeamCount(IReadonlyRobot robot)
        {
            return _robots.Count(x => x.Team == robot.Team);
        }

        public int Rand(int limit)
        {
            return _random.Next(limit);
        }

        #endregion

        private void InitializeMatch(int count, Func<int, int, Tuple<int, int>> getCoordinatesFunc, params Type[] teamType)
        {
            try
            {
                _randomUnique.Reset();
                _missiles.Clear();
                _robots.Clear();
                WinningTeam = -1;
                _stepCount = 0;
                _missileId = 0;
                _robotCount = count;

                //
                Type robotType = typeof (SDK.Robot);
                // Check if every team type robot inherits from SDK.Robot
                foreach(Type type in teamType)
                {
                    if (!type.IsSubclassOf(robotType))
                    {
                        Log.WriteLine(Log.LogLevels.Error, "Type {0} must be a subclass of {1}", type, robotType);
                        State = ArenaStates.Error;
                    }
                }

                // Continue only if no error
                if (State != ArenaStates.Error)
                {
                    // Create robots
                    // With 4 teams A, B, C and D; robots are stored ABCDABCDABCD and not AAABBBCCCDDD
                    for (int i = 0; i < count; i++)
                        for (int t = 0; t < teamType.Length; t++)
                        {
                            int x = getCoordinatesFunc(t, i).Item1;
                            int y = getCoordinatesFunc(t, i).Item2;
                            SDK.Robot userRobot = Activator.CreateInstance(teamType[t]) as SDK.Robot;
                            Robot robot = new Robot();
                            robot.Initialize(userRobot, this, teamType[t].Name, i, t, x, y);
                            _robots.Add(robot);

                            Log.WriteLine(Log.LogLevels.Info, "Robot {0}[{1}] Type {2} created at location {3},{4}", robot.TeamName, robot.Id, teamType[t], x, y);
                        }

                    //
                    State = ArenaStates.Initialized;
                }
            }
            catch (Exception ex)
            {
                // TODO: manage exception
                Log.WriteLine(Log.LogLevels.Error, "Exception while start arena. {0}", ex);

                // Force robots to stop
                foreach(Robot robot in _robots)
                    robot.Stop();

                State = ArenaStates.Error;
            }
        }

        private void StopMatch(ArenaStates newState)
        {
            MatchTime = Tick.ElapsedSeconds(MatchStart);
            //
            State = newState;
            // Stop main loop
            _cancellationTokenSource.Cancel();
            // Stop robots
            foreach (Robot robot in _robots)
                robot.Stop(); // TODO: asynchronous stop
            //
            _mainTask.Wait(1000);
        }

        private void MainLoop()
        {
            Log.WriteLine(Log.LogLevels.Info, "Starting robots");

            // Start robots
            foreach (Robot robot in _robots)
                robot.Start(MatchStart, _robotSyncCountdownEvent);

            Log.WriteLine(Log.LogLevels.Debug, "Robots started, waiting on CountdownEvent");

            // Wait until every robot has been started (max 2 seconds)
            _robotSyncCountdownEvent.Wait(2000);

            Log.WriteLine(Log.LogLevels.Debug, "Robots have really been started, CountdownEvent reached 0");

            Log.WriteLine(Log.LogLevels.Info, "Robots started");

            //
            State = ArenaStates.Running;

            //
            FireOnArenaStarted();

            // Start main loop
            Stopwatch sw = new Stopwatch();
            while(true)
            {
                //
                sw.Reset();
                sw.Start();
                //
                Step();
                //
                sw.Stop();
                double elapsed = sw.ElapsedMilliseconds;
                int sleepTime = (int)(ParametersSingleton.StepDelay - elapsed);
                if (sleepTime < 0)
                    sleepTime = 1;
                //Debug.WriteLine("Elapsed {0:0.0000} -> Sleep {1}", elapsed, sleepTime);
                _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Task cancelled. Stopping arena main loop");
                    break;
                }
            }

            FireOnArenaStopped();
        }

        private void Step()
        {
            if (State == ArenaStates.Running)
            {
                Tick now = Tick.Now;
                double elapsed = Tick.TotalSeconds(now, MatchStart);

                if (elapsed > ParametersSingleton.MaxMatchTime)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Match timeout");
                    StopMatch(ArenaStates.Timeout);
                }
                else
                {
                    // Get real step time
                    double realStepTime = _lastStepTick == null ? ParametersSingleton.StepDelay : Tick.TotalMilliseconds(now, _lastStepTick);
                    _lastStepTick = now;

                    //Log.WriteLine(Log.LogLevels.Debug, "STEP: {0} {1:0.0000}  {2:0.00}", _stepCount, elapsed, realStepTime);
                    _stepCount++;

                    // Update robots
                    foreach (Robot robot in _robots.Where(x => x.State == RobotStates.Running))
                    {
                        // Update speed, moderated by acceleration
                        robot.UpdateSpeed(realStepTime);
                        // Update heading; allow change below a certain speed
                        robot.UpdateHeading();
                        // Update distance traveled on this heading, x and y
                        robot.UpdateLocation(realStepTime);
                        // Check collisions
                        if (robot.Speed > 0)
                        {
                            // With other robots
                            Robot robot1 = robot;
                            foreach (Robot other in _robots.Where(x => x != robot1 && x.State == RobotStates.Running))
                            {
                                double diffX = System.Math.Abs(robot.LocX - other.LocX);
                                double diffY = System.Math.Abs(robot.LocY - other.LocY);
                                if (diffX < ParametersSingleton.CollisionDistance && diffY < ParametersSingleton.CollisionDistance) // Collision
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] collides Robot {2}[{3}]", robot.TeamName, robot.Id, other.TeamName, other.Id);
                                    // Damage moving robot and stop it
                                    robot.Collision(ParametersSingleton.CollisionDamage);
                                    // Damage colliding robot
                                    other.Collision(ParametersSingleton.CollisionDamage);
                                }
                            }
                            // With walls
                            if (robot.LocX < 0)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] collides left wall", robot.TeamName, robot.Id);
                                robot.CollisionWall(ParametersSingleton.CollisionDamage, 0, robot.LocY);
                            }
                            else if (robot.LocX >= ParametersSingleton.ArenaSize)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] collides right wall", robot.TeamName, robot.Id);
                                robot.CollisionWall(ParametersSingleton.CollisionDamage, ParametersSingleton.ArenaSize - 1, robot.LocY);
                            }
                            if (robot.LocY < 0)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] collides top wall", robot.TeamName, robot.Id);
                                robot.CollisionWall(ParametersSingleton.CollisionDamage, robot.LocX, 0);
                            }
                            else if (robot.LocY >= ParametersSingleton.ArenaSize)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "Robot {0}[{1}] collides bottom wall", robot.TeamName, robot.Id);
                                robot.CollisionWall(ParametersSingleton.CollisionDamage, robot.LocX, ParametersSingleton.ArenaSize - 1);
                            }
                        }
                    }

                    // Update missiles
                    lock (_missiles)
                    {
                        foreach (Missile missile in _missiles)
                        {
                            if (missile.State == MissileStates.Flying)
                            {
                                missile.UpdatePosition(realStepTime);

                                // Check for missile hitting walls
                                if (missile.LocX < 0)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] collides left wall", missile.Robot.TeamName, missile.Robot.Id);
                                    missile.CollisionWall(0, missile.LocY);
                                }
                                if (missile.LocX >= ParametersSingleton.ArenaSize)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] collides right wall", missile.Robot.TeamName, missile.Robot.Id);
                                    missile.CollisionWall(ParametersSingleton.ArenaSize - 1, missile.LocY);
                                }
                                if (missile.LocY < 0)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] collides top wall", missile.Robot.TeamName, missile.Robot.Id);
                                    missile.CollisionWall(missile.LocX, 0);
                                }
                                if (missile.LocY >= ParametersSingleton.ArenaSize)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] collides bottom wall", missile.Robot.TeamName, missile.Robot.Id);
                                    missile.CollisionWall(missile.LocX, ParametersSingleton.ArenaSize - 1);
                                }

                                // Check for missile reaching target range
                                if (missile.CurrentDistance >= missile.Range)
                                {
                                    //Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] reached its target", missile.Robot.TeamName, missile.Robot.Id);
                                    missile.TargetReached();
                                }

                                // if missile has exploded, inflict damage on all nearby robots, according to hit range
                                if (missile.State == MissileStates.Exploding)
                                {
                                    Robot missileRobot = missile.Robot as Robot;
                                    foreach (Robot robot in _robots.Where(x => x.State == RobotStates.Running))
                                    {
                                        double distance = Common.Math.Distance(robot.LocX, robot.LocY, missile.LocX, missile.LocY);
                                        foreach (DamageByRange damageByRange in _damageByRanges)
                                            if (distance < damageByRange.Range)
                                            {
                                                if (missileRobot != null)
                                                    missileRobot.Statistics.Increment(String.Format("DAMAGE_RANGE_{0}", damageByRange.Range));
                                                if (robot.Team == missile.Robot.Team)
                                                {
                                                    robot.Statistics.Increment("FRIENDLY_DAMAGE_TAKEN");
                                                    if (missileRobot != null)
                                                        missileRobot.Statistics.Increment("FRIENDLY_DAMAGE");
                                                    //Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] damages Robot {2}[{3}] dealing {4} damage, distance {5:0.000} FRIENDLY DAMAGE", missile.Robot.TeamName, missile.Robot.Id, robot.TeamName, robot.Id, damageByRange.Damage, distance);
                                                }
                                                //else
                                                //    Log.WriteLine(Log.LogLevels.Debug, "Missile from Robot {0}[{1}] damages Robot {2}[{3}] dealing {4} damage, distance {5:0.000}", missile.Robot.TeamName, missile.Robot.Id, robot.TeamName, robot.Id, damageByRange.Damage, distance);
                                                robot.Statistics.Increment(String.Format("DAMAGE_TAKEN_RANGE_{0}", damageByRange.Range));
                                                robot.TakeDamage(damageByRange.Damage);
                                                break; // missile in this range, no need to check other ranges
                                            }
                                    }

                                    missile.UpdateExploding();
                                }
                            }
                            else if (missile.State == MissileStates.Exploded)
                                missile.UpdateExploded(ParametersSingleton.ExplosionDisplayDelay);
                        }
                        // Remove deleted missile
                        _missiles.RemoveAll(x => x.State == MissileStates.Deleted);
                    }

                    if (_robots.All(x => x.State != RobotStates.Created && x.State != RobotStates.Initialized && x.State != RobotStates.Starting)) // check only if robot has started
                    {
                        // Check if there is a winning team
                        var teamsWithRobotRunning = _robots.Where(x => x.State == RobotStates.Running).GroupBy(x => x.Team).Select(g => new { Id = g.Key, Name = g.First().TeamName }).ToList();
                        if (teamsWithRobotRunning.Count == 0)
                        {
                            Log.WriteLine(Log.LogLevels.Info, "No robot alive -> Draw");
                            // Draw;
                            StopMatch(ArenaStates.Draw);
                        }
                        else if (teamsWithRobotRunning.Count == 1 && Mode != ArenaModes.Solo)
                        {
                            // And the winner is
                            WinningTeam = teamsWithRobotRunning[0].Id;
                            WinningTeamName = teamsWithRobotRunning[0].Name;
                            Log.WriteLine(Log.LogLevels.Info, "And the winner is {0}", WinningTeamName);

                            StopMatch(ArenaStates.Winner);
                        }
                    }

                    FireOnArenaStep();
                }
            }
        }

        private void FireOnArenaStarted()
        {
            if (ArenaStarted != null)
            {
                //foreach (ArenaStartedHandler h in ArenaStarted.GetInvocationList())
                //{
                //    ArenaStartedHandler handler = h;
                //    //Task.Run(() => handler.Invoke(this));
                //    handler.BeginInvoke(this, BeginInvokeCallback, handler);
                //}
                ArenaStarted(this);
            }
        }

        private void FireOnArenaStopped()
        {
            if (ArenaStopped != null)
            {
                //foreach (ArenaStoppedHandler h in ArenaStopped.GetInvocationList())
                //{
                //    ArenaStoppedHandler handler = h;
                //    //Task.Run(() => handler.Invoke(this));
                //    handler.BeginInvoke(this, BeginInvokeCallback, handler);
                //}
                ArenaStopped(this);
            }
        }

        private void FireOnArenaStep()
        {
            if (ArenaStep != null)
            {
                //foreach (ArenaStepHandler h in ArenaStep.GetInvocationList())
                //{
                //    ArenaStepHandler handler = h;
                //    //Task.Run(() => handler.Invoke(this));
                //    handler.BeginInvoke(this, BeginInvokeCallback, handler);
                //}
                //ArenaStep.BeginInvoke(this, BeginInvokeCallback, ArenaStep);
                ArenaStep(this);
            }
        }

        //private static void BeginInvokeCallback(IAsyncResult ar)
        //{
        //    Action action = ar.AsyncState as Action;
        //    if (action != null)
        //        action.EndInvoke(ar);
        //}
    }
}
