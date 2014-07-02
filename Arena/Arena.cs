using System;
using System.Linq;
using Common.Clock;
using SDK;

namespace Arena
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

    public sealed class Arena : IArena
    {
        public static readonly int ArenaSize = 1000;
        public static readonly double CollisionDistance = 1;
        public static readonly int CollisionDamage = 2;

        private Random _random;
        private Tick _matchStart;

        public InternalRobot[] Robots { get; private set; }

        public void StartSingleMatch(Type team1, Type team2)
        {
            Initialize(1, team1, team2);
        }

        public void StartDoubleMatch(Type team1, Type team2)
        {
            Initialize(2, team1, team2);
        }

        public void StartTeamMatch(Type team1, Type team2, Type team3, Type team4)
        {
            Initialize(8, team1, team2, team3, team4);
        }

        public void StopMatch()
        {
            foreach (InternalRobot robot in Robots)
                robot.Stop();
        }

        private void Initialize(int count, params Type[] teamType)
        {
            // Initialize Tick and Random
            _random = new Random();

            try
            {
                // Create robots
                Robots = new InternalRobot[count*teamType.Length];
                for (int i = 0; i < count; i++)
                    for (int t = 0; t < teamType.Length; t++)
                    {
                        int x = _random.Next(ArenaSize);
                        int y = _random.Next(ArenaSize);
                        Robot userRobot = Activator.CreateInstance(teamType[t]) as Robot;
                        InternalRobot robot = new InternalRobot();
                        robot.Initialize(userRobot, this, _random, i, t, x, y);
                        Robots[i*teamType.Length + t] = robot;
                    }

                // Start robots
                _matchStart = Tick.Now;
                foreach (InternalRobot robot in Robots)
                    robot.Start(_matchStart);
            }
            catch (Exception ex)
            {
                // TODO: manage exception
            }
        }

        public int Cannon(InternalRobot robot, Tick launchTick, int degrees, int range)
        {
            // TODO
            return 0;
        }

        public int Drive(InternalRobot robot, int degrees, int speed)
        {
            // TODO
            return 0;
        }

        public int Scan(InternalRobot robot, int degrees, int resolution)
        {
            bool found = false;
            double nearest = double.MaxValue;
            foreach (InternalRobot r in Robots.Where(x => x != robot && x.State == RobotStates.Running))
            {
                // Should use real position not last computed position (use range, originX, originY and speed)
                bool isInSector = Common.Helpers.Math.IsInSector(robot.LocX, robot.LocY, degrees, resolution, r.LocX, r.LocY);
                if (isInSector)
                {
                    found = true;
                    double distance = Common.Helpers.Math.Distance(robot.LocX, robot.LocY, r.LocX, r.LocY);
                    if (distance < nearest)
                        nearest = distance;
                }
            }
            return found ? (int) nearest : 0;
        }

        public int TeamCount(InternalRobot robot)
        {
            return Robots.Count(x => x.Team == robot.Team);
        }

        private void Step()
        {
            foreach (InternalRobot robot in Robots.Where(x => x.State == RobotStates.Running))
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
                    foreach (InternalRobot other in Robots.Where(x => x != robot && x.State == RobotStates.Running))
                    {
                        double diffX = Math.Abs(robot.RawLocX - other.RawLocX);
                        double diffY = Math.Abs(robot.RawLocY - other.RawLocY);
                        if (diffX < CollisionDistance || diffY < CollisionDistance) // Collision
                        {
                            // Damage moving robot and stop it
                            robot.Collision(CollisionDamage);
                            // Damage colliding robot
                            other.Collision(CollisionDamage);
                        }
                    }
                    // With walls
                    if (robot.LocX < 0)
                        robot.CollisionWall(CollisionDamage, 0, robot.RawLocY);
                    else if (robot.LocX >= ArenaSize)
                        robot.CollisionWall(CollisionDamage, ArenaSize - 1, robot.RawLocY);
                    if (robot.LocY < 0)
                        robot.CollisionWall(CollisionDamage, robot.RawLocX, 0);
                    else if (robot.LocY >= ArenaSize)
                        robot.CollisionWall(CollisionDamage, robot.RawLocX, ArenaSize - 1);
                }
            }
        }
    }
}
