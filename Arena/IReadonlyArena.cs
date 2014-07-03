using System;
using System.Collections.Generic;
using Common.Clock;

namespace Arena
{
    public interface IReadonlyArena
    {
        ArenaStates State { get; }
        ArenaModes Mode { get; }

        Tick MatchStart { get; }

        int ArenaSize { get; }

        int WinningTeam { get; }

        List<IReadonlyRobot> Robots { get; }
        List<IReadonlyMissile> Missiles { get; }

        void StartSolo(Type robotType, int locX, int locY, int heading, int speed);
        void StartSingleMatch(Type team1, Type team2);
        void StartDoubleMatch(Type team1, Type team2);
        void StartTeamMatch(Type team1, Type team2, Type team3, Type team4);
        void StopMatch();
    }
}
