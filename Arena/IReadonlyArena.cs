using System;
using System.Collections.Generic;
using Common;

namespace Arena
{
    public delegate void ArenaStartedHandler(IReadonlyArena arena);
    public delegate void ArenaStoppedHandler(IReadonlyArena arena);
    public delegate void ArenaStepHandler(IReadonlyArena arena);

    public interface IReadonlyArena
    {
        ArenaStates State { get; }
        ArenaModes Mode { get; }

        Tick MatchStart { get; }

        int ArenaSize { get; }

        int WinningTeam { get; }

        List<IReadonlyRobot> Robots { get; }
        List<IReadonlyMissile> Missiles { get; }

        IReadOnlyDictionary<string, int> Parameters { get; }

        event ArenaStartedHandler ArenaStarted;
        event ArenaStoppedHandler ArenaStopped;
        event ArenaStepHandler ArenaStep;

        void InitializeSolo(Type robotType, int locX, int locY, int heading, int speed);
        void InitializeSingleMatch(Type team1, Type team2);
        void InitializeSingleMatch(Type team1, Type team2, int locX1, int locY1, int locX2, int locY2);
        void InitializeDoubleMatch(Type team1, Type team2);
        void InitializeTeamMatch(Type team1, Type team2, Type team3, Type team4);

        void StartMatch();
        void StopMatch();
    }
}
