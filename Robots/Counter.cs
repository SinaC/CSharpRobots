using SDK;

namespace Robots
{
    public class Counter : Robot
    {
        public override string Name { get { return "Counter"; } }

        public override void Main()
        {
            const int res = 1;
            int d = SDK.Damage;
            int angle = SDK.Rand(360);
            while (true)
            {
                int range;
                while ((range = SDK.Scan(angle, res)) > 0)
                {
                    if (range > 700)
                    {
                        SDK.Drive(angle, 50);
                        double tm = SDK.Time;
                        while (SDK.Time - tm < 2) 
                            ;
                        SDK.Drive(angle, 0);
                        if (d != SDK.Damage)
                        {
                            d = SDK.Damage;
                            R();
                        }
                        angle -= 3;
                    }
                    else
                    {
                        SDK.Cannon(angle, range);
                        while (SDK.Cannon(angle, range) == 0) 
                            ;
                        if (d != SDK.Damage)
                        {
                            d = SDK.Damage;
                            R();
                        }
                        angle -= 15;
                    }
                }
                if (d != SDK.Damage)
                {
                    d = SDK.Damage;
                    R();
                }
                angle += res;
                angle %= 360;
            }
        }

        int _lastDir;

        void R()
        {
            int x = SDK.LocX;
            int y = SDK.LocY;

            double tm = SDK.Time;
            if (_lastDir == 0)
            {
                if (y > 512)
                {
                    _lastDir = 1;
                    SDK.Drive(270, 100);
                    while (y - 100 < SDK.LocY && SDK.Time - tm < 2)
                        ;
                    SDK.Drive(270, 0);
                }
                else
                {
                    _lastDir = 1;
                    SDK.Drive(90, 100);
                    while (y + 100 > SDK.LocY && SDK.Time - tm < 2)
                        ;
                    SDK.Drive(90, 0);
                }
            }
            else
            {
                if (x > 512)
                {
                    _lastDir = 0;
                    SDK.Drive(180, 100);
                    while (x - 100 < SDK.LocX && SDK.Time - tm < 2)
                        ;
                    SDK.Drive(180, 0);
                }
                else
                {
                    _lastDir = 0;
                    SDK.Drive(0, 100);
                    while (x + 100 > SDK.LocX && SDK.Time - tm < 2)
                        ;
                    SDK.Drive(0, 0);
                }
            }
        }

    }
}
