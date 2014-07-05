namespace Arena
{
    public interface IReadonlyRobot
    {
        int Team { get; }
        int Id { get; }
        string Name { get; }

        RobotStates State { get; }

        int LocX { get; }
        int LocY { get; }

        int Heading { get; }

        int Speed { get; }

        int Damage { get; }

        int CannonCount { get; }
    }
}
