using SDK;

// TODO: 
//  when approching border/corner stop driving randomly and go opposite direction
//  when switching target, reset speed
namespace Robots
{
    public class SinaC : Robot
    {
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

        private int _arenaSize;
        private int _missileSpeed;
        private int _maxExplosionRange;
        private int _maxExplosionRangePlusCannonRange;

        public override void Init()
        {
            _previousTime = SDK.Time;
            _arenaSize = SDK.Parameters["ArenaSize"];
            _missileSpeed = SDK.Parameters["MissileSpeed"];
            _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
            _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];

            _teamCount = SDK.FriendsCount;
            _id = SDK.Id;

            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            //DriveRandomly();
            FireOnEnemy();
            _previousSuccessfulShootTime = SDK.Time;
        }

        public override void Step()
        {
            // Update shared info
            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            double currentTime = SDK.Time;

            double elapsedTime = currentTime - _previousTime;
            double elapsedShootingTime = currentTime - _previousSuccessfulShootTime;

            if (elapsedShootingTime > 1) // 1 second since last successfull shoot
            {
                // Fire on target
                bool success = FireOnEnemyInterpolated(elapsedShootingTime);

                //
                if (success)
                    _previousSuccessfulShootTime = SDK.Time;
            }

            if (elapsedTime > 1) // 1 second since last move
            {
                System.Diagnostics.Debug.WriteLine("LOCATION {0} : {1},{2}", _id, SDK.LocX, SDK.LocY);

                // Change direction
                //DriveRandomly();

                //
                _previousTime = currentTime;
            }
        }

        //***************************************************************
        // NEW VERSION
        //***************************************************************
        private void FireOnEnemy()
        {
            // Search nearest enemy
            int enemyAngle, enemyRange;
            double enemyX, enemyY;
            bool found = FindNearestEnemy(0, out enemyAngle, out enemyRange, out enemyX, out enemyY);

            if (found)
            {
                if (enemyRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
                    SDK.Cannon(enemyAngle, enemyRange);
            }
        }

        private bool FireOnEnemyInterpolated(double elapsedTime)
        {
            bool success = false;
            // Search nearest enemy
            int enemyAngle, enemyRange;
            double enemyX, enemyY;
            bool found = FindNearestEnemy(0, out enemyAngle, out enemyRange, out enemyX, out enemyY);
            
            if (found)
            {
                System.Diagnostics.Debug.WriteLine("Nearest enemy of {0}: A{1} R{2} {3:0.0000},{4:0.0000}", _id, enemyAngle, enemyRange, enemyX, enemyY);

                double currentSpeedX, currentSpeedY;
                ComputeSpeed(elapsedTime, _previousEnemyX, _previousEnemyY, enemyX, enemyY, out currentSpeedX, out currentSpeedY);

                //System.Diagnostics.Debug.WriteLine("TICK:{0:0.00} | Enemy position: {1:0.0000}, {2:0.0000} Speed : {3:0.0000}, {4:0.0000} | range {5} angle {6}", SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

                if (_previousRange != 0 && _previousAngle != 0) // Only fire when we have valid information
                {
                    int cannonAngle, cannonRange;
                    ComputeCannonInfo(SDK.LocX, SDK.LocY, enemyX, enemyY, currentSpeedX, currentSpeedY, out cannonAngle, out cannonRange);

                    if (cannonRange > _maxExplosionRange && cannonRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
                    {
                        success = SDK.Cannon(cannonAngle, cannonRange) != 0;
                    }
                }
                else // If no information on speed, fire at current enemy location
                {
                    if (enemyRange > _maxExplosionRange && enemyRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far or too near
                    {
                        success = SDK.Cannon(enemyAngle, enemyRange) != 0;
                    }
                }

                _previousAngle = enemyAngle;
                _previousRange = enemyRange;
                _previousEnemyX = enemyX;
                _previousEnemyY = enemyY;
                _previousSpeedX = currentSpeedX;
                _previousSpeedY = currentSpeedY;
            }

            //
            return success;
        }

        private bool FindNearestEnemy(int minDistance, out int angle, out int range, out double enemyX, out double enemyY)
        {
            enemyX = 0;
            enemyY = 0;
            // Scan
            int startAngle = 0;
            int endAngle = 360;
            int nearestDistance = 0;
            int nearestAngle = 0;
            for (int resAngle = 16; resAngle >= 1; resAngle /= 2)
            {
                bool found = false;
                nearestDistance = 2 * _arenaSize;
                for (angle = startAngle; angle <= endAngle; angle += resAngle)
                {
                    range = SDK.Scan(angle, resAngle);
                    if (range > minDistance + _maxExplosionRange && range < nearestDistance)
                    //if (range > 0)
                    {
                        nearestDistance = range;
                        nearestAngle = angle;
                        found = true;
                    }
                }
                if (!found)
                    break;
                startAngle = nearestAngle - resAngle;
                endAngle = startAngle + 2 * resAngle;
            }
            // Compute position
            range = nearestDistance;
            angle = nearestAngle;
            if (range > 0)
            {
                ComputePoint(SDK.LocX, SDK.LocY, range, angle, out enemyX, out enemyY);
                if (IsFriendlyTarget(enemyX, enemyY))
                    return FindNearestEnemy(range, out angle, out range, out enemyX, out enemyY);
                return true;
            }
            return false;
        }

        //***************************************************************
        // OLD VERSION
        //***************************************************************

        public void InitOld()
        {
            _previousTime = SDK.Time;
            _arenaSize = SDK.Parameters["ArenaSize"];
            _missileSpeed = SDK.Parameters["MissileSpeed"];
            _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
            _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];

            _teamCount = SDK.FriendsCount;
            _id = SDK.Id;

            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            //DriveRandomly();
            FireOnTarget();
            _previousSuccessfulShootTime = SDK.Time;
        }

        public void Step2Old()
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
                System.Diagnostics.Debug.WriteLine("LOCATION {0} : {1},{2}", _id, SDK.LocX, SDK.LocY);

                // Change direction
                //DriveRandomly();

                //
                _previousTime = currentTime;
            }
        }

        private void FireOnTarget()
        {
            int currentAngle, currentRange;
            bool targetFound = FindTarget(1, out currentAngle, out currentRange);
            if (targetFound)
            {
                if (currentRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
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

                    if (cannonRange > _maxExplosionRange && cannonRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
                    {
                        success = SDK.Cannon(cannonAngle, cannonRange) != 0;
                    }
                }
                else // If no information on speed, fire at current enemy location
                {
                    if (targetRange > _maxExplosionRange && targetRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far or too near
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
            //int enemyAngle, enemyRange;
            //double enemyX, enemyY;
            //bool found = FindNearestEnemy(0, out enemyAngle, out enemyRange, out enemyX, out enemyY);
            //System.Diagnostics.Debug.WriteLine("Nearest enemy of {0}: A{1} R{2} {3:0.0000},{4:0.0000} found:{5}", _id, enemyAngle, enemyRange, enemyX, enemyY, found);

            angle = 0;
            range = 0;
            for (int step = 0; step < 360; step += resolution)
            {
                int r = SDK.Scan(step, resolution);
                if (r > 0)
                {
                    double targetX, targetY;
                    ComputePoint(SDK.LocX, SDK.LocY, r, step, out targetX, out targetY);
                    if (!IsFriendlyTarget(targetX, targetY))
                    {
                        range = r;
                        angle = step;
                        //System.Diagnostics.Debug.WriteLine("Enemy of {0}: A{1} R{2} {3:0.0000},{4:0.0000}", _id, angle, range, targetX, targetY);

                        //if (SDK.Abs(range-enemyRange) > 5 && SDK.Abs(angle - enemyAngle) > 5)
                        //    System.Diagnostics.Debug.WriteLine("***********************************************************");
                        return true;
                    }
                }
            }
            return false;
        }

        // HELPERS

        private void UpdateSharedInformations(int locX, int locY, int damage)
        {
            TeamLocX[_id] = locX;
            TeamLocY[_id] = locY;
            TeamDamage[_id] = damage;
        }

        private bool IsFriendlyTarget(double locX, double locY)
        {
            if (_teamCount > 1)
            {
                //System.Diagnostics.Debug.WriteLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                for (int i = 0; i < _teamCount; i++)
                    if (i != _id)
                    {
                        //System.Diagnostics.Debug.WriteLine("TEAM MEMBER {0} : {1},{2}", i, TeamLocX[i], TeamLocY[i]);
                        double dx = locX - TeamLocX[i];
                        double dy = locY - TeamLocY[i];
                        if (dx * dx + dy * dy < FriendRangeSquared)
                            return true;
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
            // Say P (using vector notation) the unknown point in which the missile meets the enemy, R the starting location of your robot, T the starting location of the target and V its velocity. 
            // The target will reach the point P in t seconds, according to the formula
            // P = T + V t
            // (P - R)^2 = (300 t)^2
            // Solving these 2 equations:
            // t = ( sqrt(300^2 D^2 - (DxV)^2) + D(dot)V ) / (300^2 - V^2)  with D = T-R
            // t = ( sqrt(300^2 (Dx^2 + Dy^2) - (DxVy - DyVx)^2) + (DxVx + DyVy) ) / (300^2 - (Vx^2 + Vy^2) )
            double dX = enemyX - robotX;
            double dY = enemyY - robotY;
            double t = SDK.Sqrt(_missileSpeed * _missileSpeed * (dX * dX + dY * dY) - (dX * speedY - dY * speedX) * (dX * speedY - dY * speedX) + 0.5) / (_missileSpeed * _missileSpeed - (speedX * speedX + speedY * speedY));
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
