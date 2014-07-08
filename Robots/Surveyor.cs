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
        private int _lastDamage;
        private int _previousSpeed;

        public override void Init()
        {
            _initialTime = SDK.Time;
            _initX = SDK.LocX;
            _initY = SDK.LocY;

            _lastTime = _initialTime;
            _lastX = _initX;
            _lastY = _initY;
            _lastDamage = 0;
            _previousSpeed = 0;

            //
            SDK.Drive(0, 100);
        }

        public override void Step()
        {
            double currentTime = SDK.Time;
            int currentX = SDK.LocX;
            int currentY = SDK.LocY;
            int currentSpeed = SDK.Speed;
            int currentDamage = SDK.Damage;

            //System.Diagnostics.Debug.WriteLine("Speed X:{0:0.0000} Y:{1:0.0000} - Time {2:0.0000}", speedX, speedY, diffTime);

            double diffLastTime = currentTime - _lastTime;
            if (diffLastTime > 1 || _lastDamage < currentDamage)
            {
                double diffTime = currentTime - _initialTime;

                double diffX = currentX - _initX;
                double diffY = currentY - _initY;
                double speedX = diffX/diffTime;
                double speedY = diffY/diffTime;

                double actualSpeedX = (currentX - _lastX)/diffLastTime;
                double actualSpeedY = (currentY - _lastY)/diffLastTime;

                int diffSpeed = currentSpeed - _previousSpeed;
                double acceleration = diffSpeed/diffLastTime;

                System.Diagnostics.Debug.WriteLine("TICK:{0:0.00} | Loc:{1},{2} | Instant speed X:{3:0.00} Y:{4:0.00} - Elapsed {5:0.00}  diff X:{6:0.00} Y:{7:0.00}  Dmg:{8} acceleration:{9:0.00}", SDK.Time, currentX, currentY, actualSpeedX, actualSpeedY, diffTime, diffX, diffY, SDK.Damage, acceleration);

                _lastTime = currentTime;
                _lastX = currentX;
                _lastY = currentY;
                _previousSpeed = currentSpeed;
                _lastDamage = currentDamage;
            }
        }
    }
}