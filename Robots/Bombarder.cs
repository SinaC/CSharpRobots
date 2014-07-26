using SDK;

namespace Robots
{
    public class Bombarder : Robot
    {
        private const double MissileSpeed = 300;

        private double _lastFire;
        private double _lastEnemyX;
        private double _lastEnemyY;
        private double _currentEnemyX;
        private double _currentEnemyY;
        private double _currentEnemySpeedX;
        private double _currentEnemySpeedY;
        private double _fireEnemyX;
        private double _fireEnemyY;

        public override void Init()
        {
            // NOP
            Cheat.Teleport(400, 400);

            double degrees, range;
            Cheat.FindNearestEnemy(out degrees, out range, out _currentEnemyX, out _currentEnemyY);

            _lastEnemyX = _currentEnemyX;
            _lastEnemyY = _currentEnemyY;
            _lastFire = SDK.Time;
        }

        public override void Step()
        {
            if (SDK.Time - _lastFire >= 1)
            {
                double degrees, range;
                Cheat.FindNearestEnemy(out degrees, out range, out _currentEnemyX, out _currentEnemyY);
                Interpolate(SDK.Time - _lastFire);
                Cheat.FireAt(_fireEnemyX, _fireEnemyY);
                _lastFire = SDK.Time;
                _lastEnemyX = _currentEnemyX;
                _lastEnemyY = _currentEnemyY;
            }
        }

        private void Interpolate(double dt)
        {
            DifferenceRelativeToTime(dt, _lastEnemyX, _lastEnemyY, _currentEnemyX, _currentEnemyY, out _currentEnemySpeedX, out _currentEnemySpeedY);

            //SDK.LogLine("Enemy speed: {0:0.0000} {1:0.0000}", _currentEnemySpeedX, _currentEnemySpeedY);

            double dX = _currentEnemyX - SDK.LocX;
            double dY = _currentEnemyY - SDK.LocY;
            double t = SDK.Sqrt(MissileSpeed * MissileSpeed * (dX * dX + dY * dY) - (dX * _currentEnemySpeedY - dY * _currentEnemySpeedX) * (dX * _currentEnemySpeedY - dY * _currentEnemySpeedX) + 0.5) / (MissileSpeed * MissileSpeed - (_currentEnemySpeedX * _currentEnemySpeedX + _currentEnemySpeedY * _currentEnemySpeedY));
            _fireEnemyX = _currentEnemyX + _currentEnemySpeedX * t;
            _fireEnemyY = _currentEnemyY + _currentEnemySpeedY * t;
        }

        private void Direct(double dt)
        {
            _fireEnemyX = _currentEnemyX;
            _fireEnemyY = _currentEnemyY;
        }

        private static void DifferenceRelativeToTime(double elapsed, double previousX, double previousY, double currentX, double currentY, out double relativeX, out double relativeY)
        {
            relativeX = (currentX - previousX) / elapsed;
            relativeY = (currentY - previousY) / elapsed;
        }
    }
}
