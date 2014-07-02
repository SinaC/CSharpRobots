using SDK;

namespace Robots
{
    public class Sniper : Robot
    {
        int _corner;
        int _c1X, _c1Y;
        int _c2X, _c2Y;
        int _c3X, _c3Y;
        int _c4X, _c4Y;
        int _s1, _s2, _s3, _s4;
        int _sc;
        int _d;

        public override void Main()
        {
            _c1X = 10; _c1Y = 10; _s1 = 0;
            _c2X = 10; _c2Y = 990; _s2 = 270;
            _c3X = 990; _c3Y = 990; _s3 = 180;
            _c4X = 990; _c4Y = 10; _s4 = 90;
            int closest = 9999;
            new_corner();
            _d = SDK.Damage;
            int dir = _sc;

            while (true)
            {
                while (dir < _sc + 90)
                {
                    int range = SDK.Scan(dir, 1);
                    if (range <= 700 && range > 0)
                    {
                        while (range > 0)
                        {
                            closest = range;
                            SDK.Cannon(dir, range);
                            range = SDK.Scan(dir, 1);
                            if (_d + 15 > SDK.Damage)
                                range = 0;
                        }
                        dir -= 10;
                    }

                    dir += 1;
                    if (_d != SDK.Damage)
                    {
                        new_corner();
                        _d = SDK.Damage;
                        dir = _sc;
                    }
                }

                if (closest == 9999)
                {
                    new_corner();
                    _d = SDK.Damage;
                    dir = _sc;
                }
                else
                    dir = _sc;
                closest = 9999;
            }
        }

        void new_corner()
        {
            int x = 0, y = 0;
            int new_ = SDK.Rand(4);
            if (new_ == _corner)
                _corner = (new_ + 1) % 4;
            else
                _corner = new_;
            if (_corner == 0)
            {
                x = _c1X;
                y = _c1Y;
                _sc = _s1;
            }
            if (_corner == 1)
            {
                x = _c2X;
                y = _c2Y;
                _sc = _s2;
            }
            if (_corner == 2)
            {
                x = _c3X;
                y = _c3Y;
                _sc = _s3;
            }
            if (_corner == 3)
            {
                x = _c4X;
                y = _c4Y;
                _sc = _s4;
            }

            int angle = plot_course(x, y);

            SDK.Drive(angle, 100);

            while (Distance(SDK.LocX, SDK.LocY, x, y) > 100 && SDK.Speed > 0)
                ;

            SDK.Drive(angle, 10);
            while (Distance(SDK.LocX, SDK.LocY, x, y) > 20 && SDK.Speed > 0)
                ;

            SDK.Drive(angle, 0);

        }

        int Distance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            _d = SDK.Sqrt((x * x) + (y * y));
            return _d;
        }

        int plot_course(int xx, int yy)
        {
            int d;
            const int scale = 100000;
            int curx = SDK.LocX;
            int cury = SDK.LocY;
            int x = curx - xx;
            int y = cury - yy;

            if (x == 0)
            {
                d = yy > cury ? 90 : 270;
            }
            else
            {
                if (yy < cury)
                {
                    if (xx > curx)
                        d = 360 + SDK.ATan((scale * y) / x);
                    else
                        d = 180 + SDK.ATan((scale * y) / x);
                }
                else
                {
                    if (xx > curx)
                        d = SDK.ATan((scale * y) / x);
                    else
                        d = 180 + SDK.ATan((scale * y) / x);
                }
            }

            return (d);
        }

    }
}
