using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    internal interface IBattlefield
    {
        int Cannon(Robot robot, int degrees, int range);
        int Drive(int degrees, int speed);
        int Scan(int degrees, int resolution);
    }
}
