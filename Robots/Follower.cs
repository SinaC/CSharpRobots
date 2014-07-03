using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
    public class Follower : SDK.Robot
    {
        public override void Main()
        {
            while (true)
            {
                int angle, range;
                bool targetFound = FindTarget(10, out angle, out range);
                if (targetFound)
                    SDK.Drive(angle, 40);
                else
                    SDK.Drive(0, 40);
            }
        }
        private bool FindTarget(int resolution, out int angle, out int range)
        {
            angle = 0;
            range = 0;
            for (int step = 0; step < 360; step += resolution)
            {
                int r = SDK.Scan(step, resolution);
                if (r > 0)
                {
                    range = r;
                    angle = step + resolution/2;
                    return true;
                }
            }
            return false;
        }
    }
}
