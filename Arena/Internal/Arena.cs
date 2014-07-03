using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Common.Clock;
using SDK;

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
        public static readonly int ArenaSize = 1000;
        public static readonly double CollisionDistance = 1;
        public static readonly int CollisionDamage = 2;
        public static readonly int ExplosionDisplayDelay = 500;

        private struct DamageByRange
        {
            public int Range;
            public int Damage;
        }

        private readonly DamageByRange[] _damageByRanges = new [] 
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
                    },
            };

        private readonly Timer _timer;
        private Tick _matchStart;

        private readonly List<Robot> _robots;
        private readonly List<Missile> _missiles;

        public int StepDelay { get { return 50; } }
        public Random Random { get; private set; }

        internal Arena()
        {
            // Initialize Tick and Random
            Random = new Random();
            // Initialize Timer
            _timer = new Timer(StepDelay);
            _timer.Elapsed += TimerElapsed;

            _robots = new List<Robot>();
            _missiles = new List<Missile>();

            State = ArenaStates.Created;
        }

        #region IReadonlyArena

        public ArenaStates State { get; private set; }

        int IReadonlyArena.ArenaSize { get { return ArenaSize; } }

        public IEnumerable<IReadonlyRobot> Robots
        {
            get { return _robots; }
        }

        public IEnumerable<IReadonlyMissile> Missiles
        {
            get
            {
                return _missiles;
            }
        }

        public void StartTest(Type robotType)
        {
            _missiles.Clear();
            _robots.Clear();

            // Create test robot
            SDK.Robot userRobot = Activator.CreateInstance(robotType) as SDK.Robot;
            Robot robot = new Robot();
            robot.Initialize(userRobot, this, 0, 0, 500, 500);
            _robots.Add(robot);

            System.Diagnostics.Debug.WriteLine("Test Robot Type {0} created at location {1},{2}", robotType, robot.LocX, robot.LocY);

            // Start robot
            _matchStart = Tick.Now;
            robot.Start(_matchStart);

            // Start timer
            _timer.Start();

            State = ArenaStates.Started;
        }

        public void StartTest(Type robotType1, int locX1, int locY1, Type robotType2, int locX2, int locY2)
        {
            _missiles.Clear();
            _robots.Clear();

            // Create test robots
            SDK.Robot userRobot1 = Activator.CreateInstance(robotType1) as SDK.Robot;
            Robot robot1 = new Robot();
            robot1.Initialize(userRobot1, this, 0, 0, locX1, locY1);
            _robots.Add(robot1);

            SDK.Robot userRobot2 = Activator.CreateInstance(robotType2) as SDK.Robot;
            Robot robot2 = new Robot();
            robot2.Initialize(userRobot2, this, 0, 0, locX2, locY2);
            _robots.Add(robot2);

            // Start robots
            _matchStart = Tick.Now;
            robot1.Start(_matchStart);
            robot2.Start(_matchStart);

            // Start timer
            _timer.Start();

            State = ArenaStates.Started;
        }

        public void StartSingleMatch(Type team1, Type team2)
        {
            StartMatch(1, team1, team2);
        }

        public void StartDoubleMatch(Type team1, Type team2)
        {
            StartMatch(2, team1, team2);
        }

        public void StartTeamMatch(Type team1, Type team2, Type team3, Type team4)
        {
            StartMatch(8, team1, team2, team3, team4);
        }

        public void StopMatch()
        {
            _timer.Stop();
            foreach (Robot robot in _robots)
                robot.Stop(); // TODO: asynchronous stop
        }

        #endregion

        public int Cannon(Robot robot, Tick launchTick, int degrees, int range)
        {
            System.Diagnostics.Debug.WriteLine("Launching missile from Robot {0} | {1} to {2} {3}", robot.Id, robot.Team, degrees, range);
            Missile missile = new Missile(this, robot, robot.RawLocX, robot.RawLocY, degrees, range);
            lock (_missiles)
            {
                _missiles.Add(missile);
            }
            return 0;
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
            if (target != null)
                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} found Robot {2} | {3}", robot.Id, robot.Team, target.Id, target.Team);
            else
                System.Diagnostics.Debug.WriteLine("Robot {0} | {1} failed to find someone else", robot.Id, robot.Team);
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

                //
                Type SDKRobotType = typeof (SDK.Robot);
                // Check if every team type robot inherits from SDK.Robot
                foreach(Type type in teamType)
                {
                    if (!type.IsSubclassOf(SDKRobotType))
                    {
                        System.Diagnostics.Debug.WriteLine("Type {0} must be a subclass of {1}", type, SDKRobotType);
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

                            System.Diagnostics.Debug.WriteLine("Robot {0} | {1} Type {2} created at location {3},{4}", i, t, teamType[t], x, y);
                        }

                    // Start robots
                    _matchStart = Tick.Now;
                    foreach (Robot robot in _robots)
                        robot.Start(_matchStart);

                    // Start timer
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                // TODO: manage exception

                // Force robots to stop
                foreach(Robot robot in _robots)
                    robot.Stop();

                State = ArenaStates.Error;
            }
        }

        private void TimerElapsed(object source, ElapsedEventArgs e)
        {
            Step();
        }

        private void Step()
        {
            // Update robots
            foreach (Robot robot in _robots.Where(x => x.State == RobotStates.Running))
            {
                // Update speed, moderated by acceleration
                robot.UpdateSpeed();
                // Update heading; allow change below a certain speed
                robot.UpdateHeading();
                // Update distance traveled on this heading, x & y
                robot.UpdateLocation();
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
                            System.Diagnostics.Debug.WriteLine("Robot {0} | {1} collides Robot {2} | {3}", robot.Id, robot.Team, other.Id, other.Team);
                            // Damage moving robot and stop it
                            robot.Collision(CollisionDamage);
                            // Damage colliding robot
                            other.Collision(CollisionDamage);
                        }
                    }
                    // With walls
                    if (robot.RawLocX < 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} collides left wall", robot.Id, robot.Team);
                        robot.CollisionWall(CollisionDamage, 0, robot.RawLocY);
                    }
                    else if (robot.RawLocX >= ArenaSize)
                    {
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} collides right wall", robot.Id, robot.Team);
                        robot.CollisionWall(CollisionDamage, ArenaSize - 1, robot.RawLocY);
                    }
                    if (robot.RawLocY < 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} collides top wall", robot.Id, robot.Team);
                        robot.CollisionWall(CollisionDamage, robot.RawLocX, 0);
                    }
                    else if (robot.RawLocY >= ArenaSize)
                    {
                        System.Diagnostics.Debug.WriteLine("Robot {0} | {1} collides bottom wall", robot.Id, robot.Team);
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
                        missile.UpdatePosition();

                        // Check for missile hitting walls
                        if (missile.LocX < 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Missile from Robot {0} | {1} collides left wall", missile.Robot.Id, missile.Robot.Team);
                            missile.CollisionWall(0, missile.LocY);
                        }
                        if (missile.LocX >= ArenaSize)
                        {
                            System.Diagnostics.Debug.WriteLine("Missile from Robot {0} | {1} collides rigth wall", missile.Robot.Id, missile.Robot.Team);
                            missile.CollisionWall(ArenaSize - 1, missile.LocY);
                        }
                        if (missile.LocY < 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Missile from Robot {0} | {1} collides top wall", missile.Robot.Id, missile.Robot.Team);
                            missile.CollisionWall(missile.LocX, 0);
                        }
                        if (missile.LocY >= ArenaSize)
                        {
                            System.Diagnostics.Debug.WriteLine("Missile from Robot {0} | {1} collides bottom wall", missile.Robot.Id, missile.Robot.Team);
                            missile.CollisionWall(missile.LocX, ArenaSize - 1);
                        }

                        // Check for missile reaching target range
                        if (missile.CurrentDistance >= missile.Range)
                        {
                            System.Diagnostics.Debug.WriteLine("Missile from Robot {0} | {1} reached its target", missile.Robot.Id, missile.Robot.Team);
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

            // Check if there is a winning team

        }
    }
}
