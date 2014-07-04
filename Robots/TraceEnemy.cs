using SDK;

namespace Robots
{
    public class TraceEnemy : Robot
    {
        public override void Main()
        {
            MeasureEnemySpeed();
        }

        private void MeasureEnemySpeed()
        {
            double previousTime = SDK.Time;
            int previousAngle = 0;
            int previousRange = 0;
            double previousEnemyX = 0;
            double previousEnemyY = 0;
            double previousSpeedX = 0;
            double previousSpeedY = 0;

            while (true)
            {
                double currentTime = SDK.Time;

                double diffTime = currentTime - previousTime;
                if (diffTime > 1)
                {
                    int currentAngle, currentRange;
                    bool targetFound = FindTarget(1, out currentAngle, out currentRange);
                    if (targetFound)
                    {
                        double currentEnemyX, currentEnemyY;
                        ComputePoint(SDK.LocX, SDK.LocY, currentRange, currentAngle, out currentEnemyX, out currentEnemyY);

                        double currentSpeedX, currentSpeedY;
                        ComputeSpeed(diffTime, previousEnemyX, previousEnemyY, currentEnemyX, currentEnemyY, out currentSpeedX, out currentSpeedY);

                        System.Diagnostics.Debug.WriteLine("TICK:{0:0.00} | Enemy position: {1:0.0000}, {2:0.0000} Speed : {3:0.0000}, {4:0.0000} | range {5} angle {6}", SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

                        int cannonAngle, cannonRange;
                        ComputeCannonInfo(SDK.LocX, SDK.LocY, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, out cannonAngle, out cannonRange);

                        previousTime = currentTime;
                        previousAngle = currentAngle;
                        previousRange = currentRange;
                        previousEnemyX = currentEnemyX;
                        previousEnemyY = currentEnemyY;
                        previousSpeedX = currentSpeedX;
                        previousSpeedY = currentSpeedY;

                    }
                }
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
                    angle = step;
                    return true;
                }
            }
            return false;
        }

        private void ComputeSpeed(double elapsed, double previousX, double previousY, double currentX, double currentY, out double speedX, out double speedY)
        {
            speedX = (currentX - previousX) / elapsed;
            speedY = (currentY - previousY) / elapsed;
        }

        private void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = SDK.Deg2Rad(degrees);
            x = centerX + distance * SDK.Cos(radians);
            y = centerY + distance * SDK.Sin(radians);
        }

        private void ComputeCannonInfo(double robotX, double robotY, double enemyX, double enemyY, double speedX, double speedY, out int angle, out int range)
        {
            //http://jrobots.sourceforge.net/jjr_tutorials.shtml
            //Say P (using vector notation) the unknown point in which the missile meets the enemy, R the starting location of your robot, T the starting location of the target and V its velocity. 
            // The target will reach the point P in t seconds, according to the formula
            // P = T + V t
            // (P - R)^2 = (300 t)^2
            // t = ( sqrt(300^2 D^2 - (DxV)^2) + D(dot)V ) / (300^2 - V^2)  with D = T-R
            // t = ( sqrt(3002 (Dx2 + Dy2) - (DxVy - DyVx)2) + (DxVx + DyVy) ) / (3002 - (Vx2 + Vy2) )
            double dX = enemyX - robotX;
            double dY = enemyY - robotY;
            //sqrt(3002 (Dx2 + Dy2) - (DxVy - DyVx)2) + (DxVx + DyVy) ) / (3002 - (Vx2 + Vy2) )
            double t = SDK.Sqrt(300*300*(dX*dX + dY*dY) - (dX*speedY - dY*speedX)*(dX*speedY - dY*speedX))/(300*300 - (speedX*speedX + speedY*speedY));
            double pX = enemyX + speedX*t;
            double pY = enemyY + speedY*t;


            System.Diagnostics.Debug.WriteLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}", t, SDK.Time + t, pX, pY);
            angle = 0;
            range = 0;
        }
    }
}
