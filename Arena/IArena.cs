using Common.Clock;

namespace Arena
{
    public interface IArena
    {
        int Cannon(InternalRobot robot, Tick launchTick, int degrees, int range);
        int Drive(InternalRobot robot, int degrees, int speed);
        int Scan(InternalRobot robot, int degrees, int resolution);
        int TeamCount(InternalRobot robot);
    }
}
