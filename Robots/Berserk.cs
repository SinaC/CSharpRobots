using System;
using SDK;

namespace Robots
{
    public class Berserk : Robot
    {
        private const double Tolerance = 0.0001;

        private static double[] a = new double[8];
        private static double[] b = new double[8];
        private static double[] c = new double[8];
        private static double[] d = new double[8];
        private static double[] e = new double[8];
        private static double[] f = new double[8];
        private static double[] g = new double[8];
        private bool h = false;
        private bool i = false;
        private bool j = false;
        private bool k = false;
        private bool l = false;
        private double m = 741.0D;
        private double n = -3.0D;
        private int o = 0;
        private double p = 0.0D;
        private double q = -1000.0D;
        private double r = -1000.0D;
        private double s;
        private double t;
        private double u;
        private double v;
        private double w;
        private double x;
        private double y;
        private double z;
        private double ab = -1000.0D;
        private double bb = -1000.0D;
        private double cb;
        private double db;
        private double eb;
        private double fb;
        private double gb;
        private double hb;
        private double ib;
        private double jb;
        private double kb;
        private double lb;
        private double mb = 0.0D;
        private int nb = 0;
        private int ob = 0;
        private int pb = 0;
        private int qb;
        private static int rb;
        private double sb = -10.0D;
        private double tb = -10.0D;
        private static double ub;
        private static double vb;
        private const double wb = 3.141592653589793D;
        private bool xb = true;
        private static int yb = 0;

        public override void Init()
        {
            qb = SDK.Id;
            updatePosition();
            rb = SDK.FriendsCount;

            if (rb == 8)
                calcCorner();
        }

        public override void Step()
        {
            switch (rb)
            {
                case 1:
                    doSingle();
                    break;
                case 2:
                    doDouble();
                    break;
                case 8:
                    doTeam();
                    break;
            }
        }

        private void doSingle()
        {
            updatePosition();
            findEnemy_single();
            updateEnemyInfo();
            smartFire();
            doDrive();
        }

        private void doDouble()
        {
            updatePosition();
            checkDeath();
            findEnemy_double();
            updateEnemyInfo();
            smartFire();
            doDrive2();
        }

        private void doTeam()
        {
            updatePosition();
            findEnemy_multiple_nearest();
            updateEnemyInfo();
            smartFire();
            doDrive();
        }

        private void calcCorner()
        {
            if (rb == 8)
            {
                if (a[qb] >= 500.0D)
                {
                    kb = (845 + 13*qb);
                }
                else
                {
                    lb = (50 + 13*qb);
                }
                if (b[qb] >= 500.0D)
                {
                    lb = (845 + 13*qb);
                }
                else
                {
                    lb = (50 + 13*qb);
                }
            }
            else
            {
                for (int i1 = 0; i1 < rb; i1++)
                {
                    kb += a[i1];
                    lb += b[i1];
                }
                kb /= rb;
                lb /= rb;
            }
        }

        private void updateEnemyInfo()
        {
            if ((m > o) || (tb - n > 2.47D))
            {
                m = o;
                n = SDK.Time;
            }
            if (SDK.Time - tb < 0.5D)
            {
                return;
            }
            tb = SDK.Time;
            u = s;
            v = t;
            double d1 = a[qb] + (o + 0.5D)*SDK.Cos(p/57.295779513082323D);
            double d2 = b[qb] + (o + 0.5D)*SDK.Sin(p/57.295779513082323D);
            double d3 = (o + 0.5D)*3.141592653589793D/1440.0D;
            double d4;
            double d5;
            double d6;
            double d7;
            double d8;
            double d9;
            for (int i1 = 0; i1 < rb; i1++)
            {
                if (tb - g[i1] < 1.0D)
                {
                    d4 = d[i1] - d1;
                    d5 = e[i1] - d2;
                    if (d4*d4 + d5*d5 < 785.0D)
                    {
                        d6 = d[i1];
                        d7 = e[i1];
                        d8 = f[i1] + (tb - g[i1])*30.0D;
                        d9 = d3 + d8;
                        d1 = (d1*d8 + d6*d3)/d9;
                        d2 = (d2*d8 + d7*d3)/d9;
                        d3 = d3*d8/d9;
                    }
                }
            }
            double tmp333_332 = d1;
            s = tmp333_332;
            d[qb] = tmp333_332;
            double tmp347_346 = d2;
            t = tmp347_346;
            e[qb] = tmp347_346;
            double tmp367_364 = (z = d3);
            y = tmp367_364;
            f[qb] = tmp367_364;
            g[qb] = SDK.Time;
            d4 = s - u;
            d5 = t - v;
            if (d4*d4 + d5*d5 > 3600.0D)
            {
                ab = -1000.0D;
                bb = -1000.0D;
                q = -1000.0D;
                r = -1000.0D;
            }
            if ((Math.Abs(ab - (-1000.0D)) < Tolerance) || (Math.Abs(bb - (-1000.0D)) < Tolerance))
            {
                cb = 0.0D;
                db = 0.0D;
            }
            else
            {
                cb = (s - u);
                db = (t - v);
            }
            gb = (hb = 3.141592653589793D*o/270.0D);
            if ((Math.Abs(q - (-1000.0D)) > Tolerance) && (Math.Abs(r - (-1000.0D)) > Tolerance))
            {
                d6 = y + w;
                tmp347_346 = z + x;
                d8 = y/d6;
                d9 = z/tmp347_346;
                double d10 = w/d6;
                double d11 = x/tmp347_346;
                ab = (d8*q + d10*s);
                bb = (d9*r + d11*t);
                eb = (y*w/d6);
                fb = (z*x/tmp347_346);
            }
            else
            {
                ab = s;
                bb = t;
                eb = y;
                fb = z;
            }
            q = (ab + cb*1.0D);
            r = (bb + db*1.0D);
            w = (eb + gb*1.0D);
            x = (fb + hb*1.0D);
            d4 = q - SDK.LocX;
            d5 = r - SDK.LocY;
            o = ((int) SDK.Sqrt(d4*d4 + d5*d5));
        }

        private void smartMove()
        {
            int i1 = SDK.Speed;
            if ((m > 755.0D) || (isWall(pb)) || (i1 == 0))
            {
                SDK.Drive(pb = (int) p, 50);
                l = false;
            }
            else
            {
                int i2 = ((int) p + 180)%360;
                int i3;
                if (o < 740)
                {
                    i3 = 100;
                }
                else
                {
                    i3 = 50;
                }
                if (i1 > 50)
                {
                    if (i3 == 50)
                    {
                        SDK.Drive(pb, 50);
                    }
                    return;
                }
                if (isWall(i2))
                {
                    SDK.Drive(pb = getAngle(p), i3);
                    l = true;
                }
                else
                {
                    SDK.Drive(pb = i2, i3);
                    l = false;
                }
            }
        }

        private void smartFire()
        {
            ib = (ab + cb*o/150.0D + 0.5D);
            jb = (bb + db*o/150.0D + 0.5D);
            double d1 = ib - SDK.LocX;
            double d2 = jb - SDK.LocY;
            fire(ib, jb);
            upPos(ib, jb);
        }

        private void upPos(double paramDouble1, double paramDouble2)
        {
            ub = paramDouble1;
            vb = paramDouble2;
        }

        void checkDeath()
        {
            if ((SDK.Time - c[0] > 1.5D) || (SDK.Time - c[1] > 1.5D))
            {
                doSingle();
            }
        }

        void findEnemy_double()
        {
            if ((qb == yb) || (dist2(a[qb], b[qb], d[yb], e[yb]) > 490000.0D))
            {
                findEnemy_multiple_nearest();
                if ((qb != yb) || (o > 700))
                {
                    k = true;
                }
                else
                {
                    k = false;
                }
                return;
            }
            k = false;
            for (p = 0.0D; p < 360.0D; p += 1.0D)
            {
                if (((o = SDK.Scan((int)p, 1)) != 0) && (!isFriend((int)p, o)))
                {
                    double d1 = a[qb] + (o + 0.5D) * SDK.Cos(p / 57.295779513082323D);
                    double d2 = b[qb] + (o + 0.5D) * SDK.Sin(p / 57.295779513082323D);
                    if (dist2(d1, d2, d[yb], e[yb]) < 3600.0D)
                    {
                        int i1 = SDK.Scan((int)p - 1, 2);
                        int i2 = SDK.Scan((int)p + 1, 2);
                        if ((i1 != 0) && (i2 == 0))
                        {
                            p -= 0.25D;
                        }
                        else
                        {
                            p += 0.25D;
                        }
                        return;
                    }
                }
            }
        }

        void findEnemy_multiple_nearest()
        {
            o = 100000;
            p = 100000.0D;
            for (int i1 = 359; i1 >= 0; i1--)
            {
                int i2;
                if (((i2 = SDK.Scan(i1, 1)) != 0) && (i2 < o) && (!isFriend(i1, i2)))
                {
                    int i3 = SDK.Scan(i1 - 1, 2);
                    int i4 = SDK.Scan(i1 + 1, 2);
                    o = i2;
                    if ((i3 != 0) && (i4 == 0))
                    {
                        p = (i1 - 0.25D);
                    }
                    else
                    {
                        p = (i1 + 0.25D);
                    }
                }
            }
            if ((o == 100000) || (Math.Abs(p - 100000.0D) < Tolerance))
            {
                fire(ub, vb);
                SDK.Drive(0, 0);
                findEnemy_multiple_nearest();
            }
        }

        void findEnemy_single()
        {
            for (p = 359.0D; p >= 0.0D; p -= 1.0D)
            {
                if ((o = SDK.Scan((int)p, 1)) != 0)
                {
                    int i1 = SDK.Scan((int)(p - 1.0D), 2);
                    int i2 = SDK.Scan((int)(p + 1.0D), 2);
                    if ((i1 != 0) && (i2 == 0))
                    {
                        p -= 0.25D;
                    }
                    else
                    {
                        p += 0.25D;
                    }
                    return;
                }
            }
        }

        void fire(double paramDouble1, double paramDouble2)
        {
            if (paramDouble1 > 1000.0D)
            {
                paramDouble1 = 1000.0D;
            }
            if (paramDouble1 < 0.0D)
            {
                paramDouble1 = 0.0D;
            }
            if (paramDouble2 > 1000.0D)
            {
                paramDouble2 = 1000.0D;
            }
            if (paramDouble2 < 0.0D)
            {
                paramDouble2 = 0.0D;
            }
            double d1 = paramDouble1 - SDK.LocX;
            double d2 = paramDouble2 - SDK.LocY;
            double d3;
            if (Math.Abs(d1) < Tolerance)
            {
                d3 = d2 <= 0.0D ? 270.0D : 90.0D;
            }
            else
            {
                d3 = SDK.ATan(d2 / d1) * 180.0D / 3.141592653589793D;
                if (d1 < 0.0D)
                {
                    d3 += 180.0D;
                }
                d3 = (d3 + 720.0D) % 360.0D;
            }
            double d4 = SDK.Sqrt(d1 * d1 + d2 * d2);
            if (d4 < 5.0D)
            {
                d4 = 6.0D;
            }
            else if (d4 < 40.0D)
            {
                d4 = 41.0D;
            }
            else
            {
                d4 += 0.5D;
            }
            SDK.Cannon((int)(d3 + 0.5D), (int)d4);
        }

        bool isWall(int paramInt)
        {
            double d1 = a[qb] + 100 * SDK.Cos(paramInt) / 100000.0;
            double d2 = b[qb] + 100 * SDK.Sin(paramInt) / 100000.0;
            return (d1 <= 0.0D) || (d1 >= 1000.0D) || (d2 <= 0.0D) || (d2 >= 1000.0D);
        }

        double distToWall()
        {
            double d1 = a[qb];
            double d2 = 999.0D - a[qb];
            double d3 = b[qb];
            double d4 = 999.0D - b[qb];
            if ((d1 < d2) && (d1 < d3) && (d1 < d4))
            {
                return d1;
            }
            if ((d2 < d3) && (d2 < d4))
            {
                return d2;
            }
            if (d3 < d4)
            {
                return d3;
            }
            return d4;
        }

        bool isFriend(int paramInt1, int paramInt2)
        {
            double d1 = a[qb] + paramInt2 * SDK.Cos(paramInt1) / 100000.0;
            double d2 = b[qb] + paramInt2 * SDK.Sin(paramInt1) / 100000.0;
            double d3 = SDK.Time;
            for (int i1 = 0; i1 < rb; i1++)
            {
                if ((i1 != qb) && (d3 - c[i1] <= 1.0D))
                {
                    double d4 = d1 - a[i1];
                    double d5 = d2 - b[i1];
                    double d6 = d4 * d4 + d5 * d5;
                    if (d6 < 3600.0D)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void doDrive()
        {
            if ((rb != 8) || (j))
            {
                smartMove();
                return;
            }
            driveTo(kb, lb);
        }

        void doDrive2()
        {
            if (k)
            {
                driveTo(d[yb], e[yb]);
            }
            else
            {
                if (SDK.Time - sb < 2.5D)
                {
                    return;
                }
                sb = SDK.Time;
                int i1;
                do
                {
                    i1 = SDK.Rand(360);
                } while (isWall(i1));
                SDK.Drive(i1, 50);
                return;
            }
        }

        void driveTo(double paramDouble1, double paramDouble2)
        {
            driveTo((int)paramDouble1, (int)paramDouble2);
        }

        void driveTo(int paramInt1, int paramInt2)
        {
            double d1 = paramInt1 - a[qb];
            double d2 = paramInt2 - b[qb];
            if (d1 * d1 + d2 * d2 > 10000.0D)
            {
                int i1;
                if (Math.Abs(d1) < Tolerance)
                {
                    i1 = d2 <= 0.0D ? 270 : 90;
                }
                else
                {
                    i1 = SDK.ATan((int)(d2 * 100000.0D / d1));
                    if (d1 < 0.0D)
                    {
                        i1 += 180;
                    }
                    i1 = (i1 + 720) % 360;
                }
                SDK.Drive(i1, 100);
            }
            else
            {
                j = true;
            }
        }

        double abs(double paramDouble)
        {
            return paramDouble <= 0.0D ? -paramDouble : paramDouble;
        }

        void updatePosition()
        {
            a[qb] = SDK.LocX;
            b[qb] = SDK.LocY;
            c[qb] = SDK.Time;
        }

        double dist2(double paramDouble1, double paramDouble2, double paramDouble3, double paramDouble4)
        {
            return (paramDouble1 - paramDouble3) * (paramDouble1 - paramDouble3) + (paramDouble2 - paramDouble4) * (paramDouble2 - paramDouble4);
        }

        int getAngle(double paramDouble)
        {
            double d1 = dist2(a[qb], b[qb], 0.0D, 0.0D);
            double d2 = dist2(a[qb], b[qb], 999.0D, 0.0D);
            double d3 = dist2(a[qb], b[qb], 999.0D, 999.0D);
            double d4 = dist2(a[qb], b[qb], 0.0D, 999.0D);
            if ((paramDouble > 45.0D) && (paramDouble <= 135.0D))
            {
                if (d1 > d2)
                {
                    return getAngleTo(0, 0);
                }
                return getAngleTo(999, 0);
            }
            if ((paramDouble > 135.0D) && (paramDouble <= 225.0D))
            {
                if (d2 > d3)
                {
                    return getAngleTo(999, 0);
                }
                return getAngleTo(999, 999);
            }
            if ((paramDouble > 225.0D) && (paramDouble <= 315.0D))
            {
                if (d3 > d4)
                {
                    return getAngleTo(999, 999);
                }
                return getAngleTo(0, 999);
            }
            if (d4 > d1)
            {
                return getAngleTo(0, 999);
            }
            return getAngleTo(0, 0);
        }

        int getAngleTo(int paramInt1, int paramInt2)
        {
            double d1 = paramInt1 - a[qb];
            double d2 = paramInt2 - b[qb];
            int i1;
            if (Math.Abs(d1) < Tolerance)
            {
                i1 = d2 <= 0.0D ? 270 : 90;
            }
            else
            {
                i1 = SDK.ATan((int)(d2 * 100000.0D / d1));
                if (d1 < 0.0D)
                {
                    i1 += 180;
                }
                i1 = (i1 + 720) % 360;
            }
            return i1;
        }
    }
}
