using Common;

namespace Arena.Internal
{
    public interface IArenaRobotInteraction // Methods in Arena called from Robot
    {
        // Cheat
        void FindNearestEnemy(IReadonlyRobot robot, out double degrees, out double range, out double x, out double y);
        void FireAt(IReadonlyRobot robot, double targetX, double targetY);

        // Normal
        int Cannon(IReadonlyRobot robot, Tick launchTick, int degrees, int range);
        void Drive(IReadonlyRobot robot, int degrees, int speed);
        int Scan(IReadonlyRobot robot, int degrees, int resolution);
        int TeamCount(IReadonlyRobot robot);
        
        //
        int Rand(int limit);
    }
}
