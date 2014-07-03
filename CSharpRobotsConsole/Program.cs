using System;
using Arena;
using Common.Clock;
using Robots;

namespace CSharpRobotsConsole
{
    class Program
    {
        private static int GetX(double x, int maxSize)
        {
            int intX = (int)((x*50)/maxSize);
            if (intX < 0)
                intX = 0;
            if (intX > 49)
                intX = 49;
            return intX;
        }

        private static int GetY(double y, int maxSize)
        {
            int intY = (int)((y * 50) / maxSize);
            if (intY < 0)
                intY = 0;
            if (intY > 49)
                intY = 49;
            return intY;
        }

        private static void DisplayCharInScreen(int x, int y, char c)
        {
            if (x < 0 || x > 49 || y < 0 || y > 49)
                return;
            Console.SetCursorPosition(x, y);
            Console.Write(c);
        }

        private static void DisplayExplosion(int x, int y)
        {
            // \|/
            // -#-
            // /|\
            DisplayCharInScreen(x - 1, y - 1, '\\');
            DisplayCharInScreen(x    , y - 1, '|');
            DisplayCharInScreen(x + 1, y - 1, '/');
            DisplayCharInScreen(x - 1, y    , '-');
            DisplayCharInScreen(x    , y    , '#');
            DisplayCharInScreen(x + 1, y    , '-');
            DisplayCharInScreen(x - 1, y + 1, '/');
            DisplayCharInScreen(x,     y + 1, '|');
            DisplayCharInScreen(x + 1, y + 1, '\\');
        }

        private static void DisplayArena(IReadonlyArena arena)
        {
            Console.Clear();
            // Display information
            double elapsed = Tick.ElapsedSeconds(arena.MatchStart);
            Console.SetCursorPosition(0, 51);
            Console.Write("Time: {0:0.00}", elapsed);
            // Display robots
            foreach (IReadonlyRobot robot in arena.Robots)
            {
                int x = GetX(robot.LocX, arena.ArenaSize);
                int y = GetY(robot.LocY, arena.ArenaSize);

                Console.SetCursorPosition(x, y);
                Console.Write(robot.Team);

                // Display robot information (3 lines per robot)
                int robotY = ((robot.Team*2) + robot.Id)*3;
                Console.SetCursorPosition(51, robotY);
                Console.Write("{0}|{1} Dmg:{2}", robot.Id, robot.Team, robot.Damage);
                Console.SetCursorPosition(51, robotY + 1);
                Console.Write("{0},{1} H{2} S{3}", robot.LocX, robot.LocY, robot.Heading, robot.Speed);
                Console.SetCursorPosition(51, robotY + 2);
                Console.Write("--------------");
            }
            // Display missiles
            foreach (IReadonlyMissile missile in arena.Missiles)
            {
                int x = GetX(missile.LocX, arena.ArenaSize);
                int y = GetY(missile.LocY, arena.ArenaSize);

                if (missile.State == MissileStates.Flying)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write("+");
                    
                    double destX, destY;
                    Common.Helpers.Math.ComputePoint(missile.LaunchLocX, missile.LaunchLocY, missile.Range, missile.Heading, out destX, out destY);
                    int screenDestX = GetX(destX, arena.ArenaSize);
                    int screenDestY = GetY(destY, arena.ArenaSize);
                    Console.SetCursorPosition(screenDestX, screenDestY);
                    Console.Write("O");
                }
                else if (missile.State == MissileStates.Exploded || missile.State == MissileStates.Exploding)
                    DisplayExplosion(x, y);
            }
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 60);
            Console.BufferWidth = 80;
            Console.BufferHeight = 60;

            IReadonlyArena arena = Factory.CreateArena();
            //arena.StartSolo(typeof (Follower), 500, 500, typeof (Robots.Rabbit), 400, 400);
            //arena.StartSolo(typeof(CrazyCannon));
            arena.StartSolo(typeof(Surveyor), 0, 500, 0, 100);
            //arena.StartSingleMatch(typeof(Counter), typeof(Counter));
            //arena.StartTeamMatch(typeof(Follower), typeof(Rabbit), typeof(Rook), typeof(Sniper));
            //System.Threading.Thread.Sleep(2000);
            //arena.StopMatch();
            //System.Threading.Thread.Sleep(2000);
            if (arena.State == ArenaStates.Error)
            {
                Console.WriteLine("Error while starting arena match!!!");
                return;
            }
            bool stopped = false;
            while (!stopped)
            {
                if (arena.State == ArenaStates.Running)
                    DisplayArena(arena);
                else
                {
                    if (arena.State == ArenaStates.Error)
                        Console.WriteLine("Arena is in error");
                    else if (arena.State == ArenaStates.Winner)
                        Console.WriteLine("Winner is team {0}", arena.WinningTeam);
                    else if (arena.State == ArenaStates.NoWinner)
                        Console.WriteLine("No winner");
                    stopped = true;
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                            //
                        case ConsoleKey.X:
                            stopped = true;
                            arena.StopMatch();
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            System.Threading.Thread.Sleep(500);
        }
    }
}
