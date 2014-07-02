using SDK;

namespace Robots
{
    // Rabbit runs around the field, SDK.Randomly and never fires; use as a target
    public class Rabbit : Robot
    {
        public override void Main()
        {
            while (true)
            {
                Go(SDK.Rand(1000), SDK.Rand(1000));
            }
        }

        private void Go(int destX, int destY)
        {
            int course = PlotCourse(destX, destY);
            SDK.Drive(course, 50);
            while (Distance(SDK.LocX, SDK.LocY, destX, destY) > 50) 
                ;
            SDK.Drive(course, 0);
            while (SDK.Speed > 0) 
                ;
        }

        private int Distance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            int d = SDK.Sqrt((x * x) + (y * y));
            return (d);
        }

        private int PlotCourse(int xx, int yy)
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
            return d;
        }
    }
}
