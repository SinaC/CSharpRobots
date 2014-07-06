using SDK;

// TODO: 
//  when approching border/corner stop driving randomly and go opposite direction
//  use IsFriendlyTarget to avoid shooting to friend and get a new target --> new FindTarget method

namespace Robots
{
    public class SinaC : Robot
    {
        public override string Name { get { return "SinaC"; } }

        public static readonly double FriendRangeSquared = 80*80;

        // Shared infos between team members
        private static readonly int[] TeamLocX = new int[8];
        private static readonly int[] TeamLocY = new int[8];
        private static readonly int[] TeamDamage = new int[8];

        private int _teamCount;
        private int _id; // own id

        double _previousSuccessfulShootTime;
        double _previousTime;
        int _previousAngle;
        int _previousRange;
        double _previousEnemyX;
        double _previousEnemyY;
        double _previousSpeedX;
        double _previousSpeedY;

        public override void Main()
        {
            _previousTime = SDK.Time;

            _teamCount = SDK.FriendsCount;
            _id = SDK.Id;

            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            DriveRandomly();
            FireOnTarget();
            _previousSuccessfulShootTime = SDK.Time;

            while (true)
            {
                // Update shared info
                UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

                double currentTime = SDK.Time;

                double elapsedTime = currentTime - _previousTime;
                double elapsedShootingTime = currentTime - _previousSuccessfulShootTime;

                if (elapsedShootingTime > 1) // 1 second since last successfull shoot
                {
                    // Fire on target
                    bool success = FireOnTargetInterpolated(elapsedShootingTime);

                    //
                    if (success)
                        _previousSuccessfulShootTime = SDK.Time;
                }

                if (elapsedTime > 1) // 1 second since last move
                {
                    // Change direction
                    DriveRandomly();

                    //
                    _previousTime = currentTime;
                }
            }
        }

        private void UpdateSharedInformations(int locX, int locY, int damage)
        {
            TeamLocX[_id] = locX;
            TeamLocY[_id] = locY;
            TeamDamage[_id] = damage;
        }

        private bool IsFriendlyTarget(double locX, double locY)
        {
            if (_teamCount > 1)
                for(int i = 0; i < _teamCount; i++)
                    if (i != _id)
                    {
                        double dx = locX - TeamLocX[i];
                        double dy = locY - TeamLocY[i];
                        if (dx*dx + dy*dy < FriendRangeSquared)
                            return true;
                    }
            return false;
        }

        private void FireOnTarget()
        {
            int currentAngle, currentRange;
            bool targetFound = FindTarget(1, out currentAngle, out currentRange);
            if (targetFound)
            {
                if (currentRange < 750) // Don't fire if too far
                    SDK.Cannon(currentAngle, currentRange);
            }
        }

        private bool FireOnTargetInterpolated(double elapsedTime)
        {
            bool success = false;
            // Fire on target
            int targetAngle, targetRange;
            bool targetFound = FindTarget(1, out targetAngle, out targetRange);
            if (targetFound)
            {
                double currentEnemyX, currentEnemyY;
                ComputePoint(SDK.LocX, SDK.LocY, targetRange, targetAngle, out currentEnemyX, out currentEnemyY);

                double currentSpeedX, currentSpeedY;
                ComputeSpeed(elapsedTime, _previousEnemyX, _previousEnemyY, currentEnemyX, currentEnemyY, out currentSpeedX, out currentSpeedY);

                //System.Diagnostics.Debug.WriteLine("TICK:{0:0.00} | Enemy position: {1:0.0000}, {2:0.0000} Speed : {3:0.0000}, {4:0.0000} | range {5} angle {6}", SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

                if (_previousRange != 0 && _previousAngle != 0) // Only fire when we have valid information
                {
                    int cannonAngle, cannonRange;
                    ComputeCannonInfo(SDK.LocX, SDK.LocY, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, out cannonAngle, out cannonRange);

                    if (cannonRange > 40 && cannonRange < 750) // Don't fire if too far
                    {
                        success = SDK.Cannon(cannonAngle, cannonRange) != 0;
                    }
                }
                else // If no information on speed, fire at current location
                {
                    if (targetRange > 40 && targetRange < 750) // Don't fire if too far or too near
                    {
                        success = SDK.Cannon(targetAngle, targetRange) != 0;
                    }
                }

                _previousAngle = targetAngle;
                _previousRange = targetRange;
                _previousEnemyX = currentEnemyX;
                _previousEnemyY = currentEnemyY;
                _previousSpeedX = currentSpeedX;
                _previousSpeedY = currentSpeedY;
            }
            return success;
        }

        private void DriveRandomly()
        {
            int driveAngle = SDK.Rand(360);
            SDK.Drive(driveAngle, 50);
        }

        // TODO: store previous angle
        private bool FindTarget(int resolution, out int angle, out int range)
        {
            angle = 0;
            range = 0;
            for (int step = 0; step < 360; step += resolution)
            {
                int r = SDK.Scan(step, resolution);
                if (r > 0)
                {
                    double targetX, targetY;
                    ComputePoint(SDK.LocX, SDK.LocY, range, step, out targetX, out targetY);
                    if (!IsFriendlyTarget(targetX, targetY))
                    {
                        range = r;
                        angle = step;
                        return true;
                    }
                }
            }
            return false;
        }

        private void ComputeSpeed(double elapsed, double previousX, double previousY, double currentX, double currentY, out double speedX, out double speedY)
        {
            speedX = (currentX - previousX)/elapsed;
            speedY = (currentY - previousY)/elapsed;
        }

        private void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = SDK.Deg2Rad(degrees);
            x = centerX + distance*SDK.Cos(radians);
            y = centerY + distance*SDK.Sin(radians);
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
            double t = SDK.Sqrt(300*300*(dX*dX + dY*dY) - (dX*speedY - dY*speedX)*(dX*speedY - dY*speedX) + 0.5)/(300*300 - (speedX*speedX + speedY*speedY));
            double pX = enemyX + speedX*t;
            double pY = enemyY + speedY*t;

            double diffX = pX - robotX;
            double diffY = pY - robotY;

            Vector v = new Vector(diffX, diffY);
            angle = (int) SDK.Rad2Deg(v.A);
            range = (int) v.R;

            //System.Diagnostics.Debug.WriteLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
        }
    }
}
