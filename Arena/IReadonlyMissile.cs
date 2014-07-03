namespace Arena
{
    public interface IReadonlyMissile
    {
        // Robot shooting the missile
        IReadonlyRobot Robot { get; }

        // State
        MissileStates State { get; }

        // Launch location, heading, range
        int LaunchLocX { get; }
        int LaunchLocY { get; }
        int Heading { get; }
        int Range { get; }

        // Current location
        int LocX { get; }
        int LocY { get; }
    }
}
