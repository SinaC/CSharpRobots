namespace Robots
{
    // Found on JJRobots
    public class Maruko4 : SDK.Robot
    {
        public override string Name
        {
            get { return "Maruko4"; }
        }

        private const int MSPD = 300;
        private const int SCALE = 100000;
        private const double GSTGAP = 1.0D;
        private const int X = 0;
        private const int Y = 1;
        private int tdeg;
        private int tdist;
        private int tx;
        private int ty;
        private int gsoldtx;
        private int gsoldty;
        private int tsx;
        private int tsy;
        private int mydeg;
        private int myspd = 49;
        private double gsoldt;
        private int myid;
        private static int[][] friends = {new int[2], new int[2], new int[2], new int[2], new int[2], new int[2], new int[2], new int[2]};
        private static int totalfriends;

        public override void Main()
        {
            this.myid = SDK.Id;
            int i = this.myid;
            int j = SDK.LocX;
            int k = SDK.LocY;
            friends[i][0] = j;
            friends[i][1] = k;
            radarscan2();
            this.mydeg = ((this.tdeg + 180)%360);
            SDK.Drive(this.mydeg, 100);
            this.tx = ((int) (SDK.LocX + this.tdist*(SDK.Cos(this.tdeg)/100000.0D)));
            this.tx = ((int) (SDK.LocY + this.tdist*(SDK.Sin(this.tdeg)/100000.0D)));
            for (;;)
            {
                smartfire();
                smartmove();
            }
        }

        private int qdist(int paramInt1, int paramInt2, int paramInt3, int paramInt4)
        {
            return (paramInt3 - paramInt1)*(paramInt3 - paramInt1) + (paramInt4 - paramInt2)*(paramInt4 - paramInt2);
        }

        private bool isfriend(int paramInt1, int paramInt2)
        {
            for (int k = 0; k < 8; k++)
            {
                if (k != this.myid)
                {
                    int i = friends[k][0];
                    int j = friends[k][1];
                    if ((i - paramInt1)*(i - paramInt1) + (j - paramInt2)*(j - paramInt2) < 2500)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int ploc_x(int paramInt1, int paramInt2)
        {
            return (int) (SDK.LocX + paramInt2*(SDK.Cos(paramInt1)/100000.0D));
        }

        private int ploc_y(int paramInt1, int paramInt2)
        {
            return (int) (SDK.LocY + paramInt2*(SDK.Sin(paramInt1)/100000.0D));
        }

        private int tlocx()
        {
            return (int) (SDK.LocX + this.tdist*(SDK.Cos(this.tdeg)/100000.0D));
        }

        private int tlocy()
        {
            return (int) (SDK.LocY + this.tdist*(SDK.Sin(this.tdeg)/100000.0D));
        }

        private void radarscan(int paramInt)
        {
            int i = 0;
            int j = -1;
            this.tdeg = -1;
            this.tdist = 9999;
            while (i <= 360)
            {
                j = SDK.Scan(i, paramInt);
                if ((j > 0) && (j < this.tdist))
                {
                    int k = (int) (SDK.LocX + j*(SDK.Cos(i)/100000.0D));
                    int m = (int) (SDK.LocY + j*(SDK.Sin(i)/100000.0D));
                    if (isfriend(k, m) == false)
                    {
                        this.tdeg = i;
                        this.tdist = j;
                    }
                }
                i += paramInt;
            }
        }

        private void radarscan2()
        {
            int i = 0;
            int j = -1;
            this.tdeg = -1;
            this.tdist = 9999;
            while (i <= 340)
            {
                j = SDK.Scan(i, 20);
                if ((j > 0) && (j < this.tdist))
                {
                    this.tdeg = i;
                    this.tdist = j;
                }
                i += 20;
            }
            this.tdist = 9999;
            for (i = this.tdeg - 10; i <= this.tdeg + 10; i++)
            {
                j = SDK.Scan(i, 1);
                if ((j > 0) && (j < this.tdist))
                {
                    int k = (int) (SDK.LocX + j*(SDK.Cos(i)/100000.0D));
                    int m = (int) (SDK.LocY + j*(SDK.Sin(i)/100000.0D));
                    if (isfriend(k, m) == false)
                    {
                        this.tdeg = i;
                        this.tdist = j;
                    }
                }
            }
            if (this.tdist == 9999)
            {
                radarscan(3);
            }
        }

        private void guessspeed()
        {
            double d1 = SDK.Time;
            double d2 = d1 - this.gsoldt;
            int i = this.tsx;
            int j = this.tsy;
            if (d2 >= 1.0D)
            {
                this.tsx = ((int) ((this.tx - this.gsoldtx)/d2));
                this.tsy = ((int) ((this.ty - this.gsoldty)/d2));
                if ((this.tsx > 35) || (this.tsx < -35))
                {
                    this.tsx = i;
                }
                if ((this.tsy > 35) || (this.tsy < -35))
                {
                    this.tsy = j;
                }
                this.gsoldtx = this.tx;
                this.gsoldty = this.ty;
                this.gsoldt = d1;
            }
        }

        private int my_atan(int paramInt1, int paramInt2)
        {
            if ((paramInt2 == 0) && (paramInt1 >= 0))
            {
                return 90;
            }
            if ((paramInt2 == 0) && (paramInt1 < 0))
            {
                return 270;
            }
            if ((paramInt2 > 0) && (paramInt1 >= 0))
            {
                return SDK.ATan(100000*paramInt1/paramInt2);
            }
            if ((paramInt2 > 0) && (paramInt1 <= 0))
            {
                return (360 + SDK.ATan(100000*paramInt1/paramInt2))%360;
            }
            return (180 + SDK.ATan(100000*paramInt1/paramInt2))%360;
        }

        private int smartfire()
        {
            int i2 = this.myid;
            int i3 = SDK.LocX;
            int i4 = SDK.LocY;
            friends[i2][0] = i3;
            friends[i2][1] = i4;
            this.tdist = SDK.Scan(this.tdeg, 1);
            if ((this.tdist <= 0) || (this.tdist > 800))
            {
                radarscan2();
            }
            if (this.tdist > 800)
            {
                guessspeed();
                return 0;
            }
            this.tx = ((int) (SDK.LocX + this.tdist*(SDK.Cos(this.tdeg)/100000.0D)));
            this.ty = ((int) (SDK.LocY + this.tdist*(SDK.Sin(this.tdeg)/100000.0D)));
            if (isfriend(this.tx, this.ty))
            {
                radarscan2();
            }
            if (this.tdist < 30)
            {
                return SDK.Cannon(this.tdeg, 30);
            }
            guessspeed();
            double d = this.tdist/300.0D;
            int i = (int) (this.tx + this.tsx*d);
            int j = (int) (this.ty + this.tsy*d);
            int k = i - friends[this.myid][0];
            int m = j - friends[this.myid][1];
            int n = SDK.Sqrt(k*k + m*m);
            int i1 = my_atan(m, k);
            return SDK.Cannon(i1, n);
        }

        private int collisiondetect(int paramInt1, int paramInt2)
        {
            int i = SDK.LocX;
            int j = SDK.LocY;
            if ((i > 1000 - paramInt1) && (((paramInt2 >= 0) && (paramInt2 < 90)) || ((paramInt2 > 270) && (paramInt2 < 360))))
            {
                return 1;
            }
            if ((i < paramInt1) && (paramInt2 > 90) && (paramInt2 < 270))
            {
                return 3;
            }
            if ((j > 1000 - paramInt1) && (paramInt2 > 0) && (paramInt2 < 180))
            {
                return 2;
            }
            if ((j < paramInt1) && (paramInt2 > 180) && (paramInt2 < 360))
            {
                return 4;
            }
            return 0;
        }

        private void updatefriends(int paramInt1, int paramInt2, int paramInt3)
        {
            friends[paramInt1][0] = paramInt2;
            friends[paramInt1][1] = paramInt3;
        }

        private void smartmove()
        {
            int i = this.mydeg;
            int j = collisiondetect(200, this.mydeg);
            if (j != 0)
            {
                SDK.Drive(this.mydeg, 48);
                while (SDK.Speed > 49)
                {
                    smartfire();
                }
                i = (this.tdeg + 100)%360;
                if (collisiondetect(200, i) != 0)
                {
                    i = (this.tdeg + 280)%360;
                }
                if (collisiondetect(200, i) != 0)
                {
                    i = this.tdeg;
                }
                this.mydeg = i;
                SDK.Drive(this.mydeg, 48);
                SDK.Drive(this.mydeg, 100);
                return;
            }
            if (((int) this.gsoldt%12 == 0) && (j == 0) && (SDK.Speed == 100))
            {
                SDK.Drive(this.mydeg, 48);
                while (SDK.Speed > 49)
                {
                    smartfire();
                }
                i = (this.mydeg + 150)%360;
                this.mydeg = i;
                SDK.Drive(this.mydeg, 48);
                SDK.Drive(this.mydeg, 100);
            }
        }
    }
}
