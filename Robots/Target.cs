using SDK;

namespace Robots
{
    // Target just sits there
    public class Target : Robot
    {
        public override void Main()
        {
            System.Diagnostics.Debug.WriteLine("Target: my position {0} {1}", SDK.LocX, SDK.LocY);
            while (true)
                ;
        }
    }
}
