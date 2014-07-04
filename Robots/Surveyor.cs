namespace Robots
{
    public class Surveyor : SDK.Robot
    {
        public override void Main()
        {
            double initialTime = SDK.Time;
            int initX = SDK.LocX;
            int initY = SDK.LocY;

            double lastTime = initialTime;
            double lastX = initX;
            double lastY = initY;

            int previousSpeed = 0;

            SDK.Drive(0, 100);

            while(true)
            {
                double currentTime = SDK.Time;
                int currentX = SDK.LocX;
                int currentY = SDK.LocY;
                int currentSpeed = SDK.Speed;

                //System.Diagnostics.Debug.WriteLine("Speed X:{0:0.0000} Y:{1:0.0000} - Time {2:0.0000}", speedX, speedY, diffTime);

                double diffLastTime = currentTime - lastTime;
                if (diffLastTime > 1 || SDK.Damage > 0)
                {
                    double diffTime = currentTime - initialTime;

                    double diffX = currentX - initX;
                    double diffY = currentY - initY;
                    double speedX = diffX / diffTime;
                    double speedY = diffY / diffTime;
                    
                    double actualSpeedX = (currentX - lastX) / diffLastTime;
                    double actualSpeedY = (currentY - lastY) / diffLastTime;

                    int diffSpeed = currentSpeed - previousSpeed;
                    double acceleration = diffSpeed / diffLastTime;

                    System.Diagnostics.Debug.WriteLine("Instant speed X:{0:0.00} Y:{1:0.00} - Time {2:0.00}  diff X:{3:0.00} Y:{4:0.00}  Dmg:{5}", actualSpeedX, actualSpeedY, diffTime, diffX, diffY, SDK.Damage);
                    System.Diagnostics.Debug.WriteLine("Acceleration {0:0.0000}", acceleration);

                    lastTime = currentTime;
                    lastX = currentX;
                    lastY = currentY;
                    previousSpeed = currentSpeed;
                }
            }
        }
    }
}
