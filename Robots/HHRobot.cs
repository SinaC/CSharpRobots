namespace Robots
{
    public class HHRobot : SDK.Robot
    {
        private const bool IsInFollowMode = true;

        private int _teta;
        private int _x;
        private int _y;

        public override void Init()
        {
            _teta = 180;
            _x = SDK.LocX;
            _y = SDK.LocY;
        }

        public override void Step()
        {
            if (IsInFollowMode)
                FollowStep();
            else
                AvoidStep();
        }

        private void FollowStep()
        {
            int angle, range;
            bool targetFound = FindTarget(1, out angle, out range);
            if (targetFound)
            {
                SDK.Drive(angle, 40);
                SDK.Cannon(angle, range);
            }
            else
            {
                SDK.Drive(0, 40);
            }
        }

        private void AvoidStep()
        {
            int xm = _x;
            int ym = _y;
            _x = SDK.LocX;
            _y = SDK.LocY;

            int angle, range;
            bool targetFound = FindTarget(1, out angle, out range);
            if (targetFound)
            {
                SDK.Drive(angle + _teta, 40);
                SDK.Cannon(angle, range);
                //System.Diagnostics.Debug.WriteLine(" --------------      Angle = {0}            --------------", angle + _teta);
                if (_x == xm)
                {
                    if (_x < 10 || _x > 990)
                    {
                        _teta = _teta + 45;
                        SDK.Drive(_teta, 5);

                        if (SDK.LocX == 0)
                            _teta = _teta + 45;

                        SDK.Drive(_teta, 100);
                        SDK.Drive(_teta, 100);
                    }
                }

                if (_y == ym)
                {
                    if (_y < 10 || _y > 990)
                    {
                        _teta = _teta + 45;
                        SDK.Drive(_teta, 5);

                        if (SDK.LocY == 0)
                            _teta = _teta + 45;

                        SDK.Drive(_teta, 100);
                        SDK.Drive(_teta, 100);
                    }
                }
            }
            else
            {
                SDK.Drive(0, 40);
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

    }
}
