using System;
using Arena;
using Robots;

namespace CSharpRobotsConsole
{
    class Program
    {
        private static int GetX(int x, int maxSize)
        {
            x = (x*50)/maxSize;
            if (x < 0)
                x = 0;
            if (x > 49)
                x = 49;
            return x;
        }

        private static int GetY(int y, int maxSize)
        {
            y = (y * 50) / maxSize;
            if (y < 0)
                y = 0;
            if (y > 49)
                y = 49;
            return y;
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
                Console.SetCursorPosition(51, robotY+1);
                Console.Write("{0},{1} H{2}", robot.LocX, robot.LocY, robot.Heading);
                Console.SetCursorPosition(51, robotY + 2);
                Console.Write("--------------");
            }
            // Display missiles
            lock (arena.Missiles)
            {
                foreach (IReadonlyMissile missile in arena.Missiles)
                {
                    int x = GetX((int) missile.LocX, arena.ArenaSize);
                    int y = GetY((int) missile.LocY, arena.ArenaSize);

                    if (missile.State == MissileStates.Flying)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write("+");
                    }
                    else if (missile.State == MissileStates.Exploded || missile.State == MissileStates.Exploding)
                        DisplayExplosion(x, y);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 51);
            Console.BufferWidth = 80;
            Console.BufferHeight = 51;

            IReadonlyArena arena = Factory.CreateArena();
            //arena.StartTest(typeof (Follower), 500, 500, typeof (Robots.Rabbit), 400, 400);
            //arena.StartTest(typeof(CrazyCannon));
            arena.StartSingleMatch(typeof(Sniper), typeof(Target));
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
                DisplayArena(arena);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                            //
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            arena.StopMatch();
            System.Threading.Thread.Sleep(500);
        }
    }
}
