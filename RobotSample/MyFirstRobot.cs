namespace RobotSample
{
    public class MyFirstRobot : SDK.Robot
    {
        public const int BorderSize = 30;

        private int _arenaSize;
        private double _lastDirectionChange;
        private double _lastShotTime;

        public override void Init()
        {
            SDK.LogLine("I'm alive ... {0}", SDK.Id);

            _arenaSize = SDK.Parameters["ArenaSize"];

            _lastDirectionChange = double.MinValue;
            _lastShotTime = double.MinValue;
        }

        public override void Step()
        {
            SDK.LogLine("STEP: {0} My position x:{1} y:{2}", SDK.Time, SDK.LocX, SDK.LocY);

            // Find Target
            int targetAngle, targetRange;
            bool targetFound = FindTarget(1, out targetAngle, out targetRange);

            // Fire (if more than one second elapsed)
            if (targetFound && SDK.Time - _lastShotTime >= 1)
            {
                SDK.Cannon(targetAngle, targetRange);
                _lastShotTime = SDK.Time;
            }

            // Move randomly, changing direction every 2 seconds
            if (SDK.Time - _lastDirectionChange >= 2)
            {
                MoveRandomly();

                _lastDirectionChange = SDK.Time;
            }
        }

        private bool FindTarget(int resolution, out int angle, out int range)
        {
            angle = 0;
            range = 0;
            for (int step = 0; step < 360; step += resolution)
            {
                int r = SDK.Scan(step, resolution);
                if (r > 0)
                {
                    range = r;
                    angle = step + resolution / 2;
                    return true;
                }
            }
            return false;
        }

        private void MoveRandomly()
        {
            // Only when far from borders
            if (SDK.LocX >= BorderSize && SDK.LocX <= _arenaSize - BorderSize && SDK.LocY >= BorderSize && SDK.LocY <= _arenaSize - BorderSize)
            {
                int driveAngle = SDK.Rand(360);
                SDK.Drive(driveAngle, 50);
            }
        }
    }
}
