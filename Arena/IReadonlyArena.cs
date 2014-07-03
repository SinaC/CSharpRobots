using System;
using System.Collections.Generic;

namespace Arena
{
    public interface IReadonlyArena
    {
        ArenaStates State { get; }

        int ArenaSize { get; }

        IEnumerable<IReadonlyRobot> Robots { get; }
        IEnumerable<IReadonlyMissile> Missiles { get; }

        void StartTest(Type robotType);
        void StartTest(Type robotType1, int locX1, int locY1, Type robotType2, int locX2, int locY2);

        void StartSingleMatch(Type team1, Type team2);
        void StartDoubleMatch(Type team1, Type team2);
        void StartTeamMatch(Type team1, Type team2, Type team3, Type team4);
        void StopMatch();
    }
}
