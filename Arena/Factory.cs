namespace Arena
{
    public class Factory
    {
        public static IReadonlyArena CreateCRobotArena()
        {
            return new Internal.CRobots.Arena();
        }

        public static IReadonlyArena CreateJRobotArena()
        {
            return new Internal.JRobots.Arena();
        }
    }
}
