using SDK;

namespace Robots
{
    // Rabbit runs around the field, randomly and never fires; use as a target
    public class Rabbit : Robot
    {
        private int _arenaSize;
        private int _destX;
        private int _destY;

        public override void Init()
        {
            _arenaSize = SDK.Parameters["ArenaSize"];

            GetRandomDestination();
        }

        public override void Step()
        {
            int angle = GetAngle();
            int distance = Distance(SDK.LocX, SDK.LocY, _destX, _destY);
            if (distance > 50) // far from destination, drive
                SDK.Drive(angle, 100);
            else
            {
                if (SDK.Speed == 0) // destination reached: change direction
                    GetRandomDestination();
                else // approaching destination, slowed down
                    SDK.Drive(angle, 0);
            }
        }

        private void GetRandomDestination()
        {
            _destX = SDK.Rand(_arenaSize);
            _destY = SDK.Rand(_arenaSize);
        }

        private int GetAngle()
        {
            int diffX = _destX - SDK.LocX;
            int diffY = _destY - SDK.LocY;

            return (int)(SDK.Rad2Deg(SDK.ATan2(diffY, diffX)));
        }

        private int Distance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            int d = SDK.Sqrt((x * x) + (y * y));
            return d;
        }
    }
}
