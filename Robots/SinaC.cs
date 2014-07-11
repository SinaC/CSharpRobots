﻿using System;
using System.Runtime.Remoting.Messaging;
using SDK;

// TODO: 
//  when switching enemy, reset enemy speed and no distance limit
//  smart movement strategy
//      when only one target, stay at 700 from target
//      otherwise, move randomly
//  share target infos, store every enemy around
//      if multiple friend can shoot the same enemy, shoot this enemy first

namespace Robots
{
    // Fire on target, using linear interpolation to predict target position
    // Move randomly, change direction every 2 seconds or when hit
    // No specific behavior on double or team match except avoiding friendly fire
    public class SinaC : Robot
    {
        public const bool Move = true;
        public const bool UpgradedPrecision = true;
        public const double Epsilon = 0.00001;

        public const int BorderSize = 30;
        public const double FriendRangeSquared = 80*80;

        // Shared infos between team members
        private static readonly int[] TeamLocX = new int[8];
        private static readonly int[] TeamLocY = new int[8];
        private static readonly int[] TeamDamage = new int[8];

        private int _teamCount;
        private int _id; // own id

        // Computed with FindNearestEnemy
        private double _currentEnemyX;
        private double _currentEnemyY;
        private double _currentEnemyAngle;
        private double _currentEnemyRange;

        // Updated by FireOnEnemy and FireOnEnemyInterpolated
        private double _previousSuccessfulShootTime;
        private double _previousTime;
        private double _previousEnemyAngle;
        private double _previousEnemyRange;
        private double _previousEnemyX;
        private double _previousEnemyY;
        private double _previousEnemySpeedX; // TODO: could be use to determine acceleration and get next speed estimation
        private double _previousEnemySpeedY;
        private int _previousDamage;

        // Updated by MoveSmartly
        private bool _noMinDistanceLimit; // TODO: reset when switching target
        private double _lastDrunkTurnTime;
        private double _goToAngle;
        private double _lastHitTime;
        private double _estimatedEnemyShotTime;


        private int _arenaSize;
        private int _maxDamage;
        private int _missileSpeed;
        private int _maxExplosionRange;
        private int _maxExplosionRangePlusCannonRange;
        private int _maxCannonRange;

        public override void Init()
        {
            SDK.Log("{0:HH:mm:ss.fff} - SINAC - Init", DateTime.Now);

            _previousTime = SDK.Time;
            _arenaSize = SDK.Parameters["ArenaSize"];
            _maxDamage = SDK.Parameters["MaxDamage"];
            _missileSpeed = SDK.Parameters["MissileSpeed"];
            _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
            _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];
            _maxCannonRange = SDK.Parameters["MaxCannonRange"];

            _teamCount = SDK.FriendsCount;
            _id = SDK.Id;
            _previousDamage = SDK.Damage;
            _noMinDistanceLimit = false;

            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            if (Move)
                //MoveRandomly();
                MoveSmartly();
            bool found = FindNearestEnemy(_maxExplosionRange);
            if (found)
            {
                bool success = FireOnEnemy();
                if (success)
                    _previousSuccessfulShootTime = SDK.Time;
            }
        }

        public override void Step()
        {
            // Update shared info
            UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

            double currentTime = SDK.Time;

            double elapsedTime = currentTime - _previousTime;
            double elapsedShootingTime = currentTime - _previousSuccessfulShootTime;

            if (elapsedShootingTime >= 1) // 1 second since last successfull shoot
            {
                bool found = FindNearestEnemy(_maxExplosionRange);
                if (found)
                {
                    // Fire on target
                    bool success = FireOnEnemyInterpolated(elapsedShootingTime);
                    //bool success = FireOnEnemy();

                    //
                    if (success)
                        _previousSuccessfulShootTime = SDK.Time;
                }
            }

            // Estimate enemy shot time using previous distance between enemy and I, supposing my target is the shooter
            int currentDamage = SDK.Damage;
            if (currentDamage != _previousDamage)
            {
                _lastHitTime = currentTime;
                _estimatedEnemyShotTime = _lastHitTime - Distance(SDK.LocX, SDK.LocY, _previousEnemyX, _previousEnemyY) / _missileSpeed;
                _previousDamage = currentDamage;
                SDK.Log("Damage detected: hit time {0} estimated shoot time {1}", _lastHitTime, _estimatedEnemyShotTime);
            }

            if (Move)
                MoveSmartly();

            //if (Move)
            //    AvoidBorders();

            //if (elapsedTime > 2 ) // 2 seconds since last move
            //{
            //    //SDK.Log("{0:HH:mm:ss.fff} - SINAC - MOVE RANDOMLY FROM LOCATION {1} : {2},{3}", DateTime.Now, _id, SDK.LocX, SDK.LocY);

            //    // Change direction
            //    if (Move)
            //        MoveRandomly();

            //    //
            //    _previousTime = currentTime;
            //}
        }

        private bool FireOnEnemy()
        {
            if (_currentEnemyAngle < _maxExplosionRangePlusCannonRange) // Don't fire if too far
            {
                SDK.Cannon((int) _currentEnemyAngle, (int) _currentEnemyRange);

                _previousEnemyAngle = _currentEnemyAngle;
                _previousEnemyRange = _currentEnemyRange;
                _previousEnemyX = _currentEnemyX;
                _previousEnemyY = _currentEnemyY;

                return true;
            }
            return false;
        }

        private bool FireOnEnemyInterpolated(double elapsedTime)
        {
            bool success = false;
            SDK.Log("{0:0.00} - SINAC - Nearest enemy of {1}: A:{2:0.0000} R:{3:0.0000} {4:0.0000},{5:0.0000}", SDK.Time, _id, _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

            double currentSpeedX, currentSpeedY;
            DifferenceRelativeToTime(elapsedTime, _previousEnemyX, _previousEnemyY, _currentEnemyX, _currentEnemyY, out currentSpeedX, out currentSpeedY);

            double currentAccelerationX, currentAccelerationY;
            DifferenceRelativeToTime(elapsedTime, _previousEnemySpeedX, _previousEnemySpeedY, currentSpeedX, currentSpeedY, out currentAccelerationX, out currentAccelerationY);

            //SDK.Log("{0:0.00} - SINAC - TICK:{1:0.00} | Enemy position: {2:0.0000}, {3:0.0000} Speed : {4:0.0000}, {5:0.0000} | range {6} angle {7}", DateTime.Now, SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);
            SDK.Log("{0:0.00} - SINAC - estimated speed {1:0.0000} {2:0.0000} acceleration {3:0.0000} {4:0.0000}", SDK.Time, currentSpeedX, currentSpeedY, currentAccelerationX, currentAccelerationY);

            int cannonAngle, cannonRange;
            ComputeCannonInfo(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY, currentSpeedX, currentSpeedY, out cannonAngle, out cannonRange);

            if (cannonRange > _maxExplosionRange && cannonRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
                success = SDK.Cannon(cannonAngle, cannonRange) != 0;

            _previousEnemyAngle = _currentEnemyAngle;
            _previousEnemyRange = _currentEnemyRange;
            _previousEnemyX = _currentEnemyX;
            _previousEnemyY = _currentEnemyY;
            _previousEnemySpeedX = currentSpeedX;
            _previousEnemySpeedY = currentSpeedY;

            //
            return success;
        }

        // Scan all around
        //  if target found,
        //      if target is within correct range (minDistance and previous best range),
        //          if target also found with rescan with same angle minus 1 with a double resolution,
        //              angle -= 0.125
        //              range = weighted average of both range (weight 1/8 for rescan and 7/8 for initial scan)
        //          else if target also found with rescan with same angle plus 1 with a double resolution,
        //              angle += 0.125
        //              range = weighted average of both range (weight 1/8 for rescan and 7/8 for initial scan)
        //          compute target position
        //          if no friend on that position, 
        //              new enemy
        private bool FindNearestEnemy(int minDistance)
        {
            bool found = false;
            _currentEnemyAngle = 0;
            _currentEnemyRange = _arenaSize;
            _currentEnemyX = 0;
            _currentEnemyY = 0;
            for (int a = 0; a < 360; a++)
            {
                int r = SDK.Scan(a, 1);

                // Try to get more precision by scanning 1 degree before and 1 degree after with double resolution and weight 1/8
                double preciseRange = r;
                double preciseAngle = a;
                if (UpgradedPrecision)
                {
                    if (r > 0)
                    {
                        int rBefore = SDK.Scan(a - 1, 2);
                        int rAfter = SDK.Scan(a + 1, 2);
                        if (rBefore > 0)
                        {
                            preciseRange = (7.0*r + rBefore)/8.0;
                            preciseAngle = a - 0.125;
                        }
                        else if (rAfter > 0)
                        {
                            preciseRange = (7.0*r + rAfter)/8.0;
                            preciseAngle = a + 0.125;
                        }
                    }
                }

                // Target must be in range
                if ((_noMinDistanceLimit  || r > minDistance) && r < _currentEnemyRange)
                {
                    //double rawEnemyX, rawEnemyY;
                    //ComputePoint(SDK.LocX, SDK.LocY, r, a, out rawEnemyX, out rawEnemyY);

                    double enemyX, enemyY;
                    ComputePoint(SDK.LocX, SDK.LocY, preciseRange, preciseAngle, out enemyX, out enemyY);
                    if (!IsFriendlyTarget(enemyX, enemyY))
                    {
                        //double cheatAngle, cheatRange, cheatX, cheatY;
                        //Cheat.CHEAT_FindNearestEnemy(out cheatAngle, out cheatRange, out cheatX, out cheatY);

                        _currentEnemyAngle = preciseAngle;
                        _currentEnemyRange = preciseRange;
                        _currentEnemyX = enemyX;
                        _currentEnemyY = enemyY;

                        //SDK.Log(" CHEAT A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", cheatAngle, cheatRange, cheatX, cheatY);
                        //SDK.Log("NORMAL A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}  RAW A:{4:0.0000} R:{5:0.0000} X:{6:0.0000} Y:{7:0.0000}", angle, range, enemyX, enemyY, a, r, rawEnemyX, rawEnemyY);
                        SDK.Log("NORMAL A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

                        found = true;
                    }
                }
            }
            return found;
        }

        private void AvoidBorders()
        {
            if (SDK.LocX < BorderSize)
                SDK.Drive(0, 50);
            else if (SDK.LocX >= _arenaSize - BorderSize)
                SDK.Drive(180, 50);
            else if (SDK.LocY < BorderSize)
                SDK.Drive(90, 50);
            else if (SDK.LocY >= _arenaSize - BorderSize)
                SDK.Drive(270, 50);
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

        // Go to center if speed is 0
        // If single match and target has taken more damage than I, drive full speed on target and fire without range restriction (aka suicide)
        // Else if distance to target is almost max cannon range, lurk around target and fire
        // Else move pseudo randomly
        private void MoveSmartly()
        {
            // Go to center if Speed is 0 at full speed
            if (SDK.Speed == 0)
                GoToCenter();
            //else if (SDK.FriendsCount == 1) and targetDamage > own damage + 40  ==> drive to target full speed and fire without range restriction (aka suicide)
            else
            {
                // TODO
                //double distanceToTarget2 = DistanceSquared(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY);
                //if (distanceToTarget2 > _maxCannonRange*_maxCannonRange && SmallestDistanceToWall(SDK.LocX, SDK.LocY) > 30)
                //    Lurk();
                //else
                //    DrunkWalk();
                DrunkWalk();
            }
        }

        private void GoToCenter()
        {
            double diffX = _arenaSize/2.0 - SDK.LocX;
            double diffY = _arenaSize/2.0 - SDK.LocY;
            _goToAngle = SDK.Rad2Deg(SDK.ATan2(diffY, diffX));
            SDK.Drive((int)_goToAngle, 1000);
            SDK.Log("Go to center {0:0.00}", _goToAngle);
        }

        private void Suicide()
        {
            _goToAngle = _currentEnemyAngle;
            SDK.Drive((int)_goToAngle, 100);
            _noMinDistanceLimit = true;
            SDK.Log("Leeeerooooyyyyyyyyy {0:0.00}", _goToAngle);
        }

        private void Lurk()
        {
            SDK.Log("Lurking around target");
        }

        private void DrunkWalk()
        {
            double distanceToTarget = Distance(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY);
            double timeMultiplier = 1.0;
            if (distanceToTarget > 300.0)
                timeMultiplier = 2.0;
            else if (distanceToTarget > 600.0)
                timeMultiplier = 3.0;
            double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
            if (SDK.Speed > 50)
            {
                if (SDK.Time - _lastDrunkTurnTime < 0.45D * timeMultiplier && distanceToWall > 30.0)
                    return;
                SDK.Drive((int)_goToAngle, 50);
                SDK.Log("DRUNK: Drive to angle {0:0.00}", _goToAngle);
                return;
            }
            if (distanceToWall < 30.0D)
            {
                double diffX = _arenaSize / 2.0 - SDK.LocX;
                double diffY = _arenaSize / 2.0 - SDK.LocY;
                _goToAngle = SDK.Rad2Deg(SDK.ATan2(diffY, diffX));
                SDK.Drive((int)_goToAngle, 100);
                _lastDrunkTurnTime = SDK.Time;
                SDK.Log("DRUNK: Too close from wall, go to center {0:0.00}", _goToAngle);
                return;
            }
            double t0 = _estimatedEnemyShotTime - (int)_estimatedEnemyShotTime; // [0, 1[
            double t1 = SDK.Time - (int)SDK.Time; // [0, 1[
            double diffT = SDK.Abs(t0 - t1); // estimate when I have to turn to avoid next shoot
            if ((diffT < 0.05D || diffT > 0.95D) && (SDK.Time - _lastDrunkTurnTime) > 0.5D)
            {
                int i = SDK.Rand(2);
                if (i == 0)
                {
                    _goToAngle += 90.0;
                    SDK.Log("DRUNK: Turning +1/4 {0:0.00}", _goToAngle);
                }
                else
                {
                    _goToAngle -= 90.0;
                    SDK.Log("DRUNK: Turning -1/4 {0:0.00}", _goToAngle);
                }
                SDK.Drive((int)_goToAngle, 100);
                _lastDrunkTurnTime = SDK.Time;
            }
        }

        // HELPERS

        private double SmallestDistanceToWall(double x, double y)
        {
            double smallestDistance = Double.MaxValue;
            // distance from left wall is x coordinate
            if (x < smallestDistance)
                smallestDistance = x;
            // distance from top wall is y coordinate
            if (y < smallestDistance)
                smallestDistance = y;
            // distance from right wall is maxsize - x coordinate
            if (_arenaSize - x < smallestDistance)
                smallestDistance = _arenaSize - x;
            // distance from bottom wall is maxsize - y coordinate
            if (_arenaSize - y < smallestDistance)
                smallestDistance = _arenaSize - y;
            return smallestDistance;
        }

        private void UpdateSharedInformations(int locX, int locY, int damage)
        {
            //SDK.Log("UPDATING INFO {0} {1} {2} {3}", _id, locX, locY, damage);
            TeamLocX[_id] = locX;
            TeamLocY[_id] = locY;
            TeamDamage[_id] = damage;
        }

        private bool IsFriendlyTarget(double locX, double locY)
        {
            if (_teamCount > 1)
            {
                //SDK.Log("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                for (int i = 0; i < _teamCount; i++)
                    if (i != _id && TeamDamage[i] < _maxDamage) // don't consider myself or dead teammate
                    {
                        //SDK.Log("TEAM MEMBER {0} : {1}, {2}", i, TeamLocX[i], TeamLocY[i]);
                        double distance2 = DistanceSquared(locX, locY, TeamLocX[i], TeamLocY[i]);
                        if (distance2 < FriendRangeSquared)
                            return true;
                    }
            }
            return false;
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            return SDK.Sqrt(DistanceSquared(x1, y1, x2, y2));
        }

        private double DistanceSquared(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            return dx*dx + dy*dy;
        }

        private void DifferenceRelativeToTime(double elapsed, double previousX, double previousY, double currentX, double currentY, out double relativeX, out double relativeY)
        {
            relativeX = (currentX - previousX) / elapsed;
            relativeY = (currentY - previousY) / elapsed;
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

            SDK.Log("{0:0.00} - SINAC - estimated position at {1:0.00} {2:0.0000} {3:0.0000}", SDK.Time, SDK.Time + t, pX, pY);

            //SDK.Log("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
        }
    }

    /* v2 2014-07-10
    // Fire on target, using linear interpolation to predict target position
        // Move randomly, change direction every 2 seconds or when hit
        public class SinaC : Robot
        {
            public static readonly int BorderSize = 30;
            public static readonly double FriendRangeSquared = 80*80;

            // Shared infos between team members
            private static readonly int[] TeamLocX = new int[8];
            private static readonly int[] TeamLocY = new int[8];
            private static readonly int[] TeamDamage = new int[8];

            private int _teamCount;
            private int _id; // own id

            private double _previousSuccessfulShootTime;
            private double _previousTime;
            private int _previousAngle;
            private int _previousRange;
            private double _previousEnemyX;
            private double _previousEnemyY;
            private double _previousSpeedX; // TODO: could be use to determine acceleration and get next speed estimation
            private double _previousSpeedY;
            private int _previousDamage;

            private int _arenaSize;
            private int _maxDamage;
            private int _missileSpeed;
            private int _maxExplosionRange;
            private int _maxExplosionRangePlusCannonRange;

            public override void Init()
            {
                System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.fff} - SINAC - Init", DateTime.Now);

                _previousTime = SDK.Time;
                _arenaSize = SDK.Parameters["ArenaSize"];
                _maxDamage = SDK.Parameters["MaxDamage"];
                _missileSpeed = SDK.Parameters["MissileSpeed"];
                _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
                _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];

                _teamCount = SDK.FriendsCount;
                _id = SDK.Id;
                _previousDamage = SDK.Damage;

                UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

                MoveRandomly();
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

                AvoidBorders();

                int currentDamage = SDK.Damage;
                if (elapsedTime > 2 )//|| currentDamage > _previousDamage) // 2 seconds since last move or damaged
                {
                    System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.fff} - SINAC - MOVE RANDOMLY FROM LOCATION {1} : {2},{3}  Damaged:{4}", DateTime.Now, _id, SDK.LocX, SDK.LocY, currentDamage > _previousDamage);

                    // Change direction
                    MoveRandomly();

                    //
                    _previousTime = currentTime;
                    _previousDamage = currentDamage;
                }
            }

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
                    //System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.fff} - SINAC - Nearest enemy of {1}: A{2} R{3} {4:0.0000},{5:0.0000}", DateTime.Now,, _id, enemyAngle, enemyRange, enemyX, enemyY);

                    double currentSpeedX, currentSpeedY;
                    ComputeSpeed(elapsedTime, _previousEnemyX, _previousEnemyY, enemyX, enemyY, out currentSpeedX, out currentSpeedY);

                    //System.Diagnostics.Debug.WriteLine("{0:HH:mm:ss.fff} - SINAC - TICK:{1:0.00} | Enemy position: {2:0.0000}, {3:0.0000} Speed : {4:0.0000}, {5:0.0000} | range {6} angle {7}", DateTime.Now, SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

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

            private void AvoidBorders()
            {
                if (SDK.LocX < BorderSize)
                    SDK.Drive(0, 50);
                else if (SDK.LocX >= _arenaSize - BorderSize)
                    SDK.Drive(180, 50);
                else if (SDK.LocY < BorderSize)
                    SDK.Drive(90, 50);
                else if (SDK.LocY >= _arenaSize - BorderSize)
                    SDK.Drive(270, 50);
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
                        if (i != _id && TeamDamage[i] < _maxDamage)
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
     */
    /* v1 2014-07-07
        public class SinaC2 : Robot
        {
            public static readonly int BorderSize = 30;
            public static readonly double FriendRangeSquared = 80 * 80;

            // Shared infos between team members
            private static readonly int[] TeamLocX = new int[8];
            private static readonly int[] TeamLocY = new int[8];
            private static readonly int[] TeamDamage = new int[8];

            private int _teamCount;
            private int _id; // own id

            private double _previousSuccessfulShootTime;
            private double _previousTime;
            private int _previousAngle;
            private int _previousRange;
            private double _previousEnemyX;
            private double _previousEnemyY;
            private double _previousSpeedX; // TODO: could be use to determine acceleration and get next speed estimation
            private double _previousSpeedY;

            private int _maxDamage;
            private int _missileSpeed;
            private int _maxExplosionRange;
            private int _maxExplosionRangePlusCannonRange;

            public override void Init()
            {
                _previousTime = SDK.Time;
                _missileSpeed = SDK.Parameters["MissileSpeed"];
                _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
                _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];

                _teamCount = SDK.FriendsCount;
                _id = SDK.Id;

                UpdateSharedInformations(SDK.LocX, SDK.LocY, SDK.Damage);

                DriveRandomly();
                FireOnTarget();
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
                    bool success = FireOnTargetInterpolated(elapsedShootingTime);

                    //
                    if (success)
                        _previousSuccessfulShootTime = SDK.Time;
                }

                if (elapsedTime > 1) // 1 second since last move
                {
                    System.Diagnostics.Debug.WriteLine("LOCATION {0} : {1},{2}", _id, SDK.LocX, SDK.LocY);

                    // Change direction
                    DriveRandomly();

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
                        if (i != _id && TeamDamage[i] < _maxDamage)
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
                double pX = enemyX + speedX * t;
                double pY = enemyY + speedY * t;

                double diffX = pX - robotX;
                double diffY = pY - robotY;

                Vector v = new Vector(diffX, diffY);
                angle = (int)SDK.Rad2Deg(v.A);
                range = (int)v.R;

                //System.Diagnostics.Debug.WriteLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
            }
        }
     */
}
