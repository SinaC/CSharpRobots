using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arena;
using Common;
using Robots;
using Math = Common.Math;

namespace CSharpRobotsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 60);
            Console.BufferWidth = 80;
            Console.BufferHeight = 60;

            IReadonlyArena arena = Factory.CreateArena();
            arena.ArenaStep += DisplayArena;

            //arena.InitializeSolo(typeof(Surveyor), 0, 500, 0, 0);
            //arena.InitializeSingleMatch(typeof(Counter), typeof(Sniper));
            arena.InitializeSingleMatch(typeof(SinaC), typeof(Surveyor));
            //arena.InitializeTeamMatch(typeof(Follower), typeof(Rabbit), typeof(Counter), typeof(Sniper));
            if (arena.State == ArenaStates.Error)
            {
                Console.WriteLine("Error while initializing arena match!!!");
                return;
            }

            arena.StartMatch();

            bool stopped = false;
            while (!stopped)
            {
                if (arena.State != ArenaStates.Running)
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

        private static int GetX(double x, int maxSize)
        {
            int intX = (int)((x * 50) / maxSize);
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
            DisplayCharInScreen(x, y - 1, '|');
            DisplayCharInScreen(x + 1, y - 1, '/');
            DisplayCharInScreen(x - 1, y, '-');
            DisplayCharInScreen(x, y, '#');
            DisplayCharInScreen(x + 1, y, '-');
            DisplayCharInScreen(x - 1, y + 1, '/');
            DisplayCharInScreen(x, y + 1, '|');
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
                int robotY = ((robot.Team * 2) + robot.Id) * 3;
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
                    Math.ComputePoint(missile.LaunchLocX, missile.LaunchLocY, missile.Range, missile.Heading, out destX, out destY);
                    int screenDestX = GetX(destX, arena.ArenaSize);
                    int screenDestY = GetY(destY, arena.ArenaSize);
                    Console.SetCursorPosition(screenDestX, screenDestY);
                    Console.Write("O");
                }
                else if (missile.State == MissileStates.Exploded || missile.State == MissileStates.Exploding)
                    DisplayExplosion(x, y);
            }
        }

        //static void OnArenaStarted(int id)
        //{
        //    System.Diagnostics.Debug.WriteLine("OnArenaStarted: begin");
        //    Thread.Sleep(5000);
        //    System.Diagnostics.Debug.WriteLine("OnArenaStarted: end");
        //}

        //static void OnArenaStopped(int id, string toto)
        //{
        //    System.Diagnostics.Debug.WriteLine("OnArenaStopped: begin");
        //    Thread.Sleep(5000);
        //    System.Diagnostics.Debug.WriteLine("OnArenaStopped: end");
        //}
        //static void Main2(string[] args)
        //{
        //    Test test = new Test();
        //    test.ArenaStarted += OnArenaStarted;
        //    test.ArenaStarted += OnArenaStarted;
        //    test.ArenaStarted += OnArenaStarted;
        //    test.ArenaStopped += OnArenaStopped;
        //    test.ArenaStopped += OnArenaStopped;
        //    test.ArenaStopped += OnArenaStopped;
        //    System.Diagnostics.Debug.WriteLine("Firing start event");
        //    test.FireStartEvent();
        //    System.Diagnostics.Debug.WriteLine("Event start fired");

        //    System.Diagnostics.Debug.WriteLine("Firing stop event");
        //    //test.FireStopEvent();
        //    System.Diagnostics.Debug.WriteLine("Event stop fired");

        //    Console.ReadKey();
        //}
    }

    //public class Test
    //{
    //    public delegate void ArenaStartedEventHandler(int id);
    //    public event ArenaStartedEventHandler ArenaStarted;

    //    public delegate void ArenaStoppedEventHandler(int id, string reason);
    //    public event ArenaStoppedEventHandler ArenaStopped;

    //    private static void OnActionCompleted(IAsyncResult result)
    //    {
    //        //http://stackoverflow.com/questions/11392978/c-sharp-calling-endinvoke-in-callback-using-generics
    //        dynamic action = result.AsyncState;
    //        action.EndInvoke(result);
    //    }

    //    public void FireStartEvent2()
    //    {
    //        if (ArenaStarted != null)
    //        {
    //            foreach (ArenaStartedEventHandler handler in ArenaStarted.GetInvocationList())
    //                Execute(0, handler.Invoke);
    //            //handler.BeginInvoke(0, OnActionCompleted, handler);
    //        }
    //    }

    //    public void FireStartEvent()
    //    {
    //        if (ArenaStarted != null)
    //        {
    //            //List<Action<int>> invokes = ArenaStarted.GetInvocationList().Cast<ArenaStartedEventHandler>().Select<ArenaStartedEventHandler,Action<int>>(x => x.Invoke).ToList();
    //            //foreach(Action<int> invoke in invokes)
    //            //    Execute(0, invoke);
    //            //ArenaStarted.GetInvocationList().Cast<ArenaStartedEventHandler>().ToList().ForEach(x => Execute(0, x.Invoke));
    //            //Array.ForEach(ArenaStarted.GetInvocationList(), x => Execute(0, (x as ArenaStartedEventHandler).Invoke));

    //            //IAsyncResult res = ArenaStarted.BeginInvoke(0, Callback, ArenaStarted);
    //            System.Diagnostics.Debug.WriteLine("+++FireStartEvent before");
    //            foreach (ArenaStartedEventHandler handler in ArenaStarted.GetInvocationList())
    //            {
    //                ArenaStartedEventHandler handler1 = handler;
    //                Task.Run(() => handler1.Invoke(0));
    //            }
    //            System.Diagnostics.Debug.WriteLine("---FireStartEvent after");
    //        }
    //    }

    //    public void FireStopEvent()
    //    {
    //        if (ArenaStopped != null)
    //            foreach (ArenaStoppedEventHandler handler in ArenaStopped.GetInvocationList())
    //                Execute(0, "toto", handler.Invoke);
    //        //handler.BeginInvoke(0, "toto", OnActionCompleted, handler);
    //    }

    //    //public void Fire<T>(T param, Delegate delegates)
    //    //{
    //    //    foreach(Delegate handler in delegates.GetInvocationList())
    //    //        Execute(param, Callback, handler.DynamicInvoke);
    //    //}

    //    public static void Execute<T>(T param, Action<T> action)
    //    {
    //        action.BeginInvoke(param, Callback, action);
    //    }

    //    public static void Execute<T1, T2>(T1 param1, T2 param2, Action<T1, T2> action)
    //    {
    //        action.BeginInvoke(param1, param2, Callback, action);
    //    }

    //    private static void Callback(IAsyncResult ar)
    //    {
    //        Action action = ar.AsyncState as Action;
    //        if (action != null)
    //            action.EndInvoke(ar);
    //    }
    //}
}
