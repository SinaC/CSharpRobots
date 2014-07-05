namespace Robots
{
    public class CrazyCannon : SDK.Robot
    {
        public override string Name { get { return "CrazyCannon"; } }

        public override void Main()
        {
            while(true)
            {
                int degrees = SDK.Rand(360);
                int range = 100 + SDK.Rand(600);
                if (1 == SDK.Cannon(degrees, range))
                {
                    System.Diagnostics.Debug.WriteLine("SHOOTING {0} | {1}", degrees, range);
                }
            }
        }
    }
}
