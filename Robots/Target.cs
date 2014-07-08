using SDK;

namespace Robots
{
    // Target just sits there
    public class Target : Robot
    {
        public override void Init()
        {
            System.Diagnostics.Debug.WriteLine("Target: my position {0} {1}", SDK.LocX, SDK.LocY);
        }

        public override void Step()
        {
            // NOP
        }
    }
}