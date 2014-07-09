namespace Robots
{
    //JJRobots (c) 2000 L.Boselli - boselli@uno.it
    public class Stinger : SDK.Robot
    {
        private static int counter;

        private static int[] locX = new int[8];
        private static int[] locY = new int[8];

        private static int driveAngle = 5;

        private double oldTargetX;
        private double oldTargetY;
        private double targetX;
        private double targetY;
        private double speedX;
        private double speedY;
        private double lastTime;
        private int range;
        private int scan;
        private int drive;
        private int id;

        public override void Init()
        {
            if ((id = SDK.Id) == 0)
            {
                counter = 1;
            }
            else
            {
                counter = id + 1;
            }
            targetX = targetY = -1000;
            speedX = speedY = 0;
            lastTime = 0;
            SDK.Drive(drive = SDK.Rand(360), 100);
        }

        public override void Step()
        {
            // NOP
            if (scan - driveAngle < drive && drive < scan + driveAngle)
            {
                if (findNearestEnemy(0)) 
                    shoot();
            }
            else
                stopAndGo();
        }


        /*
        private void OriginalVersion()
        {
            if ((id = SDK.Id) == 0)
            {
                counter = 1;
            }
            else
            {
                counter = id + 1;
            }
            targetX = targetY = -1000;
            speedX = speedY = 0;
            lastTime = 0;
            SDK.Drive(drive = SDK.Rand(360), 100);
            while (true)
            {
                do
                {
                    if (findNearestEnemy(0)) shoot();
                } while (scan - driveAngle < drive && drive < scan + driveAngle);
                stopAndGo();
            }
        }
        */

        private bool findNearestEnemy(int minDistance)
        {
            int startAngle = 0;
            int endAngle = 360;
            int nearestAngle = 0;
            int nearestDistance = 0;
            for (int resAngle = 16; resAngle >= 1; resAngle /= 2)
            {
                nearestDistance = 2000;
                for (
                    scan = startAngle;
                    scan <= endAngle;
                    scan += resAngle
                    )
                {
                    range = SDK.Scan(scan, resAngle);
                    if (range > minDistance + 40 && range < nearestDistance)
                    {
                        nearestDistance = range;
                        nearestAngle = scan;
                    }
                }
                startAngle = nearestAngle - resAngle;
                endAngle = startAngle + 2*resAngle;
            }
            range = nearestDistance;
            scan = nearestAngle;
            if (range > 0)
            {
                double time;
                double deltaT = (time = SDK.Time) - lastTime;
                targetX = (locX[id] = SDK.LocX) + range*SDK.Cos(scan)/100000.0;
                targetY = (locY[id] = SDK.LocY) + range*SDK.Sin(scan)/100000.0;
                if (isTargetAFriend()) return findNearestEnemy(range);
                if (deltaT > 0.5)
                {
                    double theSpeedX = (targetX - oldTargetX)/deltaT;
                    double theSpeedY = (targetY - oldTargetY)/deltaT;
                    oldTargetX = targetX;
                    oldTargetY = targetY;
                    double speed2 = theSpeedX*theSpeedX + theSpeedY*theSpeedY;
                    if (speed2 > 0)
                    {
                        if (speed2 < 1600)
                        {
                            speedX = theSpeedX;
                            speedY = theSpeedY;
                        }
                        else
                        {
                            speedX = speedY = 0;
                        }
                    }
                    lastTime = time;
                }
                return true;
            }
            return false;
        }

        private bool isTargetAFriend()
        {
            if (counter > 1)
            {
                for (int ct = 0; ct < counter; ct++)
                {
                    if (ct != id)
                    {
                        int dx = (int) (targetX - locX[ct]);
                        int dy = (int) (targetY - locY[ct]);
                        if (dx*dx + dy*dy < 6400) return true;
                    }
                }
            }
            return false;
        }

        private void stopAndGo()
        {
            SDK.Drive(drive = scan, 49);
            if (SDK.Speed >= 0)
            {
                findNearestEnemy(0);
                shoot();
                return;
            }
            int dx = (int)(targetX - (locX[id] = SDK.LocX));
            int dy = (int)(targetY - (locY[id] = SDK.LocY));
            if (dx == 0)
            {
                drive = dy > 0 ? 90 : 270;
            }
            else
            {
                drive = SDK.ATan(dy * 100000 / dx);
                if (dx < 0) drive += 180;
            }
            SDK.Drive(drive, 100);
        }

        //private void stopAndGo()
        //{
        //    SDK.Drive(drive = scan, 49);
        //    while (SDK.Speed >= 50)
        //    {
        //        findNearestEnemy(0);
        //        shoot();
        //    }
        //    int dx = (int) (targetX - (locX[id] = SDK.LocX));
        //    int dy = (int) (targetY - (locY[id] = SDK.LocY));
        //    if (dx == 0)
        //    {
        //        drive = dy > 0 ? 90 : 270;
        //    }
        //    else
        //    {
        //        drive = SDK.ATan(dy*100000/dx);
        //        if (dx < 0) drive += 180;
        //    }
        //    SDK.Drive(drive, 100);
        //}

        private void shoot()
        {
            if (range > 50 && range <= 800)
            {
                fireToTarget(targetX, targetY, speedX, speedY, lastTime);
            }
            else if (range > 0 && range <= 50)
            {
                SDK.Cannon(scan, 45);
            }
        }

        private void fireToTarget(double x, double y, double sx, double sy, double t)
        {
            double Dx, Dy;
            double deltaT = SDK.Time - t;
            if (deltaT > 0)
            {
                x += sx*deltaT;
                y += sy*deltaT;
                Dx = x - (locX[id] = SDK.LocX);
                Dy = y - (locY[id] = SDK.LocY);
            }
            else
            {
                Dx = x - (locX[id] = SDK.LocX);
                Dy = y - (locY[id] = SDK.LocY);
            }
            double dxsymdysx = Dx*sy - Dy*sx;
            double tp =
                (SDK.Sqrt((Dx*Dx + Dy*Dy)*90000 - dxsymdysx*dxsymdysx) + Dx*sx + Dy*sy)/
                (90000 - sx*sx - sy*sy)
                ;
            double rx = Dx + sx*tp;
            double ry = Dy + sy*tp;
            double r2 = rx*rx + ry*ry;
            if (r2 > 1600 && r2 < 547600)
            {
                double angle;
                if (rx == 0)
                {
                    angle = ry > 0 ? 1.5708 : 4.7124;
                }
                else
                {
                    angle = SDK.ATan(ry/rx);
                    if (rx < 0) angle += 3.1416;
                }
                int degrees = (int) (angle*180/3.1416);
                SDK.Cannon(degrees, (int) (SDK.Sqrt(r2) + 0.5));
            }
        }
    }
}
