using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
    public class Ciaky : SDK.Robot
    {
        const int K = 100000;
        static int[] goals_x = { 100, 900, 100, 900, 100, 900 };
        static int[] goals_y = { 100, 100, 500, 900, 900, 500 };
        static int[] locs_x = new int[8];
        static int[] locs_y = new int[8];
        static int num;
        static int lastGoal;
        int goal;
        int direction;
        int vel;
        int target_x;
        int target_y;
        int lastTarget_x;
        int lastTarget_y;
        int lastDamage;
        double lastFired;

        public override void Init()
        {
            int i;
            /*  24:    */
            int j;
            /*  25: 24 */
            if (SDK.Id == 0)
            /*  26:    */
            {
                /*  27: 26 */
                num = 1;
                /*  28: 27 */
                i = dist(SDK.LocX, SDK.LocY, goals_x[0], goals_y[0]);
                /*  29: 28 */
                this.goal = 0;
                /*  30: 29 */
                for (j = 1; j < goals_x.Length; j++)
                {
                    /*  31: 31 */
                    if (i < dist(SDK.LocX, SDK.LocY, goals_x[j], goals_y[j]))
                    /*  32:    */
                    {
                        /*  33: 33 */
                        i = dist(SDK.LocX, SDK.LocY, goals_x[j], goals_y[j]);
                        /*  34: 34 */
                        this.goal = j;
                        /*  35:    */
                    }
                    /*  36:    */
                }
                /*  37: 37 */
                lastGoal = this.goal;
                /*  38:    */
            }
            /*  39:    */     else
            /*  40:    */
            {
                /*  41: 41 */
                this.goal = ((lastGoal + 1) % goals_x.Length);
                /*  42: 42 */
                lastGoal = this.goal;
                /*  43: 43 */
                num += 1;
                /*  44:    */
            }
            /*  45: 46 */
            scan360();
        }

        public override void Step()
        {
            int i, j;
            locs_x[SDK.Id] = SDK.LocX;
            /*  49: 51 */
            locs_y[SDK.Id] = SDK.LocY;
            /*  50: 52 */
            if (targetFound())
            /*  51:    */
            {
                /*  52: 54 */
                i = dir(SDK.LocX, SDK.LocY, this.target_x, this.target_y);
                /*  53: 55 */
                j = dist(SDK.LocX, SDK.LocY, this.target_x, this.target_y);
                /*  54: 56 */
                scanTarget(i, j);
                /*  55: 57 */
                if (SDK.Time - this.lastFired >= 1.0D)
                /*  56:    */
                {
                    /*  57: 59 */
                    double d1 = (this.target_x - this.lastTarget_x) / (SDK.Time - this.lastFired);
                    /*  58: 60 */
                    double d2 = (this.target_y - this.lastTarget_y) / (SDK.Time - this.lastFired);
                    /*  59: 61 */
                    j = dist(SDK.LocX, SDK.LocY, this.target_x, this.target_y);
                    /*  60: 62 */
                    int k = this.target_x + (int)(d1 * j + 0.5D) / 300;
                    /*  61: 63 */
                    int m = this.target_y + (int)(d2 * j + 0.5D) / 300;
                    /*  62: 64 */
                    i = dir(SDK.LocX, SDK.LocY, k, m);
                    /*  63: 65 */
                    j = dist(SDK.LocX, SDK.LocY, k, m);
                    /*  64: 66 */
                    SDK.Cannon(i, j);
                    /*  65: 67 */
                    this.lastTarget_x = this.target_x;
                    /*  66: 68 */
                    this.lastTarget_y = this.target_y;
                    /*  67: 69 */
                    this.lastFired = SDK.Time;
                    /*  68:    */
                }
                /*  69:    */
            }
            /*  70:    */       else
            /*  71:    */
            {
                /*  72: 74 */
                scan360();
                /*  73:    */
            }
            /*  74: 77 */
            if (dist(SDK.LocX, SDK.LocY, goals_x[this.goal], goals_y[this.goal]) > 100)
            {
                /*  75: 79 */
                this.vel = 100;
                /*  76:    */
            }
            else
            {
                /*  77: 83 */
                this.vel = 50;
                /*  78:    */
            }
            /*  79: 85 */
            if (dist(SDK.LocX, SDK.LocY, goals_x[this.goal], goals_y[this.goal]) < 20)
            {
                /*  80: 87 */
                this.goal = ((this.goal + 1) % goals_x.Length);
                /*  81:    */
            }
            /*  82: 89 */
            this.direction = route();
            /*  83: 90 */
            SDK.Drive(this.direction, this.vel);
        }

        private bool hit()
        {
            bool b = this.lastDamage == SDK.Damage;
            this.lastDamage = SDK.Damage;
            return b;
        }

        void scan360()
        {
            int i = 0;
            while (i < 360)
            {
                int j = SDK.Scan(i, 10);
                if ((j != 0) && (j < 750))
                {
                    this.target_x = x(SDK.LocX, SDK.LocY, i, j);
                    this.target_y = y(SDK.LocX, SDK.LocY, i, j);
                    if (!friend(this.target_x, this.target_y))
                    {
                        return;
                    }
                }
                i += 10;
            }
            this.target_x = -1;
            this.target_y = -1;
        }

        void scanTarget(int paramInt1, int paramInt2)
        {
            int i = paramInt1 - 5;
            int j = paramInt2 > 200 ? 1 : 4;
            while (i <= paramInt1 + 5)
            {
                int k = SDK.Scan(i, j);
                if ((k != 0) && (k < 750))
                {
                    this.target_x = x(SDK.LocX, SDK.LocY, i, k);
                    this.target_y = y(SDK.LocX, SDK.LocY, i, k);
                    if (!friend(this.target_x, this.target_y))
                    {
                        return;
                    }
                }
                i += j;
            }
            this.target_x = -1;
            this.target_y = -1;
        }

        bool targetFound()
        {
            return (this.target_x >= 0) && (this.target_y >= 0);
        }

        int route()
        {
            int i = dir(SDK.LocX, SDK.LocY, goals_x[this.goal], goals_y[this.goal]);
            return i;
        }

        int dir(int paramInt1, int paramInt2, int paramInt3, int paramInt4)
        {
            int i;
            if (paramInt1 == paramInt3)
            {
                i = 90;
                if (paramInt4 < paramInt2)
                {
                    i += 180;
                }
            }
            else
            {
                i = SDK.ATan((paramInt4 - paramInt2) * 100000 / (paramInt3 - paramInt1));
                if (paramInt3 < paramInt1)
                {
                    i += 180;
                }
            }
            return i;
        }

        int dist(int paramInt1, int paramInt2, int paramInt3, int paramInt4)
        {
            int i = paramInt1 - paramInt3;
            int j = paramInt2 - paramInt4;
            return SDK.Sqrt(i * i + j * j);
        }

        int x(int paramInt1, int paramInt2, int paramInt3, int paramInt4)
        {
            return paramInt1 + SDK.Cos(paramInt3) * paramInt4 / 100000;
        }

        int y(int paramInt1, int paramInt2, int paramInt3, int paramInt4)
        {
            return paramInt2 + SDK.Sin(paramInt3) * paramInt4 / 100000;
        }

        bool friend(int paramInt1, int paramInt2)
        {
            for (int i = 0; i < num; i++)
            {
                if (dist(paramInt1, paramInt2, locs_x[i], locs_y[i]) < 40)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
