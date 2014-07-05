using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Clock;

// TODO: add match time limit
// BUG: sometimes robots are not started soon enough -> match is stopped considering a draw

namespace Arena.Internal
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

    internal class Arena : IReadonlyArena
    {
        public static readonly int StepDelay = 15; // !!! this delay is not guaranteed by windows when using System.Timers.Timer (not used anymore), we have to compute real elapsed delay between 2 steps (http://stackoverflow.com/questions/3744032/why-are-net-timers-limited-to-15-ms-resolution)
        public static readonly int ArenaSize = 1000;
        public static readonly double CollisionDistance = 1;
        public static readonly int CollisionDamage = 2;
        public static readonly int ExplosionDisplayDelay = 500;

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

        private readonly ManualResetEvent _stopEvent;
        private Task _mainTask;
        private readonly List<Robot> _robots;
        private readonly List<Missile> _missiles;
        private int _missileId;

        private Tick _lastStepTick;
        private int _stepCount;

        public Random Random { get; private set; }

        internal Arena()
        {
            // Initialize Random
            Random = new Random();

            // Initialize stop event and task
            _stopEvent = new ManualResetEvent(false);

            //
            _robots = new List<Robot>();
            _missiles = new List<Missile>();
            
            //
            State = ArenaStates.Created;
        }

        #region IReadonlyArena

        public ArenaStates State { get; private set; }

        public ArenaModes Mode { get; private set; }

        public Tick MatchStart { get; private set; }

        int IReadonlyArena.ArenaSize
        {
            get { return ArenaSize; }
        }

        public int WinningTeam { get; private set; }

        public List<IReadonlyRobot> Robots
        {
            //http://stackoverflow.com/questions/3128889/lock-vs-toarray-for-thread-safe-foreach-access-of-list-collection
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

        public List<IReadonlyMissile> Missiles
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

        public void StartSolo(Type robotType, int locX, int locY, int heading, int speed)
        {
            try
            {
                Mode = ArenaModes.Solo;

                _missiles.Clear();
                _robots.Clear();
                WinningTeam = -1;
                _stepCount = 0;
                _missileId = 0;

                Type robotSDKType = typeof (SDK.Robot);
                if (!robotType.IsSubclassOf(robotSDKType))
                {
                    Debug.WriteLine("Type {0} must be a subclass of {1}", robotType, robotSDKType);
                    State = ArenaStates.Error;
                }
                else
                {
                    // Create test robot
                    SDK.Robot userRobot = Activator.CreateInstance(robotType) as SDK.Robot;
                    Robot robot = new Robot();
                    robot.Initialize(userRobot, this, 0, 0, locX, locY, heading, speed);
                    _robots.Add(robot);

                    Debug.WriteLine("Test Robot Type {0} created at location {1},{2}", robotType, robot.LocX, robot.LocY);

                    State = ArenaStates.Running;

                    // 
                    MatchStart = Tick.Now;

                    // Start task and reset stop event
                    _stopEvent.Reset();
                    _mainTask = new Task(MainLoop);
                    _mainTask.Start();

                    // Start robot
                    robot.Start(MatchStart);
                }
            }
            catch (Exception ex)
            {
                // TODO: manage exception
                Debug.WriteLine("Exception while start solo mode. {0}", ex);

                // Force robots to stop
                foreach (Robot robot in _robots)
                    robot.Stop();

                State = ArenaStates.Error;
            }
        }

        public void StartSingleMatch(Type team1, Type team2)
        {
            Mode = ArenaModes.Single;
            StartMatch(1, team1, team2);
        }

        public void StartDoubleMatch(Type team1, Type team2)
        {
            Mode = ArenaModes.Double;
            StartMatch(2, team1, team2);
        }

        public void StartTeamMatch(Type team1, Type team2, Type team3, Type team4)
        {
            Mode = ArenaModes.Team;
            StartMatch(8, team1, team2, team3, team4);
        }

        public void StopMatch()
        {
            StopMatch(ArenaStates.Stopped);
        }

        #endregion

        public int Cannon(Robot robot, Tick launchTick, int degrees, int range)
        {
            Debug.WriteLine("Launching missile from Robot {0}[{1}] to {2} {3}", robot.Name, robot.Id, degrees, range);
            lock (_missiles)
            {
                Missile missile = new Missile(robot, _missileId, robot.RawLocX, robot.RawLocY, degrees, range);
                _missiles.Add(missile);
                _missileId++;
            }
            return 1;
        }

        public void Drive(Robot robot, int degrees, int speed)
        {
            // NOP: managed in InternalRobot
        }

        public int Scan(Robot robot, int degrees, int resolution)
        {
            double nearest = double.MaxValue;
            Robot target = null;
            foreach (Robot r in _robots.Where(x => x != robot && x.State == RobotStates.Running))
            {
                // TODO: use real position not last computed position (use range, originX, originY and speed)
                bool isInSector = Common.Helpers.Math.IsInSector(robot.RawLocX, robot.RawLocY, degrees, resolution, r.RawLocX, r.RawLocY);
                if (isInSector)
                {
                    double distance = Common.Helpers.Math.Distance(robot.RawLocX, robot.RawLocY, r.RawLocX, r.RawLocY);
                    if (distance < nearest)
                    {
                        nearest = distance;
                        target = r;
                    }
                }
            }
            //if (target != null)
            //    Debug.WriteLine("Robot {0}[{1}] found Robot {2}[{3}]", robot.Name, robot.Id, target.Id, target.Team);
            //else
            //    Debug.WriteLine("Robot {0}[{1}] failed to find someone else", robot.Name, robot.Id);
            return target != null ? (int)nearest : 0;
        }

        public int TeamCount(Robot robot)
        {
            return _robots.Count(x => x.Team == robot.Team);
        }

        private void StartMatch(int count, params Type[] teamType)
        {
            try
            {
                _missiles.Clear();
                _robots.Clear();
                WinningTeam = -1;
                _stepCount = 0;
                _missileId = 0;

                //
                Type robotType = typeof (SDK.Robot);
                // Check if every team type robot inherits from SDK.Robot
                foreach(Type type in teamType)
                {
                    if (!type.IsSubclassOf(robotType))
                    {
                        Debug.WriteLine("Type {0} must be a subclass of {1}", type, robotType);
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
                            int x = Random.Next(ArenaSize); // TODO: cannot give same position as another robot
                            int y = Random.Next(ArenaSize);
                            SDK.Robot userRobot = Activator.CreateInstance(teamType[t]) as SDK.Robot;
                            Robot robot = new Robot();
                            robot.Initialize(userRobot, this, i, t, x, y);
                            _robots.Add(robot);

                            Debug.WriteLine("Robot {0} | {1} | {2} Type {3} created at location {4},{5}", i, t, robot.Name, teamType[t], x, y);
                        }

                    //
                    State = ArenaStates.Running;

                    //
                    MatchStart = Tick.Now;

                    // Start task and reset stop event
                    _stopEvent.Reset();
                    _mainTask = new Task(MainLoop);
                    _mainTask.Start();

                    // Start robots
                    foreach (Robot robot in _robots)
                        robot.Start(MatchStart);
                }
            }
            catch (Exception ex)
            {
                // TODO: manage exception
                Debug.WriteLine("Exception while start arena. {0}", ex);

                // Force robots to stop
                foreach(Robot robot in _robots)
                    robot.Stop();

                State = ArenaStates.Error;
            }
        }

        private void StopMatch(ArenaStates newState)
        {
            State = newState;
            // Stop main event
            _stopEvent.Set();
            // Stop robots
            foreach (Robot robot in _robots)
                robot.Stop(); // TODO: asynchronous stop
            //
            _mainTask.Wait(1000);
        }

        private void MainLoop()
        {
            Stopwatch sw = new Stopwatch();
            while(true)
            {
                sw.Reset();
                sw.Start();

                Step();

                sw.Stop();
                double elapsed = sw.ElapsedMilliseconds;
                int sleepTime = (int)(StepDelay - elapsed);
                if (sleepTime < 0)
                    sleepTime = 1;
                //Debug.WriteLine("Elapsed {0:0.0000} -> Sleep {1}", elapsed, sleepTime);
                bool stopAsked = _stopEvent.WaitOne(sleepTime);
                if (stopAsked)
                {
                    Debug.WriteLine("Stop event received. Stopping main loop");
                    break;
                }
            }
        }

        private void Step()
        {
            if (State == ArenaStates.Running)
            {
                Tick now = Tick.Now;
                double elapsed = Tick.TotalSeconds(now, MatchStart);

                // Get real step time
                double realStepTime = _lastStepTick == null ? StepDelay : Tick.TotalMilliseconds(now, _lastStepTick);
                _lastStepTick = now;

                //Debug.WriteLine("STEP: {0} {1:0.0000}  {2:0.00}", _stepCount, elapsed, realStepTime);
                _stepCount++;

                // Update robots
                foreach (Robot robot in _robots.Where(x => x.State == RobotStates.Running))
                {
                    // Update speed, moderated by acceleration
                    robot.UpdateSpeed(realStepTime);
                    // Update heading; allow change below a certain speed
                    robot.UpdateHeading();
                    // Update distance traveled on this heading, x & y
                    robot.UpdateLocation(realStepTime);
                    // Check collisions
                    if (robot.Speed > 0)
                    {
                        // With other robots
                        Robot robot1 = robot;
                        foreach (Robot other in _robots.Where(x => x != robot1 && x.State == RobotStates.Running))
                        {
                            double diffX = Math.Abs(robot.RawLocX - other.RawLocX);
                            double diffY = Math.Abs(robot.RawLocY - other.RawLocY);
                            if (diffX < CollisionDistance && diffY < CollisionDistance) // Collision
                            {
                                Debug.WriteLine("Robot {0}[{1}] collides Robot {2}[{3}]", robot.Name, robot.Id, other.Name, other.Id);
                                // Damage moving robot and stop it
                                robot.Collision(CollisionDamage);
                                // Damage colliding robot
                                other.Collision(CollisionDamage);
                            }
                        }
                        // With walls
                        if (robot.RawLocX < 0)
                        {
                            Debug.WriteLine("Robot {0}[{1}] collides left wall", robot.Name, robot.Id);
                            robot.CollisionWall(CollisionDamage, 0, robot.RawLocY);
                        }
                        else if (robot.RawLocX >= ArenaSize)
                        {
                            Debug.WriteLine("Robot {0}[{1}] collides right wall", robot.Name, robot.Id);
                            robot.CollisionWall(CollisionDamage, ArenaSize - 1, robot.RawLocY);
                        }
                        if (robot.RawLocY < 0)
                        {
                            Debug.WriteLine("Robot {0}[{1}] collides top wall", robot.Name, robot.Id);
                            robot.CollisionWall(CollisionDamage, robot.RawLocX, 0);
                        }
                        else if (robot.RawLocY >= ArenaSize)
                        {
                            Debug.WriteLine("Robot {0}[{1}] collides bottom wall", robot.Name, robot.Id);
                            robot.CollisionWall(CollisionDamage, robot.RawLocX, ArenaSize - 1);
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
                                Debug.WriteLine("Missile from Robot {0}[{1}] collides left wall", missile.Robot.Name, missile.Robot.Id);
                                missile.CollisionWall(0, missile.LocY);
                            }
                            if (missile.LocX >= ArenaSize)
                            {
                                Debug.WriteLine("Missile from Robot {0}[{1}] collides rigth wall", missile.Robot.Name, missile.Robot.Id);
                                missile.CollisionWall(ArenaSize - 1, missile.LocY);
                            }
                            if (missile.LocY < 0)
                            {
                                Debug.WriteLine("Missile from Robot {0}[{1}] collides top wall", missile.Robot.Name, missile.Robot.Id);
                                missile.CollisionWall(missile.LocX, 0);
                            }
                            if (missile.LocY >= ArenaSize)
                            {
                                Debug.WriteLine("Missile from Robot {0}[{1}] collides bottom wall", missile.Robot.Name, missile.Robot.Id);
                                missile.CollisionWall(missile.LocX, ArenaSize - 1);
                            }

                            // Check for missile reaching target range
                            if (missile.CurrentDistance >= missile.Range)
                            {
                                Debug.WriteLine("Missile from Robot {0}[{1}] reached its target", missile.Robot.Name, missile.Robot.Id);
                                missile.TargetReached();
                            }

                            // if missile has exploded, inflict damage on all nearby robots, according to hit range
                            if (missile.State == MissileStates.Exploding)
                            {
                                foreach (Robot robot in _robots.Where(x => x.State == RobotStates.Running))
                                {
                                    double distance = Common.Helpers.Math.Distance(robot.RawLocX, robot.RawLocY, missile.LocX, missile.LocY);
                                    foreach (DamageByRange damageByRange in _damageByRanges)
                                        if (distance < damageByRange.Range)
                                        {
                                            Debug.WriteLine("Missile from Robot {0}[{1}] damages Robot {2}[{3}] dealing {4} damage, distance {5:0.000}", missile.Robot.Name, missile.Robot.Id, robot.Name, robot.Id, damageByRange.Damage, distance);
                                            robot.TakeDamage(damageByRange.Damage);
                                            break; // missile in this range, no need to check other ranges
                                        }
                                }

                                missile.UpdateExploding();
                            }
                        }
                        else if (missile.State == MissileStates.Exploded)
                            missile.UpdateExploded(ExplosionDisplayDelay);
                    }
                    // Remove deleted missile
                    _missiles.RemoveAll(x => x.State == MissileStates.Deleted);
                }

                if (_robots.All(x => x.State != RobotStates.Created && x.State != RobotStates.Initialized)) // check only if robot has started
                {
                    // Check if there is a winning team
                    List<int> teamsWithRobotRunning = _robots.Where(x => x.State == RobotStates.Running).GroupBy(x => x.Team).Select(g => g.Key).ToList();
                    if (teamsWithRobotRunning.Count == 0)
                    {
                        Debug.WriteLine("No robot alive -> Draw");
                        // Draw
                        StopMatch(ArenaStates.NoWinner);
                    }
                    else if (teamsWithRobotRunning.Count == 1 && Mode != ArenaModes.Solo)
                    {
                        // And the winner is
                        WinningTeam = teamsWithRobotRunning[0];
                        Debug.WriteLine("And the winner is {0}", WinningTeam);

                        StopMatch(ArenaStates.Winner);
                    }
                }
            }
        }
    }
}
