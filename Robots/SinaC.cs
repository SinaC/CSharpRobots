using System;
using SDK;

namespace Robots
{
    // TODO: fix linear interpolation: when I move and target doesn't move estimated enemy speed is wrong

    // Behaviour:
    //  Track previous enemy, if not found, search a new enemy (don't consider teammate as enemy)
    //  Fire on tracked/found enemy using linear interpolation if enemy tracked, using direct fire if new enemy
    //  If far from enemy, drive to fire range and fire/try to stay at fire range using zigzag pattern
    //  Else, square wave pattern trying to change direction to avoid missile
    public class SinaC : Robot
    {
        private enum MoveModes
        {
            // Don't move
            Still,
            // Move from one corner to another
            Corner,
            // Simple mode: go to target if far and random if in range
            Simple,
            // Shark mode: go to predefined destination and then circle
            Shark,
        };

        private enum SharkModes
        {
            GoToDestination, // -> DecreaseSpeed
            DecreaseSpeed, // -> Circling
            Circling, // -> GoToDestination
        }

        // Robot parameters
        private const double Pi = 3.14159;
        private const bool UpgradedPrecision = true;
        private const bool UseInterpolation = true;
        private const double FriendRange = 20;
        private const double TrackStepTime = 0.25;

        // Shared infos between team members
        private static readonly int[] TeamLocX = new int[8];
        private static readonly int[] TeamLocY = new int[8];
        private static readonly int[] TeamDamage = new int[8];
        private static readonly double[] TeamEnemyX = new double[8];
        private static readonly double[] TeamEnemyY = new double[8];

        //
        private int _teamCount; // number of members in team
        private int _id; // own id
        private double _fireCount;
        private int _fireEnemyAngle;
        private int _fireEnemyRange;
        private MoveModes _moveMode;
        private SharkModes _sharkMode;
        private double _driveAngle;
        
        // Simple move
        private double _lastRandomTurn;
        
        // Shark move
        private int _sector;
        private int _destinationX;
        private int _destinationY;

        // Current state
        private int _currentLocX;
        private int _currentLocY;
        private double _currentEnemyAngle;
        private double _currentEnemyRange;
        private double _currentEnemyX;
        private double _currentEnemyY;
        private double _currentEnemySpeedX;
        private double _currentEnemySpeedY;

        // Previous state
        private int _previousLocX;
        private int _previousLocY;
        private double _previousEnemyAngle;
        private double _previousEnemyRange;
        private double _previousEnemyX;
        private double _previousEnemyY;
        private double _previousEnemySpeedX;
        private double _previousEnemySpeedY;
        private double _previousTrackTime;
        private double _previousCannonTime;
        private int _previousDamage;

        // Heuristics
        private double _lastHitTime;
        private double _estimatedEnemyCannonTime;

        // Constants
        private int _arenaSize;
        private int _maxDamage;
        private int _missileSpeed;
        private int _maxExplosionRange;
        private int _maxExplosionRangePlusCannonRange;
        private int _maxCannonRange;
        private int _maxSpeed;

        public override void Init()
        {
            SDK.LogLine("{0:0.00} - Init - v5", SDK.Time);

            _arenaSize = SDK.Parameters["ArenaSize"];
            _maxDamage = SDK.Parameters["MaxDamage"];
            _missileSpeed = SDK.Parameters["MissileSpeed"];
            _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
            _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];
            _maxCannonRange = SDK.Parameters["MaxCannonRange"];
            _maxSpeed = SDK.Parameters["MaxSpeed"];

            _teamCount = SDK.FriendsCount;
            _id = SDK.Id;
            _fireCount = 0;
            _previousTrackTime = SDK.Time;
            _previousDamage = SDK.Damage;
            _previousCannonTime = SDK.Time;

            _currentLocX = SDK.LocX;
            _currentLocY = SDK.LocY;

            // Save own location/damage
            UpdateSharedLocationAndDamage(_currentLocX, _currentLocY, SDK.Damage);

            // Find target
            bool found = FindNearestEnemy(_maxExplosionRange);
            ComputeCannonNoInterpolationInformation();
            if (found)
                FireOnEnemy(_maxExplosionRange, _maxExplosionRangePlusCannonRange);
            SaveCurrentState();

            _moveMode = MoveModes.Still;

            //_moveMode = MoveModes.Shark; _sharkMode = SharkModes.GoToDestination;

            // Shark
            if (_moveMode == MoveModes.Shark)
            {
                if (_teamCount == 1)
                {
                    _destinationX = Clamp(_currentLocX, 100, _arenaSize - 100);
                    _destinationY = Clamp(_currentLocY, 100, _arenaSize - 100);
                }
                else
                {
                    _destinationX = _currentLocX < 500 ? 100 : 900;
                    _destinationY = _currentLocY < 500 ? 100 : 900;
                }
            }
            // Corner mode
            else if (_moveMode == MoveModes.Corner)
            {
                _destinationX = 100;
                _destinationY = 100;
            }

            Move();
        }

        public override void Step()
        {
            _currentLocX = SDK.LocX;
            _currentLocY = SDK.LocY;

            // Update shared info
            UpdateSharedLocationAndDamage(_currentLocX, _currentLocY, SDK.Damage);

            double currentTime = SDK.Time;

            double elapsedTrackTime = currentTime - _previousTrackTime;
            double elapsedCannonTime = currentTime - _previousCannonTime;

            bool newEnemy = false;
            if (elapsedTrackTime > TrackStepTime || elapsedCannonTime >= 1) // track/find enemy
            {
                // TODO: use enemy speed/acceleration to compute max range difference
                double maxRangeDifference = 10.0;//_maxSpeed * elapsedTime + 0.5; // add 0.5 to be sure
                bool tracked = TrackEnemy(_maxExplosionRange, maxRangeDifference); // update current state
                if (!tracked)
                {
                    //SDK.LogLine("Enemy LOST, searching a new one");
                    FindNearestEnemy(_maxExplosionRange); // update current state
                    newEnemy = true;
                    SaveCurrentState(); // save current state, so I could perform an interpolation on new enemy if new enemy is not detected just before firing
                }
                _previousTrackTime = currentTime;
            }

            if (elapsedCannonTime >= 1) // 1 second since last successfull shoot
            {
                // Fire on target
                if (newEnemy || !UseInterpolation)
                    ComputeCannonNoInterpolationInformation();
                else
                    ComputeCannonInterpolatedInformation(elapsedCannonTime); // this use current and previous state
                FireOnEnemy(_maxExplosionRange, _maxExplosionRangePlusCannonRange);
                SaveCurrentState();
            }

            // Estimate enemy shot time using previous distance between enemy and I, supposing current enemy is the shooter
            int currentDamage = SDK.Damage;
            if (currentDamage != _previousDamage)
            {
                int damageTaken = currentDamage - _previousDamage;
                _lastHitTime = currentTime;
                _estimatedEnemyCannonTime = _lastHitTime - Distance(_currentLocX, _currentLocY, _previousEnemyX, _previousEnemyY) / _missileSpeed;
                _previousDamage = currentDamage;
                SDK.LogLine("Damage detected: hit time {0} estimated shoot time {1}", _lastHitTime, _estimatedEnemyCannonTime);
            }

            Move();
        }

        #region Current state and shared informations

        private void UpdateSharedLocationAndDamage(int locX, int locY, int damage)
        {
            TeamLocX[_id] = locX;
            TeamLocY[_id] = locY;
            TeamDamage[_id] = damage;
        }

        private void UpdateSharedEnemyLocation(double locX, double locY)
        {
            TeamEnemyX[_id] = locX;
            TeamEnemyY[_id] = locY;
        }

        private void SaveCurrentState()
        {
            //SDK.LogLine("Save current state");
            _previousLocX = _currentLocX;
            _previousLocY = _currentLocY;
            _previousEnemyAngle = _currentEnemyAngle;
            _previousEnemyRange = _currentEnemyRange;
            _previousEnemyX = _currentEnemyX;
            _previousEnemyY = _currentEnemyY;
            _previousEnemySpeedX = _currentEnemySpeedX;
            _previousEnemySpeedY = _currentEnemySpeedY;
        }

        #endregion

        #region Tracking

        // Scan all around
        private bool FindNearestEnemy(double minDistance)
        {
            //SDK.LogLine("**************Searching a new enemy");

            bool found = false;
            double bestRange = Double.MaxValue;
            for (int a = 0; a < 360; a++)
            {
                int r = SDK.Scan(a, 1);

                double preciseRange = r;
                double preciseAngle = a;
                if (UpgradedPrecision)
                    UpdgradePrecision(a, r, out preciseAngle, out preciseRange);

                // Target must be in range
                if (r > minDistance && r < bestRange)
                {
                    double enemyX, enemyY;
                    ComputePoint(_currentLocX, _currentLocY, preciseRange, preciseAngle, out enemyX, out enemyY);
                    if (!IsFriendlyTarget(enemyX, enemyY))
                    {
                        double cheatAngle, cheatRange, cheatX, cheatY;
                        Cheat.FindNearestEnemy(out cheatAngle, out cheatRange, out cheatX, out cheatY);

                        // Save enemy position/angle/range
                        _currentEnemyAngle = preciseAngle;
                        _currentEnemyRange = preciseRange;
                        _currentEnemyX = enemyX;
                        _currentEnemyY = enemyY;

                        // Save enemy to shared
                        UpdateSharedEnemyLocation(_currentEnemyX, _currentEnemyY);

                        SDK.LogLine("FIND CHEAT A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", cheatAngle, cheatRange, cheatX, cheatY);
                        SDK.LogLine("FIND ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

                        bestRange = preciseRange;
                        found = true;
                    }

                }
            }
            return found;
        }

        // Search previous target without rescanning everything
        private bool TrackEnemy(double minDistance, double maxRangeDifference)
        {
            //SDK.LogLine("TRACKING ENEMY");
            // In following method, _currentEnemyAngle and _currentEnemyRange represents angle/range in previous TrackEnemy
            bool tracked = false;
            int a = (int) SDK.Round(_currentEnemyAngle);
            int r;
            int sign = 1; // sign of angle increment
            int increment = 0; // angle increment

            // sample: starting with angle = 20
            // 20, 20+1*1=21, 21-1*2=19, 19+1*3=22, ...
            while (true)
            {
                r = SDK.Scan(a, 1);
                //SDK.LogLine("Tracking enemy: a:{0} r:{1} inc:{2} sign:{3}  diff:{4:0.0000}  max diff: {5:0.0000}", a, r, increment, sign, SDK.Abs(r - _currentEnemyRange), maxRangeDifference);
                if (r > 0 || increment >= 10)
                    break;
                increment++;
                a += sign*increment;
                sign = -sign;
            }
            if (r > minDistance && SDK.Abs(r - _currentEnemyRange) <= maxRangeDifference)
            {
                double preciseRange = r;
                double preciseAngle = a;
                if (UpgradedPrecision)
                    UpdgradePrecision(a, r, out preciseAngle, out preciseRange);
                //
                double enemyX, enemyY;
                ComputePoint(_currentLocX, _currentLocY, preciseRange, preciseAngle, out enemyX, out enemyY);

                //
                double rawEnemyX, rawEnemyY;
                ComputePoint(_currentLocX, _currentLocY, r, a, out rawEnemyX, out rawEnemyY);

                double cheatAngle, cheatRange, cheatX, cheatY;
                Cheat.FindNearestEnemy(out cheatAngle, out cheatRange, out cheatX, out cheatY);

                // Save enemy position/angle/range
                _currentEnemyAngle = preciseAngle;
                _currentEnemyRange = preciseRange;
                _currentEnemyX = enemyX;
                _currentEnemyY = enemyY;

                // Save enemy to shared
                UpdateSharedEnemyLocation(_currentEnemyX, _currentEnemyY);

                SDK.LogLine("TRACK CHEAT A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", cheatAngle, cheatRange, cheatX, cheatY);
                SDK.LogLine("TRACK ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);
                SDK.LogLine("  RAW ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", a, r, rawEnemyX, rawEnemyY);

                //SDK.LogLine("TRACK ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

                tracked = true;
            }
            return tracked;
        }

        // Try to get more precision by scanning 1 degree before and 1 degree after with double resolution and weight 1/8
        private void UpdgradePrecision(int a, int r, out double preciseAngle, out double preciseRange)
        {
            preciseAngle = a;
            preciseRange = r;
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

        #endregion

        #region Fire

        private void ComputeCannonNoInterpolationInformation()
        {
            _fireEnemyAngle = (int) SDK.Round(_currentEnemyAngle);
            _fireEnemyRange = (int) SDK.Round(_currentEnemyRange);
        }

        private void ComputeCannonInterpolatedInformation(double elapsedTime)
        {
            DifferenceRelativeToTime(elapsedTime, _previousEnemyX, _previousEnemyY, _currentEnemyX, _currentEnemyY, out _currentEnemySpeedX, out _currentEnemySpeedY);

            //double currentAccelerationX, currentAccelerationY;
            //DifferenceRelativeToTime(elapsedTime, _previousEnemySpeedX, _previousEnemySpeedY, currentSpeedX, currentSpeedY, out currentAccelerationX, out currentAccelerationY);

            SDK.LogLine("{0:0.00} | Enemy position: {1:0.0000}, {2:0.0000} Speed:{3:0.0000}, {4:0.0000} | range {5} angle {6} | Loc:{7},{8}", SDK.Time, _currentEnemyX, _currentEnemyY, _currentEnemySpeedX, _currentEnemySpeedY, _currentEnemyRange, _currentEnemyAngle, _currentLocX, _currentLocY);
            //SDK.LogLine("{0:0.00} - estimated speed {1:0.0000} {2:0.0000} acceleration {3:0.0000} {4:0.0000}", SDK.Time, _currentEnemySpeedX, _currentEnemySpeedY, currentAccelerationX, currentAccelerationY);

            //http://jrobots.sourceforge.net/jjr_tutorials.shtml
            // Say P (using vector notation) the unknown point in which the missile meets the enemy, R the starting location of your robot, T the starting location of the target and V its velocity. 
            // The target will reach the point P in t seconds, according to the formula:
            // P = T + V t
            // (P - R)^2 = (300 t)^2
            // Solving these 2 equations:
            // t = ( sqrt(300^2 D^2 - (DxV)^2) + D(dot)V ) / (300^2 - V^2)  with D = T-R
            // t = ( sqrt(300^2 (Dx^2 + Dy^2) - (DxVy - DyVx)^2) + (DxVx + DyVy) ) / (300^2 - (Vx^2 + Vy^2) )
            double dX = _currentEnemyX - _currentLocX;
            double dY = _currentEnemyY - _currentLocY;
            double t = SDK.Sqrt(_missileSpeed*_missileSpeed*(dX*dX + dY*dY) - (dX*_currentEnemySpeedY - dY*_currentEnemySpeedX)*(dX*_currentEnemySpeedY - dY*_currentEnemySpeedX) + 0.5)/(_missileSpeed*_missileSpeed - (_currentEnemySpeedX*_currentEnemySpeedX + _currentEnemySpeedY*_currentEnemySpeedY));
            double pX = _currentEnemyX + _currentEnemySpeedX*t;
            double pY = _currentEnemyY + _currentEnemySpeedY*t;

            _fireEnemyAngle = (int)SDK.Round(Angle(_currentLocX, _currentLocY, pX, pY));
            _fireEnemyRange = (int)SDK.Round(Distance(pX, pY, _currentLocX, _currentLocY));

            SDK.LogLine("{0:0.00} - estimated position at {1:0.00} {2:0.0000} {3:0.0000}  A:{4:0.0000} R:{5:0.0000}", SDK.Time, SDK.Time + t, pX, pY, FixAngle(_fireEnemyAngle), _fireEnemyRange);

            //SDK.LogLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
        }

        // Fire interpolation
        private bool FireOnEnemy(double minDistance, double maxDistance)
        {
            if (_fireEnemyRange > minDistance && _fireEnemyRange < maxDistance) // Don't fire if too far or if too near
            {
                bool fired = SDK.Cannon(_fireEnemyAngle, _fireEnemyRange) != 0;
                if (fired)
                {
                    SDK.LogLine("Fired on enemy count: {0}", _fireCount);
                    _fireCount++;
                    _previousCannonTime = SDK.Time;
                    return true;
                }
            }
            return false;
        }

        private bool IsFriendlyTarget(double locX, double locY)
        {
            if (_teamCount > 1)
            {
                //SDK.LogLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                for (int i = 0; i < _teamCount; i++)
                    if (i != _id && TeamDamage[i] < _maxDamage) // don't consider myself or dead teammate
                    {
                        //SDK.LogLine("TEAM MEMBER {0} : {1}, {2}", i, TeamLocX[i], TeamLocY[i]);
                        double distance = Distance(locX, locY, TeamLocX[i], TeamLocY[i]);
                        if (distance < FriendRange)
                            return true;
                    }
            }
            return false;
        }

        #endregion

        #region Move

        private void Move()
        {
            switch(_moveMode)
            {
                case MoveModes.Still:
                    SDK.Drive(0,0);
                    break;
                case MoveModes.Corner:
                    CornerMove();
                    break;
                case MoveModes.Simple:
                    SimpleMove();
                    break;
                case MoveModes.Shark:
                    SharkMove();
                    break;
            }
        }

        #region Simple

        private void SimpleMove()
        {
            double distanceToEnemy = Distance(_currentLocX, _currentLocY, _currentEnemyX, _currentEnemyY);

            if (distanceToEnemy > _maxExplosionRangePlusCannonRange)
                MoveToEnemy();
            else
                MoveRandomly();
        }

        private void MoveRandomly()
        {
            double distanceToTarget = Distance(_currentLocX, _currentLocY, _currentEnemyX, _currentEnemyY);
            double timeMultiplier = 1.0; // move less often when far from target
            if (distanceToTarget > 300.0)
                timeMultiplier = 2.0;
            else if (distanceToTarget > 600.0)
                timeMultiplier = 3.0;
            double distanceToWall = SmallestDistanceToWall(_currentLocX, _currentLocY);
            if (SDK.Speed > 50) // Slow down without changing direction
            {
                if (SDK.Time - _lastRandomTurn >= 0.45 * timeMultiplier || distanceToWall > 30.0) // But only if I not turned too recently or very close to wall
                {
                    Drive(_driveAngle, 50);
                    //SDK.LogLine("Slowing down {0:0.00} {1}", _driveAngle, SDK.Speed);
                }
            }
            else if (distanceToWall < 30.0) // Escape from wall, moving to center
            {
                _driveAngle = Angle(_currentLocX, _currentLocY, _arenaSize/2.0, _arenaSize/2.0);
                Drive(_driveAngle, 100);
                _lastRandomTurn = SDK.Time;
                //SDK.LogLine("Driving to center away from wall at full speed {0:0.00} {1}", _driveAngle, SDK.Speed);
            }
                //SDK.LogLine("t0:{0:0.0000}  t1:{1:0.0000}  diff:{2:0.0000}", t0, t1, diffT);
            else
            {
                // Check if being hit or didn't turn too recently
                double t0 = _estimatedEnemyCannonTime - (int) _estimatedEnemyCannonTime; // [0, 1[
                double t1 = SDK.Time - (int) SDK.Time; // [0, 1[
                double diffT = SDK.Abs(t0 - t1); // estimate when I have to turn to avoid next shoot

                if ((diffT < 0.05 || diffT > 0.95) && SDK.Time - _lastRandomTurn > 0.5)
                //else if (SDK.Time - _lastRandomTurn > 0.9) // Change direction randomly if not turned too recently
                {
                    int i = SDK.Rand(2);
                    if (i == 0)
                    {
                        _driveAngle += 90.0;
                        //SDK.LogLine("Turning +1/4 at full speed {0:0.00} {1}", _driveAngle, SDK.Speed);
                    }
                    else
                    {
                        _driveAngle -= 90.0;
                        //SDK.LogLine("Turning -1/4 at full speed {0:0.00} {1}", _driveAngle, SDK.Speed);
                    }
                    Drive(_driveAngle, 100);
                    _lastRandomTurn = SDK.Time;
                }
            }
        }

        private void MoveToEnemy()
        {
            _driveAngle = _currentEnemyAngle;
            Drive(_driveAngle, 100);
            //SDK.LogLine("Move to enemy at full speed");
        }

        #endregion

        #region Corner

        private void CornerMove()
        {
            //SDK.Drive(0, SDK.Rand(2) == 0 ? 100 : 50);
            double distanceToDestination = Distance(_currentLocX, _currentLocY, _destinationX, _destinationY);
            if (distanceToDestination < 100 && SDK.Speed > 50)
            {
                _driveAngle = Angle(_currentLocX, _currentLocY, _destinationX, _destinationY);
                Drive(_driveAngle, 50);
                //SDK.LogLine("Set speed to 50 {0:0.0000}", _driveAngle);
            }
            else if (distanceToDestination < 75)
            {
                // Change corner
                if (_currentLocX <= 100 && _currentLocY <= 100) // top left, go to top right
                {
                    _destinationX = _arenaSize - 100;
                    //SDK.LogLine("Changing corner: from top left to top right");
                }
                else if (_currentLocX >= _arenaSize - 100 && _currentLocY <= 100) // top right, go to bottom right
                {
                    _destinationY = _arenaSize - 100;
                    //SDK.LogLine("Changing corner: from top right to bottom right");
                }
                else if (_currentLocX >= _arenaSize - 100 && _currentLocY >= _arenaSize - 100)  // bottom right, go to bottom left
                {
                    _destinationX = 100;
                    //SDK.LogLine("Changing corner: from bottom right to bottom left");
                }
                else if (_currentLocX <= 100 && _currentLocY >= _arenaSize - 100) // bottom left, go to top left
                {
                    _destinationY = 100;
                    //SDK.LogLine("Changing corner: from bottom left to top left");
                }
                _driveAngle = Angle(_currentLocX, _currentLocY, _destinationX, _destinationY);
                Drive(_driveAngle, 100);
                //SDK.LogLine("Set speed to 100 (1) {0:0.0000}", _driveAngle);
            }
            else
            {
                _driveAngle = Angle(_currentLocX, _currentLocY, _destinationX, _destinationY);
                Drive(_driveAngle, 100);
                //SDK.LogLine("Set speed to 100 (2) {0:0.0000}", _driveAngle);
            }
        }

        #endregion

        #region Shark

        private void SharkMove()
        {
            switch (_sharkMode)
            {
                case SharkModes.GoToDestination:
                    GoToDestination();
                    break;
                case SharkModes.DecreaseSpeed:
                    DecreaseSpeed();
                    break;
                case SharkModes.Circling:
                    Circling();
                    break;
            }
        }

        private void GoToDestination()
        {
            if (FarFromDestination())
            {
                _driveAngle = Angle(_currentLocX, _currentLocY, _destinationX, _destinationY);
                Drive(_driveAngle, 100);
                //SDK.LogLine("Far from destination {0},{1} go to this destination at full speed {2:0.00} {3}", _destinationX, _destinationY, _driveAngle, SDK.Speed);
            }
            else
                _sharkMode = SharkModes.DecreaseSpeed;
        }

        private void DecreaseSpeed()
        {
            if (SDK.Speed > 50)
            {
                Drive(_driveAngle, 50);
                //SDK.LogLine("Destination reached, slowing down {0:0.00} {1}", _driveAngle, SDK.Speed);
            }
            else
                _sharkMode = SharkModes.Circling;
        }

        private void Circling()
        {
            if (SDK.Speed > 0)
            {
                if (_currentLocX > _destinationX + 15)
                {
                    if (_currentLocY > _destinationY + 15)
                    {
                        if (_sector != 1)
                        {
                            _sector = 1;
                            SDK.Drive(315, 50);
                        }
                    }
                    else if (_currentLocY > _destinationY - 15)
                    {
                        if (_sector != 2)
                        {
                            _sector = 2;
                            SDK.Drive(270, 50);
                        }
                    }
                    else if (_sector != 3)
                    {
                        _sector = 3;
                        SDK.Drive(225, 50);
                    }
                }
                else if (_currentLocX > _destinationX - 15)
                {
                    if (_currentLocY > _destinationY + 15)
                    {
                        if (_sector != 4)
                        {
                            _sector = 4;
                            SDK.Drive(0, 50);
                        }
                    }
                    else if (_currentLocY > _destinationY - 15)
                    {
                        if (_sector != 5)
                        {
                            _sector = 5;
                            SDK.Drive(45, 50);
                        }
                    }
                    else if (_sector != 6)
                    {
                        _sector = 6;
                        SDK.Drive(180, 50);
                    }
                }
                else if (_currentLocY > _destinationY + 15)
                {
                    if (_sector != 7)
                    {
                        _sector = 7;
                        SDK.Drive(45, 50);
                    }
                }
                else if (_currentLocY > _destinationY - 15)
                {
                    if (_sector != 8)
                    {
                        _sector = 8;
                        SDK.Drive(90, 50);
                    }
                }
                else if (_sector != 9)
                {
                    _sector = 9;
                    SDK.Drive(135, 50);
                }
            }
            else
                _sharkMode = SharkModes.GoToDestination;
        }

        public bool FarFromDestination()
        {
            int diffX = _currentLocX - _destinationX;
            int diffY = _currentLocY - _destinationY;
            return diffX * diffX + diffY * diffY > 5500;
        }

        #endregion

        #endregion

        #region Helpers

        private double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private void Drive(double angle, int speed)
        {
            SDK.Drive((int) SDK.Round(angle), speed);
        }

        private double SmallestDistanceToWall(double locX, double locY)
        {
            double smallestDistance = Double.MaxValue;
            // distance from left wall is locX coordinate
            if (locX < smallestDistance)
                smallestDistance = locX;
            // distance from top wall is locY coordinate
            if (locY < smallestDistance)
                smallestDistance = locY;
            // distance from right wall is maxsize - locX coordinate
            if (_arenaSize - locX < smallestDistance)
                smallestDistance = _arenaSize - locX;
            // distance from bottom wall is maxsize - locY coordinate
            if (_arenaSize - locY < smallestDistance)
                smallestDistance = _arenaSize - locY;
            return smallestDistance;
        }

        private int FixAngle(int angle)
        {
            angle %= 360;
            if (angle < 0)
                angle += 360;
            return angle;
        }

        private double FixAngle(double angle)
        {
            angle %= 360.0;
            if (angle < 0)
                angle += 360.0;
            return angle;
        }

        private double Angle(double x1, double y1, double x2, double y2)
        {
            double diffX = x2 - x1;
            double diffY = y2 - y1;
            double angleRadians = SDK.ATan2(diffY, diffX);
            if (angleRadians >= Pi)
                angleRadians = 2 * Pi - angleRadians;
            return SDK.Rad2Deg(angleRadians);
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
            relativeX = (currentX - previousX)/elapsed;
            relativeY = (currentY - previousY)/elapsed;
        }

        private void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = SDK.Deg2Rad(degrees);
            x = centerX + distance*SDK.Cos(radians);
            y = centerY + distance*SDK.Sin(radians);
        }

        #endregion
    }

    /* v4 2014-07-16
        // TODO: 
       //  when switching enemy, reset enemy speed and no distance limit
       //  try to focus on the same enemy even if a nearest enemy is found
       //  share target infos, store every enemy around
       //      if multiple friend can shoot the same enemy, shoot this enemy first    
       // Behaviour:
       // Fire on target, using linear interpolation to predict target position
       // Move to center if stopped, move to enemy if damage is low, move drunk otherwise
       // No specific behavior on double or team match except avoiding friendly fire
       public class SinaC : Robot
       {
           private const bool EnableMove = true;
           private const bool UpgradedPrecision = true;
           private const double Epsilon = 0.00001;
           private const int BorderSize = 30;
           private const double FriendRangeSquared = 40*40;
           private const double TrackFindTimeSlot = 0.25;
           private const double LurkDistance = 743;

           // Shared infos between team members
           private static readonly int[] TeamLocX = new int[8];
           private static readonly int[] TeamLocY = new int[8];
           private static readonly int[] TeamDamage = new int[8];
           private static readonly double[] TeamEnemyX = new double[8];
           private static readonly double[] TeamEnemyY = new double[8];

           private int _teamCount; // number of members in team
           private int _id; // own id

           // enemy position history
           private int _historySize;
           private double[] _historyEnemyX;
           private double[] _historyEnemyY;

           // Updated with FindNearestEnemy and TrackEnemy
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
           private int _dartMode;
           private bool _hangFire;
           private double _dartStartTime;
           private int _dartModeDamage = 0;

           // Constants
           private int _arenaSize;
           private int _maxDamage;
           private int _missileSpeed;
           private int _maxExplosionRange;
           private int _maxExplosionRangePlusCannonRange;
           private int _maxCannonRange;
           private int _maxSpeed;

           public override void Init()
           {
               SDK.LogLine("{0:HH:mm:ss.fff} - Init - v4", DateTime.Now);

               _previousTime = SDK.Time;
               _arenaSize = SDK.Parameters["ArenaSize"];
               _maxDamage = SDK.Parameters["MaxDamage"];
               _missileSpeed = SDK.Parameters["MissileSpeed"];
               _maxExplosionRange = SDK.Parameters["MaxExplosionRange"];
               _maxExplosionRangePlusCannonRange = SDK.Parameters["MaxExplosionRange"] + SDK.Parameters["MaxCannonRange"];
               _maxCannonRange = SDK.Parameters["MaxCannonRange"];
               _maxSpeed = SDK.Parameters["MaxSpeed"];

               _teamCount = SDK.FriendsCount;
               _id = SDK.Id;
               _previousDamage = SDK.Damage;
               _noMinDistanceLimit = false;
               _dartMode = 0;
               _hangFire = false;
               _dartStartTime = 0;
               _dartModeDamage = 0;
               _previousEnemyX = -1;
               _previousEnemyY = -1;

               _historySize = (int)(SDK.Parameters["MaxMatchTime"]/TrackFindTimeSlot) + 50; // +50 to be sure :)
               _historyEnemyX = new double[_historySize];
               _historyEnemyY = new double[_historySize];
               for (int i = 0; i < _historySize; i++ )
               {
                   _historyEnemyX[i] = -1;
                   _historyEnemyY[i] = -1;
               }

               UpdateSharedLocation(SDK.LocX, SDK.LocY, SDK.Damage);

               if (EnableMove)
                   MoveSmartly();
               bool found = FindNearestEnemy();
               RegisterHistory(SDK.Time, _currentEnemyX, _currentEnemyY);
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
               UpdateSharedLocation(SDK.LocX, SDK.LocY, SDK.Damage);

               double currentTime = SDK.Time;

               double elapsedTime = currentTime - _previousTime;
               double elapsedShootingTime = currentTime - _previousSuccessfulShootTime;

               if (elapsedTime > TrackFindTimeSlot || elapsedShootingTime >= 1) // track/find enemy
               {
                   // TODO: use enemy speed/acceleration to compute max range difference
                   double maxRangeDifference = 10.0;//_maxSpeed * elapsedTime + 0.5; // add 0.5 to be sure
                   bool found = TrackEnemy(maxRangeDifference);
                   if (!found)
                   {
                       //SDK.LogLine("Enemy LOST, searching a new one");
                       FindNearestEnemy();
                   }
                   RegisterHistory(currentTime, _currentEnemyX, _currentEnemyY);
                   _previousTime = currentTime;
               }

               if (elapsedShootingTime >= 1 && !_hangFire) // 1 second since last successfull shoot
               {
                   bool success;
                   // Fire on target
                   if (_previousEnemyX < 0 && _previousEnemyY < 0)
                       success = FireOnEnemy();
                   else
                       success = FireOnEnemyInterpolated(elapsedShootingTime);

                   //
                   if (success)
                       _previousSuccessfulShootTime = SDK.Time;
               }

               // Estimate enemy shot time using previous distance between enemy and I, supposing my target is the shooter
               int currentDamage = SDK.Damage;
               if (currentDamage != _previousDamage)
               {
                   int damageTaken = currentDamage - _previousDamage;
                   _lastHitTime = currentTime;
                   _estimatedEnemyShotTime = _lastHitTime - Distance(SDK.LocX, SDK.LocY, _previousEnemyX, _previousEnemyY) / _missileSpeed;
                   _previousDamage = currentDamage;
                   SDK.LogLine("Damage detected: hit time {0} estimated shoot time {1}", _lastHitTime, _estimatedEnemyShotTime);

                   if (_dartMode != 0)
                       _dartModeDamage += damageTaken;
               }

               if (EnableMove)
                   MoveSmartly();
           }

           private bool FireOnEnemy()
           {
               if (_currentEnemyRange < _maxExplosionRangePlusCannonRange) // Don't fire if too far
               {
                   SDK.Cannon((int)SDK.Round(_currentEnemyAngle), (int)SDK.Round(_currentEnemyRange));

                   _previousEnemyAngle = _currentEnemyAngle;
                   _previousEnemyRange = _currentEnemyRange;
                   _previousEnemyX = _currentEnemyX;
                   _previousEnemyY = _currentEnemyY;

                   return true;
               }
               return false;
           }

           // Compute enemy current speed, then use linear interpolation to compute next enemy position
           private bool FireOnEnemyInterpolated(double elapsedTime)
           {
               bool success = false;
               SDK.LogLine("{0:0.00} - Nearest enemy of {1}: A:{2:0.0000} R:{3:0.0000} {4:0.0000},{5:0.0000}", SDK.Time, _id, _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

               double currentSpeedX, currentSpeedY;
               DifferenceRelativeToTime(elapsedTime, _previousEnemyX, _previousEnemyY, _currentEnemyX, _currentEnemyY, out currentSpeedX, out currentSpeedY);

               //double currentAccelerationX, currentAccelerationY;
               //DifferenceRelativeToTime(elapsedTime, _previousEnemySpeedX, _previousEnemySpeedY, currentSpeedX, currentSpeedY, out currentAccelerationX, out currentAccelerationY);

               //SDK.LogLine("{0:0.00} - TICK:{1:0.00} | Enemy position: {2:0.0000}, {3:0.0000} Speed : {4:0.0000}, {5:0.0000} | range {6} angle {7}", DateTime.Now, SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);
               //SDK.LogLine("{0:0.00} - estimated speed {1:0.0000} {2:0.0000} acceleration {3:0.0000} {4:0.0000}", SDK.Time, currentSpeedX, currentSpeedY, currentAccelerationX, currentAccelerationY);

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
           private bool FindNearestEnemy()
           {
               //SDK.LogLine("**************Searching a new enemy");

               bool found = false;
               double bestRange = double.MaxValue;
               for (int a = 0; a < 360; a++)
               {
                   int r = SDK.Scan(a, 1);

                   double preciseRange = r;
                   double preciseAngle = a;
                   if (UpgradedPrecision)
                       UpdgradePrecision(a, r, out preciseAngle, out preciseRange);

                   // Target must be in range
                   if ((_noMinDistanceLimit || r > _maxExplosionRange) && r < bestRange)
                   {
                       //double rawEnemyX, rawEnemyY;
                       //ComputePoint(SDK.LocX, SDK.LocY, r, a, out rawEnemyX, out rawEnemyY);

                       double enemyX, enemyY;
                       ComputePoint(SDK.LocX, SDK.LocY, preciseRange, preciseAngle, out enemyX, out enemyY);
                       if (!IsFriendlyTarget(enemyX, enemyY))
                       {
                           //double cheatAngle, cheatRange, cheatX, cheatY;
                           //Cheat.FindNearestEnemy(out cheatAngle, out cheatRange, out cheatX, out cheatY);

                           _currentEnemyAngle = preciseAngle;
                           _currentEnemyRange = preciseRange;
                           _currentEnemyX = enemyX;
                           _currentEnemyY = enemyY;

                           UpdateSharedEnemy(_currentEnemyX, _currentEnemyY);

                           //SDK.LogLine(" CHEAT A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", cheatAngle, cheatRange, cheatX, cheatY);
                           //SDK.LogLine("NORMAL A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}  RAW A:{4:0.0000} R:{5:0.0000} X:{6:0.0000} Y:{7:0.0000}", angle, range, enemyX, enemyY, a, r, rawEnemyX, rawEnemyY);
                        
                           //SDK.LogLine("FIND ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

                           bestRange = preciseRange;
                           found = true;
                       }

                   }
               }
               return found;
           }

           // Search previous target without rescanning everything
           private bool TrackEnemy(double maxRangeDifference)
           {
               //SDK.LogLine("TRACKING ENEMY");
               // In following method, _currentEnemyAngle and _currentEnemyRange represents angle/range in previous TrackEnemy
               bool found = false;
               int a = (int)SDK.Round(_currentEnemyAngle);
               int r;
               int sign = 1; // sign of angle increment
               int increment = 0; // angle increment

               // sample: starting with angle = 20
               // 20, 20+1*1=21, 21-1*2=19, 19+1*3=22, ...
               while (true)
               {
                   r = SDK.Scan(a, 1);
                   //SDK.LogLine("Tracking enemy: a:{0} r:{1} inc:{2} sign:{3}  diff:{4:0.0000}  max diff: {5:0.0000}", a, r, increment, sign, SDK.Abs(r - _currentEnemyRange), maxRangeDifference);
                   if (r > 0 || increment >= 10)
                       break;
                   increment++;
                   a += sign*increment;
                   sign = -sign;
               }
               if (r > 0 && SDK.Abs(r - _currentEnemyRange) <= maxRangeDifference)
               {
                   double preciseRange = r;
                   double preciseAngle = a;
                   if (UpgradedPrecision)
                       UpdgradePrecision(a, r, out preciseAngle, out preciseRange);
                   //
                   double enemyX, enemyY;
                   ComputePoint(SDK.LocX, SDK.LocY, preciseRange, preciseAngle, out enemyX, out enemyY);

                   //
                   _currentEnemyAngle = preciseAngle;
                   _currentEnemyRange = preciseRange;
                   _currentEnemyX = enemyX;
                   _currentEnemyY = enemyY;

                   UpdateSharedEnemy(_currentEnemyX, _currentEnemyY);

                   //SDK.LogLine("TRACK ENEMY A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

                   found = true;
               }
               return found;
           }

           // Try to get more precision by scanning 1 degree before and 1 degree after with double resolution and weight 1/8
           private void UpdgradePrecision(int a, int r, out double preciseAngle, out double preciseRange)
           {
               preciseAngle = a;
               preciseRange = r;
               if (r > 0)
               {
                   int rBefore = SDK.Scan(a - 1, 2);
                   int rAfter = SDK.Scan(a + 1, 2);
                   if (rBefore > 0)
                   {
                       preciseRange = (7.0 * r + rBefore) / 8.0;
                       preciseAngle = a - 0.125;
                   }
                   else if (rAfter > 0)
                   {
                       preciseRange = (7.0 * r + rAfter) / 8.0;
                       preciseAngle = a + 0.125;
                   }
               }
           }

           // Go to center if speed is 0
           // If single match and target has taken more damage than I, drive full speed on target and fire without range restriction (aka suicide)
           // Else if distance to target is almost max cannon range, lurk around target and fire
           // Else, move pseudo randomly (if near wall, go to center; if too fast, slow down; if being hit recently, anticipate next hit and move to avoid shoot
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
                   //double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
                   //if (distanceToTarget2 > (_maxCannonRange-20) * (_maxCannonRange-20) && distanceToWall > 30)
                   //    Lurk();
                   //else
                   //{
                   //    _dartMode = 0; // reset dart mode
                   //    DrunkWalk();
                   //}
                   double distanceToTarget = Distance(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY);
                   if (distanceToTarget > _maxCannonRange + 50)
                       GoToEnemy();
                   else
                       DrunkWalk();
               }
           }

           private void GoToCenter()
           {
               _goToAngle = Angle(_arenaSize / 2.0, _arenaSize / 2.0, SDK.LocX, SDK.LocY);
               SDK.Drive((int)SDK.Round(_goToAngle), 100);
               SDK.LogLine("Go to center {0:0.00} {1}", _goToAngle, SDK.Speed);
           }

           private void GoToEnemy()
           {
               _goToAngle = _currentEnemyAngle;
               SDK.Drive((int)SDK.Round(_goToAngle), 100);
               SDK.LogLine("Go to enemy {0:0.00} {1}", _currentEnemyAngle, SDK.Speed);
           }

           private void Suicide()
           {
               _goToAngle = _currentEnemyAngle;
               SDK.Drive((int)SDK.Round(_goToAngle), 100);
               _noMinDistanceLimit = true;
               SDK.LogLine("Leeeerooooyyyyyyyyy {0:0.00} {1}", _goToAngle, SDK.Speed);
           }

           private void Lurk()
           {
               // Get enemy position 2 seconds before
               int slot2SecondsBefore = TimeSlot(SDK.Time - 2);
               if (slot2SecondsBefore < 0)
                   slot2SecondsBefore = 0;
               SDK.LogLine("LURKING: Slot:{0}", slot2SecondsBefore);
               double distanceMeToEnemy2SecondsBefore = Distance(SDK.LocX, SDK.LocY, _historyEnemyX[slot2SecondsBefore], _historyEnemyY[slot2SecondsBefore]);
               double angleEnemyMe2SecondsBefore = Angle(_historyEnemyX[slot2SecondsBefore], _historyEnemyY[slot2SecondsBefore], SDK.LocX, SDK.LocY);
               double angleEnemyCenter2SecondsBefore = Angle(_arenaSize/2.0, _arenaSize/2.0, _historyEnemyX[slot2SecondsBefore], _historyEnemyY[slot2SecondsBefore]);
               double angleMeCenter2SecondsBefore = Angle(SDK.LocX, SDK.LocY, _arenaSize / 2.0, _arenaSize / 2.0);

               // If too far from enemy, go to enemy full speed
               if (distanceMeToEnemy2SecondsBefore > 790)
               {
                   SDK.Drive((int)SDK.Round(angleEnemyMe2SecondsBefore), 100);
                   SDK.LogLine("LURKING: far from enemy, full speed to enemy {0:0.00} {1} dist{2}", angleEnemyMe2SecondsBefore, SDK.Speed, distanceMeToEnemy2SecondsBefore);
               }
               else
               {
                   double angle;
                   double multiplier = 1;
                   int centerAngleDiff = FixAngle(FixAngle((int) SDK.Round(angleEnemyCenter2SecondsBefore)) - FixAngle((int) SDK.Round(angleMeCenter2SecondsBefore)));
                   if (centerAngleDiff < 180)
                   {
                       angle = angleEnemyMe2SecondsBefore - 90;
                       multiplier = 1;
                   }
                   else
                   {
                       angle = angleEnemyMe2SecondsBefore + 90;
                       multiplier = -1;
                   }
                   if (SDK.Abs(centerAngleDiff) <= 20)
                       Dart(angleEnemyMe2SecondsBefore, distanceMeToEnemy2SecondsBefore);
                   else if (distanceMeToEnemy2SecondsBefore > LurkDistance)
                   {
                       angle = angle + multiplier*20;
                       SDK.Drive((int)SDK.Round(angle), 50);
                       _hangFire = false;
                       SDK.LogLine("LURKING: > LurkDistance, drive to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                   }
                   else if (distanceMeToEnemy2SecondsBefore < LurkDistance)
                   {
                       angle = angle - multiplier*45;
                       SDK.Drive((int)SDK.Round(angle), 50);
                       _hangFire = false;
                       SDK.LogLine("LURKING: > LurkDistance, drive away enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                   }
                   double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
                   if (distanceToWall < 20)
                   {
                       SDK.Drive((int)SDK.Round(angleEnemyMe2SecondsBefore), 50);
                       SDK.LogLine("LURKING: too close from wall, go to enemy mid-speed {0:0.00} {1}", angleEnemyMe2SecondsBefore, SDK.Speed);
                   }
               }
           }

           private void Dart(double angle, double range)
           {
               switch(_dartMode)
               {
                   case -1:
                       if (SmallestDistanceToWall(SDK.LocX, SDK.LocY) > 20)
                       {
                           angle = angle - 180;
                           SDK.Drive((int) SDK.Round(angle), 50);
                           SDK.LogLine("DART,-1: far from wall, drive opposite to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                       }
                       else
                       {
                           SDK.Drive((int) SDK.Round(angle), 50);
                           SDK.LogLine("DART,-1: too close from wall, drive to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                       }
                       if (range > LurkDistance)
                       {
                           _dartMode = 0;
                           SDK.LogLine("DART,-1: > LurkDistance, switch to mode 0");
                       }
                       _hangFire = false;
                       break;
                   case 1:
                       SDK.Drive((int) SDK.Round(angle), 50);
                       SDK.LogLine("DART,1: drive to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                       if (SDK.Time - _dartStartTime > 0.45 || range < 736)
                       {
                           _hangFire = false;
                           _dartMode = -1;
                           SDK.LogLine("DART,1: > LurkDistance, switch to mode -1");
                       }
                       break;
                   case 0:
                       _hangFire = true;
                       if (SDK.Time - _previousSuccessfulShootTime > 2)
                       {
                           _hangFire = false;
                           SDK.LogLine("DART,0: fire hung for more than 2 seconds, lets fire again");
                       }
                       double distanceToEdge = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
                       if (range > LurkDistance || distanceToEdge < 20)
                       {
                           SDK.Drive((int)SDK.Round(angle), 50);
                           SDK.LogLine("DART,0: drive to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                       }
                       else
                       {
                           angle = angle + 180;
                           SDK.Drive((int) SDK.Round(angle), 50);
                           SDK.LogLine("DART,0: drive opposite to enemy mid-speed {0:0.00} {1}", angle, SDK.Speed);
                       }
                       if (_dartModeDamage > 30 && distanceToEdge > 25)
                       {
                           _hangFire = false;
                           SDK.LogLine("DART,0: taken too much damage in dart mode, stop hanging fire");
                       }
                       else if (SDK.Damage > 90)
                       {
                           _hangFire = false;
                           SDK.LogLine("DART,0: taken too much damage, stop hanging fire");
                       }
                       else
                       {
                           double d1 = SDK.Time - _lastHitTime;
                           double d2 = d1 - (int)d1;
                           if (d2 > 0.003 && d2 < 0.01 && SDK.Time - _previousSuccessfulShootTime > 0.55)
                           {
                               _dartMode = 1;
                               _dartStartTime = SDK.Time;
                           }
                       }
                       break;
               }
           }

           private void DrunkWalk()
           {
               _hangFire = false;
               double distanceToTarget = Distance(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY);
               double timeMultiplier = 1.0; // move less often when far from target
               if (distanceToTarget > 300.0)
                   timeMultiplier = 2.0;
               else if (distanceToTarget > 600.0)
                   timeMultiplier = 3.0;
               double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
               if (SDK.Speed > 50) // Slow down without changing direction
               {
                   if (SDK.Time - _lastDrunkTurnTime < 0.45 * timeMultiplier && distanceToWall > 30.0)
                       return;
                   SDK.Drive((int)SDK.Round(_goToAngle), 50);
                   SDK.LogLine("DRUNK: Slow down {0:0.00} {1}", _goToAngle, SDK.Speed);
                   return;
               }
               if (distanceToWall < 30.0) // escape from wall
               {
                   double diffX = _arenaSize / 2.0 - SDK.LocX;
                   double diffY = _arenaSize / 2.0 - SDK.LocY;
                   _goToAngle = SDK.Rad2Deg(SDK.ATan2(diffY, diffX));
                   SDK.Drive((int)SDK.Round(_goToAngle), 100);
                   _lastDrunkTurnTime = SDK.Time;
                   SDK.LogLine("DRUNK: Too close from wall, go to center full speed {0:0.00} {1}", _goToAngle, SDK.Speed);
                   return;
               }
               // check if being hit or didn't turn too recently
               double t0 = _estimatedEnemyShotTime - (int)_estimatedEnemyShotTime; // [0, 1[
               double t1 = SDK.Time - (int)SDK.Time; // [0, 1[
               double diffT = SDK.Abs(t0 - t1); // estimate when I have to turn to avoid next shoot
               if ((diffT < 0.05 || diffT > 0.95) && SDK.Time - _lastDrunkTurnTime > 0.5)
               {
                   int i = SDK.Rand(2);
                   if (i == 0)
                   {
                       _goToAngle += 90.0;
                       SDK.LogLine("DRUNK: Turning +1/4 full speed {0:0.00} {1}", _goToAngle, SDK.Speed);
                   }
                   else
                   {
                       _goToAngle -= 90.0;
                       SDK.LogLine("DRUNK: Turning -1/4 full speed {0:0.00} {1}", _goToAngle, SDK.Speed);
                   }
                   SDK.Drive((int)SDK.Round(_goToAngle), 100);
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

           private void RegisterHistory(double time, double locX, double locY)
           {
               int timeSlot = TimeSlot(time);
               if (_historyEnemyX[timeSlot] < 0) // register only one information / timeslot
               {
                   _historyEnemyX[timeSlot] = locX;
                   _historyEnemyY[timeSlot] = locY;
                   SDK.LogLine("UPDATING HISTORY {0} slot {1} {2:0.0000} {3:0.0000} distance: {4:0.0000}", _id, timeSlot, locX, locY, Distance(SDK.LocX, SDK.LocY, locX, locY));
               }
           }

           private void UpdateSharedLocation(int locX, int locY, int damage)
           {
               //SDK.LogLine("UPDATING INFO {0} {1} {2} {3}", _id, locX, locY, damage);
               TeamLocX[_id] = locX;
               TeamLocY[_id] = locY;
               TeamDamage[_id] = damage;
           }

           private void UpdateSharedEnemy(double locX, double locY)
           {
               TeamEnemyX[_id] = locX;
               TeamEnemyY[_id] = locY;
           }

           private bool IsFriendlyTarget(double locX, double locY)
           {
               if (_teamCount > 1)
               {
                   //SDK.LogLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                   for (int i = 0; i < _teamCount; i++)
                       if (i != _id && TeamDamage[i] < _maxDamage) // don't consider myself or dead teammate
                       {
                           //SDK.LogLine("TEAM MEMBER {0} : {1}, {2}", i, TeamLocX[i], TeamLocY[i]);
                           double distance2 = DistanceSquared(locX, locY, TeamLocX[i], TeamLocY[i]);
                           if (distance2 < FriendRangeSquared)
                               return true;
                       }
               }
               return false;
           }

           private int CountAliveFriends()
           {
               int count = 0;
               for (int i = 0; i < _teamCount; i++)
                   if (i != _id && TeamDamage[i] < _maxDamage)
                       count++;
               return count;
           }

           private int FixAngle(int angle)
           {
               while (angle < 0)
                   angle += 360;
               angle %= 360;
               return angle;
           }

           private int TimeSlot(double time)
           {
               return (int) (time/TrackFindTimeSlot);
           }

           private double Angle(double x1, double y1, double x2, double y2)
           {
               double diffX = x1 - y2;
               double diffY = y1 - y2;
               return SDK.Rad2Deg(SDK.ATan2(diffY, diffX));
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

               //SDK.LogLine("{0:0.00} - estimated position at {1:0.00} {2:0.0000} {3:0.0000}", SDK.Time, SDK.Time + t, pX, pY);

               //SDK.LogLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
           }
       }
       */
    /* v3 2014-07-15
    public class SinaC : Robot
    {
        public const bool Move = true;
        public const bool UpgradedPrecision = true;
        public const double Epsilon = 0.00001;

        public const int BorderSize = 30;
        public const double FriendRangeSquared = 40*40;

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
            SDK.LogLine("{0:HH:mm:ss.fff} - Init", DateTime.Now);

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
                SDK.LogLine("Damage detected: hit time {0} estimated shoot time {1}", _lastHitTime, _estimatedEnemyShotTime);
            }

            if (Move)
                MoveSmartly();

            //if (Move)
            //    AvoidBorders();

            //if (elapsedTime > 2 ) // 2 seconds since last move
            //{
            //    //SDK.LogLine("{0:HH:mm:ss.fff} - MOVE RANDOMLY FROM LOCATION {1} : {2},{3}", DateTime.Now, _id, SDK.LocX, SDK.LocY);

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
            SDK.LogLine("{0:0.00} - Nearest enemy of {1}: A:{2:0.0000} R:{3:0.0000} {4:0.0000},{5:0.0000}", SDK.Time, _id, _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

            double currentSpeedX, currentSpeedY;
            DifferenceRelativeToTime(elapsedTime, _previousEnemyX, _previousEnemyY, _currentEnemyX, _currentEnemyY, out currentSpeedX, out currentSpeedY);

            double currentAccelerationX, currentAccelerationY;
            DifferenceRelativeToTime(elapsedTime, _previousEnemySpeedX, _previousEnemySpeedY, currentSpeedX, currentSpeedY, out currentAccelerationX, out currentAccelerationY);

            //SDK.LogLine("{0:0.00} - TICK:{1:0.00} | Enemy position: {2:0.0000}, {3:0.0000} Speed : {4:0.0000}, {5:0.0000} | range {6} angle {7}", DateTime.Now, SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);
            SDK.LogLine("{0:0.00} - estimated speed {1:0.0000} {2:0.0000} acceleration {3:0.0000} {4:0.0000}", SDK.Time, currentSpeedX, currentSpeedY, currentAccelerationX, currentAccelerationY);

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
                        //Cheat.FindNearestEnemy(out cheatAngle, out cheatRange, out cheatX, out cheatY);

                        _currentEnemyAngle = preciseAngle;
                        _currentEnemyRange = preciseRange;
                        _currentEnemyX = enemyX;
                        _currentEnemyY = enemyY;

                        //SDK.LogLine(" CHEAT A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", cheatAngle, cheatRange, cheatX, cheatY);
                        //SDK.LogLine("NORMAL A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}  RAW A:{4:0.0000} R:{5:0.0000} X:{6:0.0000} Y:{7:0.0000}", angle, range, enemyX, enemyY, a, r, rawEnemyX, rawEnemyY);
                        SDK.LogLine("NORMAL A:{0:0.0000} R:{1:0.0000} X:{2:0.0000} Y:{3:0.0000}", _currentEnemyAngle, _currentEnemyRange, _currentEnemyX, _currentEnemyY);

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
        // Else, move pseudo randomly (if near wall, go to center; if too fast, slow down; if being hit recently, anticipate next hit and move to avoid shoot
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
                //double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
                //if (distanceToTarget2 > _maxCannonRange*_maxCannonRange && distanceToWall > 30)
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
            SDK.Drive((int)_goToAngle, 100);
            SDK.LogLine("Go to center {0:0.00} {1}", _goToAngle, SDK.Speed);
        }

        private void Suicide()
        {
            _goToAngle = _currentEnemyAngle;
            SDK.Drive((int)_goToAngle, 100);
            _noMinDistanceLimit = true;
            SDK.LogLine("Leeeerooooyyyyyyyyy {0:0.00} {1}", _goToAngle, SDK.Speed);
        }

        private void Lurk()
        {
            SDK.LogLine("Lurking around target");
        }

        private void DrunkWalk()
        {
            double distanceToTarget = Distance(SDK.LocX, SDK.LocY, _currentEnemyX, _currentEnemyY);
            double timeMultiplier = 1.0; // move less often when far from target
            if (distanceToTarget > 300.0)
                timeMultiplier = 2.0;
            else if (distanceToTarget > 600.0)
                timeMultiplier = 3.0;
            double distanceToWall = SmallestDistanceToWall(SDK.LocX, SDK.LocY);
            if (SDK.Speed > 50) // Slow down without changing direction
            {
                if (SDK.Time - _lastDrunkTurnTime < 0.45 * timeMultiplier && distanceToWall > 30.0)
                    return;
                SDK.Drive((int)_goToAngle, 50);
                SDK.LogLine("DRUNK: Slow down {0:0.00} {1}", _goToAngle, SDK.Speed);
                return;
            }
            if (distanceToWall < 30.0) // escape from wall
            {
                double diffX = _arenaSize / 2.0 - SDK.LocX;
                double diffY = _arenaSize / 2.0 - SDK.LocY;
                _goToAngle = SDK.Rad2Deg(SDK.ATan2(diffY, diffX));
                SDK.Drive((int)_goToAngle, 100);
                _lastDrunkTurnTime = SDK.Time;
                SDK.LogLine("DRUNK: Too close from wall, go to center {0:0.00} {1}", _goToAngle, SDK.Speed);
                return;
            }
            // check if being hit or didn't turn too recently
            double t0 = _estimatedEnemyShotTime - (int)_estimatedEnemyShotTime; // [0, 1[
            double t1 = SDK.Time - (int)SDK.Time; // [0, 1[
            double diffT = SDK.Abs(t0 - t1); // estimate when I have to turn to avoid next shoot
            if ((diffT < 0.05 || diffT > 0.95) && SDK.Time - _lastDrunkTurnTime > 0.5)
            {
                int i = SDK.Rand(2);
                if (i == 0)
                {
                    _goToAngle += 90.0;
                    SDK.LogLine("DRUNK: Turning +1/4 {0:0.00} {1}", _goToAngle, SDK.Speed);
                }
                else
                {
                    _goToAngle -= 90.0;
                    SDK.LogLine("DRUNK: Turning -1/4 {0:0.00} {1}", _goToAngle, SDK.Speed);
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
            //SDK.LogLine("UPDATING INFO {0} {1} {2} {3}", _id, locX, locY, damage);
            TeamLocX[_id] = locX;
            TeamLocY[_id] = locY;
            TeamDamage[_id] = damage;
        }

        private bool IsFriendlyTarget(double locX, double locY)
        {
            if (_teamCount > 1)
            {
                //SDK.LogLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                for (int i = 0; i < _teamCount; i++)
                    if (i != _id && TeamDamage[i] < _maxDamage) // don't consider myself or dead teammate
                    {
                        //SDK.LogLine("TEAM MEMBER {0} : {1}, {2}", i, TeamLocX[i], TeamLocY[i]);
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

            SDK.LogLine("{0:0.00} - estimated position at {1:0.00} {2:0.0000} {3:0.0000}", SDK.Time, SDK.Time + t, pX, pY);

            //SDK.LogLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
        }
    }
    */
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
                SDK.LogLine("{0:HH:mm:ss.fff} - SINAC - Init", DateTime.Now);

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
                    SDK.LogLine("{0:HH:mm:ss.fff} - SINAC - MOVE RANDOMLY FROM LOCATION {1} : {2},{3}  Damaged:{4}", DateTime.Now, _id, SDK.LocX, SDK.LocY, currentDamage > _previousDamage);

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
                    //SDK.LogLine("{0:HH:mm:ss.fff} - SINAC - Nearest enemy of {1}: A{2} R{3} {4:0.0000},{5:0.0000}", DateTime.Now,, _id, enemyAngle, enemyRange, enemyX, enemyY);

                    double currentSpeedX, currentSpeedY;
                    ComputeSpeed(elapsedTime, _previousEnemyX, _previousEnemyY, enemyX, enemyY, out currentSpeedX, out currentSpeedY);

                    //SDK.LogLine("{0:HH:mm:ss.fff} - SINAC - TICK:{1:0.00} | Enemy position: {2:0.0000}, {3:0.0000} Speed : {4:0.0000}, {5:0.0000} | range {6} angle {7}", DateTime.Now, SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

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
                    //SDK.LogLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                    for (int i = 0; i < _teamCount; i++)
                        if (i != _id && TeamDamage[i] < _maxDamage)
                        {
                            //SDK.LogLine("TEAM MEMBER {0} : {1},{2}", i, TeamLocX[i], TeamLocY[i]);
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

                //SDK.LogLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
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
                    SDK.LogLine("LOCATION {0} : {1},{2}", _id, SDK.LocX, SDK.LocY);

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

                    //SDK.LogLine("TICK:{0:0.00} | Enemy position: {1:0.0000}, {2:0.0000} Speed : {3:0.0000}, {4:0.0000} | range {5} angle {6}", SDK.Time, currentEnemyX, currentEnemyY, currentSpeedX, currentSpeedY, currentRange, currentAngle);

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
                //SDK.LogLine("Nearest enemy of {0}: A{1} R{2} {3:0.0000},{4:0.0000} found:{5}", _id, enemyAngle, enemyRange, enemyX, enemyY, found);

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
                            //SDK.LogLine("Enemy of {0}: A{1} R{2} {3:0.0000},{4:0.0000}", _id, angle, range, targetX, targetY);

                            //if (SDK.Abs(range-enemyRange) > 5 && SDK.Abs(angle - enemyAngle) > 5)
                            //    SDK.LogLine("***********************************************************");
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
                    //SDK.LogLine("CHECKING IF FRIENDLY TARGET {0} - {1}, {2}", _id, locX, locY);
                    for (int i = 0; i < _teamCount; i++)
                        if (i != _id && TeamDamage[i] < _maxDamage)
                        {
                            //SDK.LogLine("TEAM MEMBER {0} : {1},{2}", i, TeamLocX[i], TeamLocY[i]);
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

                //SDK.LogLine("t: {0:0.0000} (global: {1:0.0000} new enemy position: {2:0.0000}, {3:0.0000}  angle {4:0} range {5:0}", t, SDK.Time + t, pX, pY, angle, range);
            }
        }
     */
}
