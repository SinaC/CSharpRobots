using System.Threading;
using SDK;

namespace Robots
{
    // Target just sits there
    public class Target : Robot
    {
        public override string Name { get { return "Target"; } }

        public override void Main()
        {
            System.Diagnostics.Debug.WriteLine("Target: my position {0} {1}", SDK.LocX, SDK.LocY);
            while (true)
                Thread.Sleep(10);
        }
    }
}
