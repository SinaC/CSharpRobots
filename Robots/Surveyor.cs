namespace Robots
{
    // Measurement robot
    public class Surveyor : SDK.Robot
    {
        private double _initialTime;
        private int _initX;
        private int _initY;
        private double _lastTime;
        private double _lastX;
        private double _lastY;
        private double _lastSpeedX;
        private double _lastSpeedY;
        private int _lastDamage;
        private int _previousSpeed;

        private int _driveAngle;
        private double _lastTurn;
        private double _lastHit;
        private bool _hit;
        private int _turnCount;
        private int _sign;

        public override void Init()
        {
            InitZigzagMove();
        }

        public override void Step()
        {
            StepZigzagMove();
        }

        #region Zigzag move

        private void InitZigzagMove()
        {
            _driveAngle = SDK.Rand(360);
            SDK.Drive(_driveAngle, 100);
            _lastHit = 100000; // arbitrary big value
            _hit = false;
            _lastTurn = SDK.Time;
            _lastDamage = SDK.Damage;
            _sign = +1;
            _turnCount = 0;
        }

        private void StepZigzagMove()
        {
            // big range: really good against linear interpolation and average against direct fire
            // small range: average
            if (SDK.Damage > _lastDamage)
            {
                SDK.LogLine("HIT: {0}", SDK.Damage);
                _lastHit = SDK.Time;
                _lastDamage = SDK.Damage;
                _hit = true;
            }

            // half-second after being hit, we have to change direction
            if ((SDK.Time - _lastHit > 0.5 && _hit) ||  SDK.Time - _lastTurn > 1.5)
            {
                _turnCount = (_turnCount + 1) % 4;
                if (_turnCount == 0)
                {
                    _sign = -_sign;
                    _driveAngle += 210 * _sign;
                }
                else
                    _driveAngle += 70*_sign;
                SDK.Drive(_driveAngle, 50);
                _lastTurn = SDK.Time;
                _hit = false;
            }
        }

        #endregion

        #region Circular move

        private void InitCircularMove()
        {
            Cheat.Teleport(500, 500);
            _driveAngle = 0;
            SDK.Drive(_driveAngle, 50);
            _lastTime = SDK.Time;
            //SDK.Drive(180, 100);
        }

        private void StepCircularMove()
        {
            // big range: really good against linear interpolation and below average against direct fire
            // small range: average
            Cheat.SetDamage(0);
            double currentTime = SDK.Time;
            if (currentTime - _lastTime > 0.5)
                SDK.Drive(_driveAngle, 50);
            if (currentTime - _lastTime > 1 && SDK.Speed <= 50)
            {
                _driveAngle += 75;
                SDK.Drive(_driveAngle, 100);
                _lastTime = SDK.Time;
            }
        }

        #endregion


        #region Speed test

        public void InitSpeedTest()
        {
            _initialTime = SDK.Time;
            _initX = SDK.LocX;
            _initY = SDK.LocY;

            _lastTime = _initialTime;
            _lastX = _initX;
            _lastY = _initY;
            _lastSpeedX = 0;
            _lastSpeedY = 0;
            _lastDamage = 0;
            _previousSpeed = 0;

            //
            SDK.Drive(0, 100);
        }

        public void StepSpeedTest()
        {
            double currentTime = SDK.Time;
            int currentX = SDK.LocX;
            int currentY = SDK.LocY;
            int currentSpeed = SDK.Speed;
            int currentDamage = SDK.Damage;

            //SDK.LogLine("Speed X:{0:0.0000} Y:{1:0.0000} - Time {2:0.0000}", speedX, speedY, diffTime);

            double diffLastTime = currentTime - _lastTime;
            if (diffLastTime > 1)// || _lastDamage < currentDamage)
            {
                double diffTime = currentTime - _initialTime;

                double diffX = currentX - _initX;
                double diffY = currentY - _initY;
                double speedX = diffX / diffTime;
                double speedY = diffY / diffTime;

                double actualSpeedX = (currentX - _lastX) / diffLastTime;
                double actualSpeedY = (currentY - _lastY) / diffLastTime;

                double actualAccelerationX = (actualSpeedX - _lastSpeedX) / diffLastTime;
                double actualAccelerationY = (actualSpeedY - _lastSpeedY) / diffLastTime;

                int diffSpeed = currentSpeed - _previousSpeed;
                double acceleration = diffSpeed / diffLastTime;

                SDK.LogLine("TICK:{0:0.00} | Loc:{1},{2} | Instant speed X:{3:0.00} Y:{4:0.00} - Elapsed {5:0.00}  diff X:{6:0.00} Y:{7:0.00}  Dmg:{8} acceleration:{9:0.00}  {10:0.0000} {11:0.0000}", SDK.Time, currentX, currentY, actualSpeedX, actualSpeedY, diffTime, diffX, diffY, SDK.Damage, acceleration, actualAccelerationX, actualAccelerationY);

                _lastTime = currentTime;
                _lastX = currentX;
                _lastY = currentY;
                _lastSpeedX = actualSpeedX;
                _lastSpeedY = actualSpeedY;
                _previousSpeed = currentSpeed;
                _lastDamage = currentDamage;
            }
        }

        #endregion
    }
}