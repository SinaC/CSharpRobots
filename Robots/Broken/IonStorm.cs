using SDK;

namespace Robots.Broken
{
    [Broken]
    public class IonStorm : Robot
    {
        private static short[] f1 = {100, 35, -35, -100, -35, 35};
        private static short[] f2 = {40, -40};
        private static short[] f3 = {15, -15};
        private static short[] f4 = {0, 90, -45, -90, 45};
        private static short[] f5 = {0};
        private static short[] f6 = new short[0];
        private static double[] f7 = {0.0D, 1000.0D, 0.0D, 1000.0D};
        private static double[] f8 = {0.0D, 0.0D, 1000.0D, 1000.0D};
        private static double[] f9 = new double[8];
        private static double[] f10 = new double[8];
        private static double[] f11 = new double[8];
        private static double[] f12 = new double[4];
        private static double[] f13 = new double[2];
        private static double[] f14 = new double[2];
        private static double[] f15 = new double[2];
        private static double[] f16 = new double[2];
        private static IonStorm[] f17 = new IonStorm[8];
        private static int f18;
        private static short[] f19 = {45, 45, 45, -45, -45, -45, 45, 45, 45, -45, -45, -45};
        private static short[] f20 = {60, 60, 60, 60, 60, 60, 60, 60};
        private static short[] f21 = {-60, -60, -60, -60, -60, -60, -60, -60};
        private static short[] f22 = {125, -125, 125, -125, 125, -125, 125, -125};
        private static short[] f23 = {-65, -110, 60, 120, -70, -110, 60, 120, -70, -110, 60, 120};
        private int f24;
        private double f25 = 0.0D;
        private double f26 = 1.0D;
        private double f27 = 0.0D;
        private double f28 = 1.0D;
        private double f29 = 0.0D;
        private double f30 = 0.0D;
        private double f31 = -5.0D;
        private bool f32 = true;
        private int f33 = 0;
        private bool f34 = true;
        private int f35 = 0;
        private double f36 = 0.0D;
        private double f37;
        private int f38;
        private bool f39 = false;
        private bool f40 = false;
        private bool f41 = false;
        private bool f42 = false;
        private bool f43 = false;
        private bool f44 = false;
        private int f45 = 1;
        private double f46;
        private double f47;
        private short[] f48 = f4;
        private int f49 = 0;
        private int f50 = 500;
        private double f51 = 0.0D;
        private double f52 = 0.0D;
        private double f53 = 0.0D;
        private double f54 = 0.0D;
        private double f55 = 0.0D;
        private int f56 = 0;
        private int f57 = 0;
        private double f58 = 0.0D;
        private double f59 = 99999.0D;
        private double f60 = 99999.0D;
        private double f61 = -9999.0D;
        private double[] f62 = new double[900];
        private double[] f63 = new double[900];
        private double[] f64 = new double[900];
        private int f65 = 0;
        private double[] f66 = new double[700];
        private double[] f67 = new double[700];
        private double[] f68 = new double[700];
        private double[] f69 = new double[700];
        private double[] f70 = new double[700];
        private int f71 = 0;
        private int f72 = 0;
        private int f73 = 4;
        private double[] f74;
        private double[] f75;
        private double[] f76;
        private int f77 = 0;
        private double f78;
        private double f79;
        private double[] f80 = new double[5];
        private double[] f81 = new double[5];
        private double f82;
        private double f83;
        private double f84;
        private double f85;
        private double f86;
        private double f87;
        private double f88;
        private int f89;
        private double f90;
        private double f91;
        private double f92 = 0.0D;
        private double f93 = -5.0D;
        private int f94;
        private double f95;
        private double f96;
        private double[] f97 = new double[50];
        private double[] f98 = new double[50];
        private int f99 = 0;
        private IonStorm f100;
        private double f101;
        private double f102;
        private int f103 = 0;
        private double[] f104 = new double[100];
        private double[] f105 = new double[100];
        private int f106 = 0;
        private double f107;

        private double _m1Time;
        private int _step;


        public override void Init()
        {
            f74 = this.f62;
            f75 = this.f63;
            f76 = this.f64;

            this.f24 = SDK.Id;
            f17[this.f24] = this;
            f18 = SDK.FriendsCount;
            this.f34 = false;
            this.f33 = 0;
            for (int i = 0; i < f11.Length; i++)
            {
                f11[i] = 0.0D;
            }
            for (int i = 0; i < f13.Length; i++)
            {
                f13[i] = -5000.0D;
                f14[i] = -5000.0D;
                f15[i] = -5000.0D;
                f16[i] = 0.0D;
            }
            m2();
            m24();
            m59(true);
            for (int i = 0; i < 4; i++)
            {
                f12[i] = 0.0D;
            }

            _step = 1;
            _m1Time = 0.15;
        }

        public override void Step()
        {
            switch (_step)
            {
                case 1:
                    if (f87 < _m1Time)
                        m1_();
                    else
                    {
                        this.f32 = true;
                        this.f34 = (f18 == 2);
                        if (f18 == 2)
                        {
                            this.f100 = f17[(1 - this.f24)];
                            if (m73(this.f90, this.f91, this.f100.f90, this.f100.f91) < 100.0D)
                            {
                                this.f25 = this.f87;
                            }
                        }
                        m10();
                        
                        _m1Time = 0.5;
                        _step = 2;
                    }
                    break;
                case 2:
                    if (f87 < _m1Time)
                        m1_();
                    else
                    {
                        m8();
                        m14();
                        this.f32 = true;

                        _m1Time = 100000000;
                        _step = 3;
                    }
                    break;
                default:
                    m1_();
                    break;
            }
        }


        private double actual_speed()
        {
            return 30.0 * SDK.Speed / 100.0;
        }

        private void m1_()
        {
            m70();
            m2();
            m4();
            m58();
            m7();
            m33();
            m53();
        }

        private void main()
        {
            this.f24 = SDK.Id;
            f17[this.f24] = this;
            f18 = SDK.FriendsCount;
            this.f34 = false;
            this.f33 = 0;
            for (int i = 0; i < f11.Length; i++)
            {
                f11[i] = 0.0D;
            }
            for (int i = 0; i < f13.Length; i++)
            {
                f13[i] = -5000.0D;
                f14[i] = -5000.0D;
                f15[i] = -5000.0D;
                f16[i] = 0.0D;
            }
            m2();
            m24();
            m59(true);
            for (int i = 0; i < 4; i++)
            {
                f12[i] = 0.0D;
            }
            m1(0.15D);
            this.f32 = true;
            this.f34 = (f18 == 2);
            if (f18 == 2)
            {
                this.f100 = f17[(1 - this.f24)];
                if (m73(this.f90, this.f91, this.f100.f90, this.f100.f91) < 100.0D)
                {
                    this.f25 = this.f87;
                }
            }
            m10();
            m1(0.5D);
            m8();
            m14();
            this.f32 = true;
            m1(1000000.0D);
        }

        private void m1(double paramDouble)
        {
            do
            {
                m70();
                m2();
                m4();
                m58();
                m7();
                m33();
                m53();
            } while (this.f87 < paramDouble);
        }

        private void m2()
        {
            this.f90 = SDK.LocX;
            this.f91 = SDK.LocY;
            this.f87 = SDK.Time;
            this.f88 = actual_speed();
            f9[this.f24] = this.f90;
            f10[this.f24] = this.f91;
            f11[this.f24] = this.f87;
            this.f94 = 0;
            this.f60 = 999999.0D;
            for (int i = 0; i < f18; i++)
            {
                if (m3(i))
                {
                    this.f94 += 1;
                    double d;
                    if ((i != this.f24) && ((d = m73(this.f90, this.f91, f9[i], f10[i])) < this.f60))
                    {
                        this.f60 = d;
                    }
                }
            }
        }

        private bool m3(int paramInt)
        {
            double d;
            return ((d = f11[paramInt]) >= 0.0D) && (this.f87 - d <= 0.15D);
        }

        private void m4()
        {
            int i;
            if ((i = SDK.Damage) != this.f89)
            {
                if ((f18 == 1) || ((f18 == 2) && (this.f56 <= 1)))
                {
                    m6();
                }
                this.f89 = i;
                this.f93 = this.f87;
            }
        }

        private bool m5()
        {
            return (this.f88 <= 15.0D) && (this.f38 <= 50);
        }

        private void m6()
        {
            if ((this.f87 >= 3.0D) && ((this.f89 <= 0) || (this.f52 < 300.0D)) && (this.f28 - this.f87 <= 1.3D))
            {
                m59(false);
                double d;
                for (d = this.f87 - this.f52/300.0D + 1.0D; d - this.f29 < 0.5D; d += 1.0D)
                    ;
                this.f28 = d;
            }
        }

        private void m7()
        {
            m17();
            m14();
            m19();
            if (!this.f44)
            {
                m21();
                if (!this.f41)
                {
                    m23();
                    m24();
                }
            }
        }

        private void m8()
        {
            if (f18 > 2)
            {
                int i = m9(-1);
                m9(i);
                m73(this.f90, this.f91, f7[i], f8[i]);
                double d1 = 0.7D;
                int j = i;
                double d2 = 0.0D;
                for (int k = 0; k < 4; k++)
                {
                    double d3 = m73(this.f90, this.f91, f7[k], f8[k]) + 0.1D;
                    double d4 = f12[k];
                    double d5 = (5000.0D + d4)/(d3 + 0.1D);
                    if (k != i)
                    {
                        d5 *= d1;
                    }
                    if (d5 > d2)
                    {
                        d2 = d5;
                        j = k;
                    }
                }
                if (d2 >= 2.0D)
                {
                    m13(j, 850.0D);
                }
            }
        }

        private int m9(int paramInt)
        {
            int i = 0;
            double d = -100000000.0D;
            for (int j = 0; j < 4; j++)
            {
                if ((j != paramInt) && (f12[j] > d))
                {
                    d = f12[j];
                    i = j;
                }
            }
            return i;
        }

        private void m10()
        {
            double d1 = 2.0D;
            double d2 = 1.5D;
            for (int i = 0; i < f18; i++)
            {
                m11(f9[i], f10[i], d1 + d2);
            }
            for (int i = 0; i < 360; i++)
            {
                double d3;
                if ((d3 = SDK.Scan(i, 1)) > 0.0D)
                {
                    double d4 = this.f90 + d3*m76(i);
                    double d5 = this.f91 + d3*m75(i);
                    m11(d4, d5, -d2);
                }
            }
        }

        private void m11(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            for (int i = 0; i < 4; i++)
            {
                double d = m73(f7[i], f8[i], paramDouble1, paramDouble2);
                f12[i] += paramDouble3*m12(d, false);
            }
        }

        private double m12(double paramDouble, bool parambool)
        {
            if ((parambool) && (paramDouble < 100.0D))
            {
                return 400.0D - paramDouble*3.0D;
            }
            if (paramDouble < 300.0D)
            {
                return 100.0D - paramDouble/12.0D;
            }
            paramDouble -= 300.0D;
            if (paramDouble < 300.0D)
            {
                return 75.0D - paramDouble/6.0D;
            }
            paramDouble -= 300.0D;
            if (paramDouble < 300.0D)
            {
                return 25.0D - paramDouble/12.0D;
            }
            return 0.0D;
        }

        private bool m13(int paramInt, double paramDouble)
        {
            double d1 = 85.0D;
            this.f46 = f7[paramInt];
            if (this.f46 == 0.0D)
            {
                this.f46 += d1;
            }
            else
            {
                this.f46 -= d1;
            }
            this.f47 = f8[paramInt];
            if (this.f47 == 0.0D)
            {
                this.f47 += d1;
            }
            else
            {
                this.f47 -= d1;
            }
            double d2 = m73(this.f46, this.f47, this.f90, this.f91);
            this.f43 = ((d2 > 0.0D) && (d2 <= paramDouble));
            this.f44 = ((this.f43) && (d2 > 200.0D));
            if (this.f43)
            {
                m16();
            }
            return this.f43;
        }

        private void m14()
        {
            if ((f18 != 2) || (this.f44) || (!m5()))
            {
                return;
            }
            double d = 100.0D;
            this.f46 = (this.f53 - d*m76(this.f51));
            this.f47 = (this.f54 - d*m75(this.f51));
            this.f43 = false;
            if ((m15()) && (!m18()))
            {
                this.f43 = true;
                this.f44 = true;
                this.f41 = false;
                this.f42 = false;
                m16();
            }
        }

        private bool m15()
        {
            if (this.f33 == 2)
            {
                return false;
            }
            if (f18 == 2)
            {
                if (this.f100 == null)
                {
                    return false;
                }
                if (this.f100.f89 > 90)
                {
                    return false;
                }
                if (this.f87 - this.f93 < 3.0D)
                {
                    return false;
                }
                if (this.f52 < 200.0D)
                {
                    return false;
                }
                if (this.f33 == 2)
                {
                    return false;
                }
            }
            double d1 = m73(this.f90, this.f91, this.f46, this.f47);
            double d2;
            double d3;
            double d4 = (d3 = ((d2 = this.f88) - 15.0D)/5.0D)*(d2 - 5.0D*d3/2.0D);
            return d1 - d4 > 0.0D;
        }

        private void m16()
        {
            this.f37 = m74(this.f46 - this.f90, this.f47 - this.f91);
            m26();
        }

        private void m17()
        {
            if ((this.f43) && (m18()))
            {
                m20();
                this.f43 = false;
            }
        }

        private bool m18()
        {
            double d = m73(this.f90, this.f91, this.f46, this.f47);
            if ((this.f100 != null) && (this.f100.f89 > 90))
            {
                return true;
            }
            if (d < 40.0D)
            {
                return true;
            }
            if ((f18 > 2) && (d < 400.0D) && (this.f58 > 740.0D))
            {
                this.f42 = true;
                return true;
            }
            return false;
        }

        private void m19()
        {
            if (this.f44)
            {
                if (!m15())
                {
                    m20();
                }
                if (this.f38 != m27())
                {
                    m26();
                }
            }
        }

        private void m20()
        {
            if (this.f44)
            {
                this.f44 = false;
                m25();
                m31();
            }
        }

        private void m21()
        {
            int i = f18 > 2 ? 500 : 680;
            int j = (this.f43) || ((!this.f41) && (!m5())) ? 0 : 1;
            this.f41 = ((j != 0) && (this.f52 > i));
            //break
            //label180;
            int k = 0;
            do
            {
                double d1 = f7[k];
                double d2 = f8[k];
                if (m73(this.f53, this.f54, d1, d2) >= 741.0D)
                {
                    double d3 = m74(d1 - this.f90, d2 - this.f91);
                    if (m76(this.f53)*m76(d3) + m75(this.f53)*m75(d3) <= 0.0D)
                    {
                        this.f41 = true;
                    }
                }
                k++;
            } while ((!this.f41) && (k < 4));
            //label180:
            if ((this.f41) && (f18 == 2))
            {
                if (this.f33 == 1)
                {
                    this.f41 = false;
                }
                else if ((this.f100 != null) && (this.f100.f59 < this.f52) && (this.f100.f89 < 90))
                {
                    this.f41 = false;
                }
            }
            if (this.f42)
            {
                this.f41 = true;
            }
            if (this.f41)
            {
                m22();
            }
        }

        private void m22()
        {
            double d1 = this.f89 >= 10 ? 0 : this.f89 >= 90 ? 1 : -1;
            double d2 = 742.5D + d1;
            double d3 = 2.0D;
            double d4, d5;
            int i, j;
            if (this.f65 > 0)
            {
                d4 = this.f87 - d3;
                i = 0;
                j = this.f65 - 1;
                do
                {
                    if ((d5 = m73(this.f90, this.f91, this.f62[j], this.f63[j])) < d2)
                    {
                        i = 1;
                    }
                    if ((d5 < d2 + 10.0D) && (i == 0))
                    {
                        i = 2;
                    }
                    j--;
                    if ((i == 1) || (j < 0))
                    {
                        break;
                    }
                } while (this.f64[j] >= d4);
            }
            else
            {
                i = 1;
            }
            if (i == 1)
            {
                this.f45 = -1;
            }
            if (i == 0)
            {
                this.f45 = 1;
            }
            this.f99 = 0;
            if (i == 1)
            {
                m86(-40.0D);
            }
            else if (i == 0)
            {
                m86(1.0D);
            }
            else if (i == 2)
            {
                m86(this.f45);
            }
            if (!m87())
            {
                m84();
                m85();
            }
            d4 = m89();
            j = 50;
            d5 = m78(this.f51 + 180.0D);
            if ((this.f52 < 720.0D) && (!m29(d5, 90.0D)))
            {
                j = 100;
                d4 = d5;
            }
            double d6 = m76(d4);
            double d7 = m75(d4);
            if ((this.f90 < 3.0D) && (d6 < 0.0D))
            {
                d6 = 0.0D;
            }
            if ((this.f90 > 997.0D) && (d6 > 0.0D))
            {
                d6 = 0.0D;
            }
            if ((this.f91 < 3.0D) && (d7 < 0.0D))
            {
                d7 = 0.0D;
            }
            if ((this.f91 > 997.0D) && (d7 > 0.0D))
            {
                d7 = 0.0D;
            }
            if ((d6 != 0.0D) || (d7 != 0.0D))
            {
                d4 = m74(d6, d7);
            }
            else
            {
                d4 = m74(500.0D - this.f90, 500.0D - this.f91) + (SDK.Rand(60) - 30);
            }
            if (this.f52 > 830.0D)
            {
                j = 100;
                d4 = this.f51;
            }
            int k = this.f57*100;
            int m = 0;
            int n = 0;
            for (int i1 = 0; (n == 0) && (i1 < f18); i1++)
            {
                if (m3(i1))
                {
                    IonStorm localIonStorm;
                    n = ((localIonStorm = f17[i1]).f41) && (localIonStorm.f52 <= 790.0D) && (localIonStorm.f60 >= 20.0D) ? 0 : 1;
                    m += 100 - localIonStorm.f89;
                }
            }
            double d8 = this.f94*3;
            double d9 = this.f57*5;
            if (this.f94 <= 2)
            {
                d9 += 2.0D;
            }
            double d10 = m/d9;
            double d11 = k/d8;
            int i2 = (180.0D - this.f87 >= d10 - 3.0D) && (d10 <= d11) ? 0 : 1;
            if ((n == 0) && (i2 != 0))
            {
                j = 100;
                d4 = this.f51;
            }
            if (SDK.Abs(m79(d4 - this.f37)) > 15.0D)
            {
                j = 50;
            }
            if (actual_speed() <= 15.0D)
            {
                this.f37 = d4;
            }
            this.f38 = j;
            m31();
            this.f42 = ((actual_speed() > 15.0D) || (j > 50));
        }

        private void m23()
        {
            if (this.f87 >= this.f25)
            {
                this.f39 = false;
                this.f40 = false;
                this.f36 = this.f51;
                short[] arrayOfShort = f1;
                double d1 = 2.0D;
                double d2 = SDK.Abs(this.f90 - 500.0D);
                double d3 = SDK.Abs(this.f91 - 500.0D);
                double d4;
                int i = (d4 = 500.0D - (d2 > d3 ? d2 : d3)) >= 20.0D ? 0 : 1;
                int j = d4 >= 75.0D ? 0 : 1;
                double d5 = this.f51 + 180.0D;
                double d6 = m74(500.0D - this.f90, 500.0D - this.f91);
                double d7 = m68(this.f53, this.f54);
                if ((this.f43) && (f18 > 2))
                {
                    this.f36 = m74(this.f46 - this.f90, this.f47 - this.f91);
                    arrayOfShort = f2;
                }
                else if (this.f52 > 780.0D)
                {
                    arrayOfShort = f5;
                }
                else
                {
                    if (f18 >= 2)
                    {
                        if (this.f60 < (f18 == 2 ? 100 : 50))
                        {
                            this.f99 = 0;
                            m84();
                            this.f36 = m89();
                            arrayOfShort = f5;
                            //break
                            //label596;
                            goto label596;
                        }
                    }
                    if (i != 0)
                    {
                        this.f36 = d6;
                        arrayOfShort = f5;
                    }
                    else if (j != 0)
                    {
                        double d8 = m76(d5) + m76(d6)*1.5D;
                        double d9 = m75(d5) + m75(d6)*1.5D;
                        this.f36 = m74(d8, d9);
                    }
                    else if ((this.f33 == 1) && (this.f52 > 650.0D))
                    {
                        arrayOfShort = f3;
                        this.f40 = true;
                    }
                    else if ((this.f52 > 740.0D) && (this.f89 < 90))
                    {
                        if (f18 > 2)
                        {
                            arrayOfShort = f1;
                        }
                        else
                        {
                            arrayOfShort = f6;
                        }
                    }
                    else if (this.f52 > 710.0D)
                    {
                        this.f36 = (f18 <= 2 ? d5 : this.f51);
                        arrayOfShort = f4;
                    }
                    else if ((f18 <= 2) && (this.f52 < 80.0D))
                    {
                        this.f36 = d5;
                        arrayOfShort = f5;
                        this.f39 = true;
                    }
                    else if ((this.f52 < 320.0D) && (this.f89 > 90))
                    {
                        this.f36 = d5;
                    }
                    else if ((f18 != 2) || (this.f57 != 1) || (d7 <= this.f89 + 15))
                    {
                        if ((f18 == 2) && (this.f57 == 1) && (d7 < this.f89 - 15))
                        {
                            this.f36 = d5;
                        }
                        else
                        {
                            arrayOfShort = f6;
                        }
                    }
                }
                label596:
                if (arrayOfShort != this.f48)
                {
                    this.f48 = arrayOfShort;
                    this.f49 = 0;
                }
                this.f25 = (this.f87 + d1);
            }
        }

        private void m24()
        {
            if (this.f88 == 0.0D)
            {
                this.f37 = m74(500.0D - this.f90, 500.0D - this.f91);
                m26();
            }
            m25();
            if ((this.f38 > 50) && (m30()))
            {
                this.f38 = 50;
                m31();
            }
            if ((this.f87 >= this.f28) && (m5()))
            {
                m28();
            }
        }

        private void m25()
        {
            if ((this.f38 > m27()) && (this.f88 >= 10.0D))
            {
                this.f38 = m27();
                m31();
            }
        }

        private void m26()
        {
            int i = m27();
            this.f38 = i;
            m31();
        }

        private int m27()
        {
            if (this.f44)
            {
                if (f18 == 2)
                {
                    int i = 1 - this.f24;
                    if ((this.f100 != null) && (m3(i)) && (this.f100.f59 > this.f58))
                    {
                        return 80;
                    }
                }
                return 100;
            }
            if ((this.f39) || (this.f40))
            {
                return 100;
            }
            if (this.f41)
            {
                return 50;
            }
            if (this.f52 > 850.0D)
            {
                return 100;
            }
            if ((this.f88 < 10.0D) || (this.f89 > 0))
            {
                if (this.f52 >= 300.0D)
                {
                    return 100;
                }
                return 65;
            }
            return 50;
        }

        private void m28()
        {
            double d1 = m78(this.f36 + m32());
            this.f37 = d1;
            double d2 = this.f52 > 500.0D ? 2 : 1;
            if ((this.f39) || (this.f40))
            {
                d2 = 4.0D;
            }
            while (m29(this.f37, d2*25.0D))
            {
                this.f37 += 10 + SDK.Rand(30);
            }
            m26();
            this.f29 = this.f87;
            this.f28 = (this.f87 + d2);
        }

        private bool m29(double paramDouble1, double paramDouble2)
        {
            double d1 = this.f90 + paramDouble2*m76(paramDouble1);
            double d2 = this.f91 + paramDouble2*m75(paramDouble1);
            return (d1 <= 0.0D) || (d2 <= 0.0D) || (d1 >= 1000.0D) || (d2 >= 1000.0D);
        }

        private bool m30()
        {
            if ((this.f38 <= 50) || (this.f39) || (this.f44))
            {
                return false;
            }
            double d1 = this.f88 - 15.0D;
            double d2;
            return ((d2 = this.f28 - this.f87 - 0.05D) <= 0.0D) || (d1/d2 >= 5.0D);
        }

        private void m31()
        {
            SDK.Drive((int)SDK.Round(this.f37), this.f38);
        }

        private double m32()
        {
            double d1;
            if (this.f48 == f6)
            {
                double d2 = SDK.Rand(1000) >= this.f50 ? -1.0D : 1.0D;
                double d3 = 90.0D + (SDK.Rand(20) - 10);
                d1 = this.f92 + d2*d3;
                this.f50 = ((int) (this.f50 - d2*100.0D));
            }
            else
            {
                d1 = this.f48[this.f49] + (SDK.Rand(30) - 15);
                this.f49 = ((this.f49 + 1)%this.f48.Length);
            }
            this.f92 = d1;
            return d1;
        }

        private void m33()
        {
            if (this.f87 >= this.f26 - 0.03D)
            {
                while ((m59(false)) && (m34()))
                {
                    double d3 = this.f78 - SDK.LocX;
                    double d4 = this.f79 - SDK.LocY;
                    double d1 = m74(d3, d4);
                    double d2 = m73(d3, d4, 0.0D, 0.0D);
                    int i;
                    if ((i = m72(d1, d2) ? 0 : 1) != 0)
                    {
                        m2();
                    }
                    if (i == 0)
                    {
                        m67(this.f53, this.f54, m63(d2));
                    }
                }
            }
        }

        private bool m34()
        {
            double d1 = this.f26 > this.f87 ? this.f26 : this.f87;
            double d2 = this.f52;
            for (int i = 0; i < 2; i++)
            {
                double d3 = d1 + d2/300.0D;
                if ((f18 <= 2) && (this.f65 <= 4))
                {
                    this.f78 = this.f53;
                    this.f79 = this.f54;
                }
                else if (m37())
                {
                    m40(d3);
                }
                else if (!m41(d3))
                {
                    if (m35())
                    {
                        return false;
                    }
                    m40(d3);
                }
                if ((m73(this.f78, this.f79, this.f53, this.f54) > 100.0D) || (this.f78 <= 0.0D) || (this.f78 >= 1000.0D) || (this.f79 <= 0.0D) || (this.f79 >= 1000.0D))
                {
                    this.f78 = this.f53;
                    this.f79 = this.f54;
                }
                double d4 = m73(this.f90, this.f91, this.f78, this.f79);
                if ((f18 <= 2) || (SDK.Abs(d4 - d2) < 3.0D))
                {
                    break;
                }
                d2 = d4;
            }
            return true;
        }

        private bool m35()
        {
            double d1 = SDK.Time;
            if ((f18 > 2) && (d1 < 20.0D) && (this.f65 < 3) && (this.f52 > 300.0D) && (this.f89 < 80))
            {
                this.f26 = (d1 + 0.05D);
                return true;
            }
            if (d1 < 4.0D)
            {
                return false;
            }
            if (d1 - this.f61 < 10.0D)
            {
                return false;
            }
            if (this.f89 >= 90)
            {
                return false;
            }
            if (this.f52 <= 100.0D)
            {
                return false;
            }
            if (d1 >= this.f27 + 1.0D + 0.25D)
            {
                return false;
            }
            if (!m46(4))
            {
                return false;
            }
            if (this.f83 >= 0.3D)
            {
                return false;
            }
            double d2 = 0.4D;
            double d3 = 0.1D;
            if (d2 > this.f82 - this.f83/2.0D)
            {
                return false;
            }
            if (this.f82 < d2 + d3)
            {
                return false;
            }
            m50();
            double d4 = -this.f85;
            double d5 = this.f86;
            double d6;
            if ((d6 = this.f52/300.0D) > d5 + this.f82)
            {
                return false;
            }
            double d7;
            if (d6 < d5)
            {
                if (d4 >= d2)
                {
                    d7 = 0.0D;
                }
                else
                {
                    d7 = d6;
                }
            }
            else if (d4 >= d2)
            {
                d7 = d6 - d5;
            }
            else
            {
                d7 = m73(d5, d6 - d5, 0.0D, 0.0D);
            }
            double d8;
            if (d6 < d5)
            {
                d8 = 0.0D;
            }
            else
            {
                d8 = d6 - d5;
            }
            double d9;
            if (d6 < this.f82 - d2)
            {
                d9 = 0.0D;
            }
            else
            {
                d9 = d6 - (this.f82 - d2);
            }
            int i = m36(d7);
            int j = m36(d8);
            int k = m36(d9);
            int m = i >= j ? 0 : 1;
            int n = i >= k ? 0 : 1;
            if ((m == 0) && (n == 0))
            {
                return false;
            }
            double d10;
            if (j >= k)
            {
                d10 = d2 - d4 + this.f83/2.0D;
            }
            else
            {
                d10 = this.f86 + d2 + this.f83/2.0D;
            }
            if (d10 >= 1.0D)
            {
                return false;
            }
            if (d10 > 0.25D)
            {
                d10 = 0.25D;
            }
            else
            {
                this.f61 = d1;
            }
            this.f26 = (d1 + d10);
            return true;
        }

        private int m36(double paramDouble)
        {
            paramDouble *= 15.0D;
            if (paramDouble < 5.0D)
            {
                return 10;
            }
            if (paramDouble < 20.0D)
            {
                return 5;
            }
            if (paramDouble < 40.0D)
            {
                return 3;
            }
            return 0;
        }

        private bool m37()
        {
            double d = f18 > 2 ? 1.0D : 5.0D;
            if ((this.f71 >= 2) && (this.f87 - this.f68[(this.f71 - 1)] < d))
            {
                return false;
            }
            return m38(d) >= 20.0D;
        }

        private double m38(double paramDouble)
        {
            int i;
            for (i = this.f65 - 1; (i >= 0) && (this.f87 - this.f64[i] < paramDouble); i--) 
                ;
            if (i < 0)
            {
                return 10.0D;
            }
            return m39(i, this.f65 - 1);
        }

        private double m39(int paramInt1, int paramInt2)
        {
            double d1 = m81(this.f74, paramInt1, paramInt2);
            double d2 = m81(this.f75, paramInt1, paramInt2);
            double d3 = m81(this.f76, paramInt1, paramInt2);
            return m73(d1, d2, 0.0D, 0.0D)/d3;
        }

        private void m40(double paramDouble)
        {
            int i;
            int j = (i = this.f65 - 1) - 2;
            double d1 = this.f52/300.0D;
            double d2 = 0.0D;
            int k = 0;
            int m;
            if (((m = (f18 <= 2) || ((this.f87 >= 7.0D) && ((this.f87 >= 20.0D) || (this.f65 < 3) || (this.f65 > 10))) ? 0 : 1) != 0) && (this.f87 > 1.5D) && (m38(1.0D) < 5.0D))
            {
                m = 0;
            }
            double d3;
            double d4;
            if (j >= 0)
            {
                if (m37())
                {
                    d2 = 1.0D;
                    k = 1;
                    j = i - 3;
                    do
                    {
                        j--;
                        if (j <= 0)
                        {
                            break;
                        }
                    } while (this.f64[(j - 1)] > this.f68[(this.f71 - 1)]);
                }
                else if (m != 0)
                {
                    d2 = 1.0D;
                    k = 1;
                    j = i - 6;
                }
                else if ((this.f71 >= 3) && (m46(4)) && (this.f83 <= 1.2D))
                {
                    m50();
                    d3 = 0.4D;
                    j = i - 1;
                    d4 = this.f87 + this.f85 - 0.05D;
                    while ((j > 0) && (this.f64[(j - 1)] >= d4))
                    {
                        j--;
                    }
                    if (-this.f85 > d3)
                    {
                        if ((d2 = this.f86/d1) < 0.0D)
                        {
                            d2 = 0.0D;
                        }
                        else if (d2 > 1.0D)
                        {
                            d2 = 1.0D;
                        }
                        k = 1;
                    }
                }
            }
            if ((k == 0) && (this.f71 >= 3))
            {
                m47();
                if ((d2 = (this.f82 - (this.f87 - this.f107))/d1) > 1.0D)
                {
                    d2 = 1.0D;
                }
                else if (d2 < 0.0D)
                {
                    d2 = 0.0D;
                }
                k = 1;
            }
            if ((k == 0) && (this.f71 <= 4))
            {
                d2 = 0.75D;
                k = 1;
                j = i - 1;
            }
            double d5;
            if ((k != 0) && (m == 0) && (this.f65 >= 6))
            {
                if ((d5 = ((m38(1.0D) - 15.0D)/5.0D - 0.5D)/d1) > 1.0D)
                {
                    d5 = 1.0D;
                }
                if (d2 < d5)
                {
                    d2 = d5;
                }
            }
            if (k != 0)
            {
                if (j < 0)
                {
                    j = 0;
                }
                d3 = this.f64[j];
                d4 = this.f62[j];
                d5 = this.f63[j];
                double d6 = this.f55;
                double d7 = this.f53;
                double d8 = this.f54;
                double d10 = m74(d7 - d4, d8 - d5);
                double d9;
                if ((f18 > 2) && (this.f87 < 6.0D) && (d2 == 1.0D))
                {
                    d9 = 2.5D*d1*(2.0D*this.f87 + d1);
                }
                else if (d6 > d3)
                {
                    d9 = d2*d1*m73(d4, d5, d7, d8)/(d6 - d3);
                }
                else
                {
                    d9 = 0.0D;
                }
                this.f78 = (d7 + d9*m76(d10));
                this.f79 = (d8 + d9*m75(d10));
                if (m73(this.f78, this.f79, this.f53, this.f54) > 70.0D)
                {
                    this.f78 = this.f53;
                    this.f79 = this.f54;
                }
            }
            else
            {
                this.f78 = this.f53;
                this.f79 = this.f54;
            }
        }

        private bool m41(double paramDouble)
        {
            if (f18 > 2)
            {
                return false;
            }
            if (!m46(5))
            {
                return false;
            }
            m54();
            if (this.f52 > 250.0D)
            {
                if ((this.f52 < 500.0D) && (m42(paramDouble, f19, 3, 18)))
                {
                    return true;
                }
                if ((this.f52 > 500.0D) && (m42(paramDouble, f20, 4, 15)))
                {
                    return true;
                }
                if ((this.f52 > 500.0D) && (m42(paramDouble, f21, 4, 15)))
                {
                    return true;
                }
            }
            if ((f18 == 2) && (m42(paramDouble, f22, 4, 15)))
            {
                return true;
            }
            return m42(paramDouble, f23, 4, 20);
        }

        private bool m42(double paramDouble, short[] paramArrayOfShort, int paramInt1, int paramInt2)
        {
            if (this.f72 < paramInt1 + 2)
            {
                return false;
            }
            int i = this.f72 - paramInt1;
            int j = 0;
            int k = 2;
            int m = 0;
            while ((m == 0) && (j < paramArrayOfShort.Length - paramInt1 - k))
            {
                double d = 0.0D;
                for (int n = 0; n < paramInt1; n++)
                {
                    d += SDK.Abs(m79(this.f69[(i + n)]) - paramArrayOfShort[(j + n)]);
                }
                if ((m = d/paramInt1 > paramInt2 ? 0 : 1) == 0)
                {
                    j++;
                }
            }
            if (m == 0)
            {
                return false;
            }
            j += paramInt1 - 1;
            i += paramInt1 - 1;
            m43(this.f87, paramArrayOfShort, j, i);
            if (m73(this.f78, this.f79, this.f53, this.f54) > 15.0D)
            {
                return false;
            }
            m43(paramDouble, paramArrayOfShort, j, i);
            return true;
        }

        private void m43(double paramDouble, short[] paramArrayOfShort, int paramInt1, int paramInt2)
        {
            m71();
            this.f77 = this.f72;
            while (this.f68[paramInt2] > this.f87)
            {
                paramInt2--;
                paramInt1--;
            }
            double d1 = m39(paramInt2 - 1, paramInt2);
            double d2 = m57(paramInt2 - 1, paramInt2) + paramArrayOfShort[paramInt1];
            double d3 = this.f66[paramInt2];
            double d4 = this.f67[paramInt2];
            double d5 = paramDouble - this.f68[paramInt2];
            for (int i = 0; (d5 > 0.0D) && (i < this.f103) && (paramInt1 + i + 1 < paramArrayOfShort.Length); i++)
            {
                double d6;
                double d7;
                double d8 = (d7 = (d6 = this.f101) < d5 ? d6 : d5)*d1;
                double d9 = paramArrayOfShort[(paramInt1 + i + 1)];
                d3 += d8*m76(d2);
                d4 += d8*m75(d2);
                d5 -= d7;
                d2 += d9;
            }
            this.f78 = d3;
            this.f79 = d4;
        }

        private double m44(int paramInt1, int paramInt2)
        {
            double d = m57(paramInt1 - paramInt2, paramInt1);
            int tmp11_10 = paramInt1;
            return m57(tmp11_10, tmp11_10 + paramInt2) - d;
        }

        private int m45(int paramInt)
        {
            int i;
            if ((i = this.f71 - 1 < paramInt ? this.f71 - 1 : paramInt) == 0)
            {
                return 0;
            }
            for (int j = 0; j < i; j++)
            {
                double d = this.f70[(this.f71 - i + j)];
                this.f81[j] = d;
                if ((j == 0) || (d < this.f84))
                {
                    this.f84 = d;
                }
            }
            this.f82 = m82(this.f81, i);
            this.f83 = m83(this.f81, i, this.f82);
            return i;
        }

        private bool m46(int paramInt)
        {
            if (m45(4) == 0)
            {
                return false;
            }
            this.f101 = (this.f83 < 1.2D ? this.f82 : this.f84);
            this.f102 = this.f83;
            this.f103 = paramInt;
            return true;
        }

        private void m47()
        {
            m70();
            double d1;
            if ((d1 = this.f71 > 0 ? this.f68[(this.f71 - 1)] : 0.0D) < this.f87 - 10.0D)
            {
                d1 = this.f87 - 10.0D;
            }
            int i;
            for (i = this.f65 - 1; (i > 0) && (this.f64[(i - 1)] >= d1); i--) 
                ;
            int j = 3;
            this.f106 = 0;
            for (int k = i + 3; (k < this.f65) && (k < this.f104.Length); k++)
            {
                this.f104[this.f106] = m39(k - j, k);
                this.f105[this.f106] = ((this.f64[(k - j)] + this.f64[k])/2.0D);
                this.f106 += 1;
            }
            double d2 = 0.0D;
            int m = 1;
            int n = 1;
            int i1;
            for (i1 = i + j; i1 < this.f65 - 2; i1++)
            {
                int i2 = i1 - i - j;
                double d3 = this.f104[i2];
                if ((m != 0) && (m48(i2, 2)))
                {
                    d1 = d2;
                    m = 0;
                    n = 1;
                }
                else if ((n != 0) && (i2 > 0) && (m49(i2, 2)) && (d3 < 17.0D))
                {
                    if (d3 <= 16.0D)
                    {
                        d1 = this.f64[i1];
                    }
                    m = 1;
                    n = 0;
                }
            }
            this.f107 = d1;
            if (this.f87 - d1 > 1.0D)
            {
                for (i1 = 2; i1 >= 1; i1--)
                {
                    if ((this.f106 >= i1) && (this.f104[(this.f106 - i1)] < 15.0D))
                    {
                        this.f107 = this.f105[(this.f106 - i1)];
                        return;
                    }
                }
            }
        }

        private bool m48(int paramInt1, int paramInt2)
        {
            if (paramInt1 >= this.f106 - paramInt2)
            {
                return false;
            }
            double d;
            if ((d = this.f104[paramInt1]) < 15.0D)
            {
                return false;
            }
            for (int i = paramInt1 + 1; (i < this.f106) && (this.f104[i] > 15.0D); i++)
            {
                if (d < this.f104[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool m49(int paramInt1, int paramInt2)
        {
            if (paramInt1 >= this.f106 - paramInt2)
            {
                return false;
            }
            double d;
            if ((d = this.f104[paramInt1]) > 17.0D)
            {
                return false;
            }
            for (int i = paramInt1 + 1; (i < this.f106) && (this.f104[i] < 17.0D); i++)
            {
                if (d > this.f104[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void m50()
        {
            double d = this.f68[(this.f71 - 1)];
            int i = 0;
            do
            {
                d += this.f101;
                i++;
            } while ((d < this.f87) && (i < this.f103));
            this.f86 = (d - this.f87);
            this.f85 = (this.f86 - this.f101);
        }

        private bool m51(int paramInt)
        {
            double d1;
            if (SDK.Abs(d1 = m52(paramInt)) < 30.0D)
            {
                return false;
            }
            double d2 = m52(paramInt + 1);
            double d3 = m52(paramInt - 1);
            return (SDK.Abs(d1) > SDK.Abs(d2)) && (SDK.Abs(d1) >= SDK.Abs(d3));
        }

        private double m52(int paramInt)
    {
        int tmp2_1 = paramInt;
        double d1 = m57(tmp2_1, tmp2_1 + 2);
        double d2 = m57(paramInt - 2 - 1, paramInt - 1);
        return m80(15.0D, d1 - d2);
    }

        private bool m53()
        {
            if (this.f73 >= this.f65 - 2 - 1)
            {
                return false;
            }
            if ((f18 > 2) && (this.f87 < 20.0D))
            {
                return false;
            }
            int i = this.f73;
            if (m51(i))
            {
                this.f95 = 0.0D;
                this.f96 = 0.0D;
                m55(this.f62, i);
                m55(this.f63, i);
                int j = this.f71;
                this.f68[j] = this.f96;
                this.f66[j] = m56(this.f62, i);
                this.f67[j] = m56(this.f63, i);
                this.f70[j] = (this.f96 - (j > 0 ? this.f68[(j - 1)] : 0.0D));
                this.f71 += 1;
                this.f69[j] = m52(i);
                if (j - 1 > 0)
                {
                    m71();
                    this.f69[(j - 1)] = m44(j - 1, 1);
                    m70();
                }
            }
            this.f73 += 1;
            return true;
        }

        private void m54()
        {
            this.f72 = this.f71;
            if ((this.f71 >= 2) && (this.f83 < 0.5D))
            {
                int i = 4;
                for (int j = 0; j < i; j++)
                {
                    int k = this.f72 - 1;
                    double d1;
                    double d2 = (d1 = this.f68[k]) + this.f101;
                    if (this.f87 - d2 < 0.3D)
                    {
                        break;
                    }
                    int m;
                    for (m = this.f65 - 1; m >= 0; m--)
                    {
                        if (this.f64[m] <= d2)
                        {
                            break;
                        }
                    }
                    if (m < 0)
                    {
                        break;
                    }
                    m71();
                    double d3 = m74(this.f62[m] - this.f66[k], this.f63[m] - this.f67[k]);
                    double d4 = this.f101/(this.f64[m] - d1);
                    double d5 = m39(k - 1, k);
                    this.f68[(k + 1)] = d2;
                    this.f66[(k + 1)] = (this.f66[k] + d4*d5*m76(d3));
                    this.f67[(k + 1)] = (this.f67[k] + d4*d5*m75(d3));
                    this.f69[k] = m44(k, 1);
                    m70();
                    this.f72 += 1;
                }
            }
        }

        private void m55(double[] paramArrayOfDouble, int paramInt)
        {
            int i = paramInt - 3;
            int tmp7_6 = paramInt;
            int tmp18_17 = paramInt;
            double d1 = m81(paramArrayOfDouble, tmp7_6, tmp7_6 + 2)/m81(this.f64, tmp18_17, tmp18_17 + 2);
            int tmp30_28 = i;
            int tmp42_40 = i;
            double d2 = m81(paramArrayOfDouble, tmp30_28, tmp30_28 + 2)/m81(this.f64, tmp42_40, tmp42_40 + 2);
            double d3;
            if ((d3 = SDK.Abs(d1 - d2)) > this.f95)
            {
                this.f95 = d3;
                this.f96 = ((paramArrayOfDouble[i] - d2*this.f64[i] - paramArrayOfDouble[paramInt] + d1*this.f64[paramInt])/(d1 - d2));
            }
        }

        private double m56(double[] paramArrayOfDouble, int paramInt)
        {
            int i;
            int j = (i = paramInt - 1) - 2;
            double d1 = this.f64[j];
            double d2 = this.f64[i];
            double d3 = paramArrayOfDouble[j];
            double d4 = paramArrayOfDouble[i];
            double d5 = this.f96;
            if (d2 - d1 == 0.0D)
            {
                return d4;
            }
            return d4 + (d5 - d2)*(d4 - d3)/(d2 - d1);
        }

        private double m57(int paramInt1,int paramInt2)
    {
        return m74(m81(this.f74, paramInt1, paramInt2), m81(this.f75, paramInt1, paramInt2));
    }

        private void m58()
        {
            if ((this.f87 >= this.f30) && (m59(true)))
            {
                this.f30 = (this.f87 + 0.25D);
            }
        }

        private bool m59(bool parambool)
        {
            for (int i = 0; i < 2; i++)
            {
                m60();
                if (this.f57 != 0)
                {
                    int j = (int)SDK.Round(this.f51);
                    for (int k = 0; k <= 180; k++)
                    {
                        if ((m64(j + k, parambool)) || (m64(j - k, parambool)))
                        {
                            return true;
                        }
                    }
                    this.f32 = true;
                }
            }
            return false;
        }

        private void m60()
        {
            if ((f18 == 1) && (!this.f32))
            {
                this.f57 = 1;
                return;
            }
            if ((this.f32) || (this.f87 - this.f31 >= 3.0D))
            {
                int i;
                if (this.f32)
                {
                    i = 1;
                }
                else if (f18 == 2)
                {
                    i = 1;
                }
                else
                {
                    int j = this.f52 >= 700.0D ? 0 : 1;
                    int k = this.f87 - this.f31 >= 1.0D ? 0 : 1;
                    i = (j != 0) && (k != 0) ? 0 : 1;
                }
                double d1 = 0.0D;
                double d2 = 9999.0D;
                double d3 = -10000000.0D;
                int m = 1;
                this.f58 = 99999.0D;
                this.f59 = 0.0D;
                this.f56 = 0;
                this.f57 = 0;
                double d4 = 0.0D;
                double d5 = 0.0D;
                double d6 = 0.0D;
                this.f90 = SDK.LocX;
                this.f91 = SDK.LocY;
                int n = 0;
                while (n < 360)
                {
                    int i1;
                    if ((i1 = SDK.Scan(n, m)) != 0)
                    {
                        double d7 = this.f90 + i1*m76(n);
                        double d8 = this.f91 + i1*m75(n);
                        if (!m65(d7, d8, i1, m))
                        {
                            if (i1 < this.f58)
                            {
                                this.f58 = i1;
                            }
                            if (i1 > this.f59)
                            {
                                this.f59 = i1;
                            }
                            if (i1 < 1440)
                            {
                                this.f56 += 1;
                            }
                            this.f57 += 1;
                            m67(d7, d8, 0.0D);
                            double d10;
                            double d13;
                            double d9;
                            IonStorm localIonStorm;
                            if (f18 == 2)
                            {
                                if (!m3(1 - this.f24))
                                {
                                    d10 = m68(d7, d8);
                                    d13 = m63(i1);
                                    d9 = -((100.0D - d10)/(d13 + 0.001D));
                                }
                                else
                                {
                                    if (this.f100 == null)
                                    {
                                        this.f100 = f17[(1 - this.f24)];
                                    }
                                    d10 = (this.f90 + this.f100.f90)/2.0D;
                                    d13 = (this.f91 + this.f100.f91)/2.0D;
                                    d9 = m62(d7, d8, true)*600.0D + m61(i1, m73(d10, d13, d7, d8));
                                }
                            }
                            else if ((this.f41) || (f18 == 1))
                            {
                                d9 = -i1;
                            }
                            else
                            {
                                d9 = 0.0D;
                                if (this.f44)
                                {
                                    d10 = this.f90 + 75.0D*m76(this.f37);
                                    d13 = this.f91 + 75.0D*m75(this.f37);
                                    double d15 = m73(d10, d13, d7, d8);
                                    d9 += 2.0D*m12(d15, true);
                                }
                                else
                                {
                                    d9 += m12(i1, true);
                                }
                                int i2 = 0;
                                for (int i3 = 0; i3 < f18; i3++)
                                {
                                    if ((i3 != this.f24) && (m3(i3)))
                                    {
                                        localIonStorm = f17[i3];
                                        if (m73(d7, d8, localIonStorm.f90, localIonStorm.f91) < 40.0D)
                                        {
                                            d9 -= 1000.0D;
                                        }
                                        if ((localIonStorm.f65 != 0) && (m73(d7, d8, localIonStorm.f53, localIonStorm.f54) < 10.0D))
                                        {
                                            i2++;
                                        }
                                    }
                                }
                                double d12 = 10*(1 - i1/700);
                                if (i2 > 0)
                                {
                                    if (i2 <= 4)
                                    {
                                        d9 += d12*2.0D;
                                    }
                                    else
                                    {
                                        d9 -= d12;
                                    }
                                }
                                if (m73(d7, d8, this.f53, this.f54) < 10.0D)
                                {
                                    d9 += 10.0D;
                                }
                                if (i1 < 250)
                                {
                                    for (i2 = 0; i2 < f18; i2++)
                                    {
                                        if ((i2 != this.f24) && (m3(i2)) && ((d12 = m73(f9[i2], f10[i2], d7, d8)) < 100.0D))
                                        {
                                            d9 += m12(d12, true);
                                        }
                                    }
                                }
                            }
                            if ((f18 == 2) && (this.f34))
                            {
                                double d11 = m62(d7, d8, false);
                                if (this.f57 == 2)
                                {
                                    localIonStorm = f17[(1 - this.f24)];
                                    double d14 = m63(m73(d7, d8, this.f90, this.f91)) + m63(m73(d5, d6, this.f90, this.f91));
                                    double d16 = m63(m73(d7, d8, localIonStorm.f90, localIonStorm.f91)) + m63(m73(d5, d6, localIonStorm.f90, localIonStorm.f91));
                                    double d17 = d11 > d4 ? d11 : d4;
                                    double d18 = d14 > d16 ? d14 : d16;
                                    if (d17 - d18 > 4.0D)
                                    {
                                        this.f33 = 1;
                                    }
                                    else if (d18 - d17 > 4.0D)
                                    {
                                        this.f33 = 2;
                                    }
                                    else
                                    {
                                        this.f33 = 0;
                                    }
                                    this.f34 = false;
                                }
                                d4 = d11;
                                d5 = d7;
                                d6 = d8;
                            }
                            if (d9 > d3)
                            {
                                d3 = d9;
                                d2 = i1;
                                d1 = n;
                            }
                        }
                    }
                    n += m;
                }
                if (i != 0)
                {
                    this.f51 = d1;
                    this.f52 = d2;
                }
                this.f31 = this.f87;
                this.f32 = false;
            }
        }

        private double m61(double paramDouble1, double paramDouble2)
        {
            double d;
            if (paramDouble1 > 700.0D)
            {
                d = paramDouble1;
            }
            else if (paramDouble1 < 125.0D)
            {
                d = paramDouble1 - 1000.0D;
            }
            else
            {
                d = paramDouble2;
            }
            return -d;
        }

        private double m62(double paramDouble1, double paramDouble2, bool parambool)
        {
            double d1 = 0.0D;
            if ((parambool) && (m73(this.f90, this.f91, paramDouble1, paramDouble2) > 740.0D))
            {
                return -10.0D;
            }
            for (int i = 0; i < f18; i++)
            {
                if (m3(i))
                {
                    double d2;
                    double d3;
                    if ((d2 = m73(f9[i], f10[i], paramDouble1, paramDouble2)) < 740.0D)
                    {
                        d3 = m63(d2);
                    }
                    else if (parambool)
                    {
                        d3 = 0.1D;
                    }
                    else
                    {
                        d3 = (d2 - 700.0D - 40.0D)/300.0D;
                    }
                    d1 += d3;
                }
            }
            return d1;
        }

        private double m63(double paramDouble)
        {
            if (paramDouble < 90.0D)
            {
                return 10.0D;
            }
            if (paramDouble < 130.0D)
            {
                return 10.0D - (paramDouble - 90.0D)*0.075D;
            }
            if (paramDouble < 400.0D)
            {
                return 7.0D - (paramDouble - 130.0D)*0.01111111111111111D;
            }
            if (paramDouble < 700.0D)
            {
                return 4.0D - (paramDouble - 400.0D)*0.005D;
            }
            if (paramDouble < 740.0D)
            {
                return (740.0D - paramDouble)*0.075D;
            }
            return 0.0D;
        }

        private bool m64(int paramInt, bool parambool)
        {
            paramInt = m77(paramInt);
            int i;
            if ((i = SDK.Scan(paramInt, 1)) <= 0)
            {
                return false;
            }
            if (SDK.Abs(this.f52 - i) > 25.0D)
            {
                return false;
            }
            double d1 = 0.0D;
            for (int k = 1; k >= -1; k -= 2)
            {
                int j;
                if (((j = SDK.Scan(paramInt + k, 2)) > 0) && (SDK.Abs(j - i) < 3.0D))
                {
                    d1 += k*0.25D;
                }
            }
            double d2 = SDK.Time;
            double d3 = paramInt + d1;
            double d4 = i;
            double d5 = SDK.LocX + d4*m76(d3);
            double d6 = SDK.LocY + d4*m75(d3);
            m75(0.5D);
            //(0.5D*d4);
            if (m65(d5, d6, d4, 0.5D))
            {
                return false;
            }
            int m = 1;
            if (m73(d5, d6, this.f53, this.f54) > 20.0D)
            {
                m = 0;
                if (f18 == 2)
                {
                    m20();
                }
                this.f65 = 0;
                this.f71 = 0;
                this.f72 = 0;
                this.f73 = 4;
                for (int n = 0; n < f18; n++)
                {
                    if (n != this.f24)
                    {
                        IonStorm localIonStorm = f17[n];
                        if ((this.f87 - localIonStorm.f55 <= 0.5D) && (m73(d5, d6, localIonStorm.f53, localIonStorm.f54) <= 15.0D) && (localIonStorm.f65 > this.f65))
                        {
                            for (int i1 = 0; i1 < localIonStorm.f65; i1++)
                            {
                                this.f62[i1] = localIonStorm.f62[i1];
                                this.f63[i1] = localIonStorm.f63[i1];
                                this.f64[i1] = localIonStorm.f64[i1];
                            }
                            while (m53())
                            {
                            }
                        }
                    }
                }
                this.f32 = (this.f65 == 0);
            }
            if ((m != 0) && (this.f55 > 0.0D) && (d4 > 300.0D))
            {
                double d7 = (d5 + this.f53)/2.0D;
                double d8 = (d6 + this.f54)/2.0D;
                double d9 = (d2 + this.f55)/2.0D;
                m66(d9, d7, d8, parambool);
            }
            else
            {
                m66(d2, d5, d6, parambool);
            }
            this.f53 = d5;
            this.f54 = d6;
            this.f55 = d2;
            this.f51 = d3;
            this.f52 = d4;
            m67(d5, d6, 0.0D);
            return true;
        }

        private bool m65(double paramDouble1, double paramDouble2, double paramDouble3, double paramDouble4)
        {
            double d = 3.0D + paramDouble3*m75(paramDouble4)/2.0D;
            for (int i = 0; i < f18; i++)
            {
                if ((i != this.f24) && (m3(i)) && (m73(paramDouble1, paramDouble2, f9[i], f10[i]) < d))
                {
                    return true;
                }
            }
            return false;
        }

        private void m66(double paramDouble1, double paramDouble2, double paramDouble3, bool parambool)
        {
            if (parambool)
            {
                this.f62[this.f65] = paramDouble2;
                this.f63[this.f65] = paramDouble3;
                this.f64[this.f65] = paramDouble1;
                this.f65 += 1;
            }
            if (this.f71 == 0)
            {
                this.f68[0] = paramDouble1;
                this.f66[0] = paramDouble2;
                this.f67[0] = paramDouble3;
                this.f71 = 1;
            }
        }

        private void m67(double paramDouble1, double paramDouble2, double paramDouble3)
        {
            if (f18 <= 2)
            {
                int i = m69(paramDouble1, paramDouble2);
                f13[i] = paramDouble1;
                f14[i] = paramDouble2;
                f15[i] = this.f87;
                f16[i] += paramDouble3;
            }
        }

        private double m68(double paramDouble1, double paramDouble2)
        {
            int i = m69(paramDouble1, paramDouble2);
            double d;
            if ((d = f16[i]) > 99.0D)
            {
                d = 99.0D;
            }
            return d;
        }

        private int m69(double paramDouble1, double paramDouble2)
        {
            int i = 0;
            double d1 = 100000.0D;
            int j = -1;
            for (int k = 0; k < f13.Length; k++)
            {
                int m = this.f87 - f15[k] < 4.0D ? 0 : 1;
                double d2 = m73(paramDouble1, paramDouble2, f13[k], f14[k]);
                if ((j < 0) && (m != 0))
                {
                    j = k;
                }
                else if ((m == 0) && (d2 < d1))
                {
                    i = k;
                    d1 = d2;
                }
            }
            if (d1 < 100.0D)
            {
                return i;
            }
            if (j >= 0)
            {
                f16[j] = 0.0D;
                return j;
            }
            return i;
        }

        private void m70()
        {
            this.f74 = this.f62;
            this.f75 = this.f63;
            this.f76 = this.f64;
            this.f77 = this.f65;
        }

        private void m71()
        {
            this.f74 = this.f66;
            this.f75 = this.f67;
            this.f76 = this.f68;
            this.f77 = this.f71;
        }

        private bool m72(double paramDouble1, double paramDouble2)
        {
            int i = (int)SDK.Round(paramDouble1);
            int j = (int)SDK.Round(paramDouble2);
            while (SDK.Cannon(i, j) == 0)
            {
                this.f87 = SDK.Time;
                if (this.f87 - this.f26 > 0.03D)
                {
                    return false;
                }
            }
            this.f27 = SDK.Time;
            this.f26 = (this.f27 + 1.0D);
            return true;
        }

        private double m73(double paramDouble1, double paramDouble2, double paramDouble3, double paramDouble4)
        {
            double d1 = paramDouble1 - paramDouble3;
            double d2 = paramDouble2 - paramDouble4;
            double tmp14_12 = d1;
            double tmp18_16 = d2;
            return SDK.Sqrt(tmp14_12*tmp14_12 + tmp18_16*tmp18_16);
        }

        private double m74(double paramDouble1, double paramDouble2)
        {
            if (paramDouble1 == 0.0D)
            {
                return paramDouble2 > 0.0D ? 90 : 270;
            }
            double d1 = SDK.Abs(paramDouble1);
            double d2 = SDK.ATan(SDK.Abs(paramDouble2)/d1)*57.295779513100001D;
            if (paramDouble1 > 0.0D)
            {
                if (paramDouble2 >= 0.0D)
                {
                    return d2;
                }
                return 360.0D - d2;
            }
            if (paramDouble2 >= 0.0D)
            {
                return 180.0D - d2;
            }
            return 180.0D + d2;
        }

        private double m75(double paramDouble)
        {
            return SDK.Sin(paramDouble*0.0174532925D);
        }

        private double m76(double paramDouble)
        {
            return SDK.Cos(paramDouble*0.0174532925D);
        }

        private int m77(int paramInt)
        {
            while (paramInt < 0)
            {
                paramInt += 360;
            }
            return paramInt%360;
        }

        private double m78(double paramDouble)
        {
            return m80(180.0D, paramDouble);
        }

        private double m79(double paramDouble)
        {
            return m80(0.0D, paramDouble);
        }

        private double m80(double paramDouble1, double paramDouble2)
        {
            while (paramDouble2 < paramDouble1 - 180.0D)
            {
                paramDouble2 += 360.0D;
            }
            while (paramDouble2 >= paramDouble1 + 180.0D)
            {
                paramDouble2 -= 360.0D;
            }
            return paramDouble2;
        }

        private double m81(double[] paramArrayOfDouble, int paramInt1, int paramInt2)
        {
            return paramArrayOfDouble[paramInt2] - paramArrayOfDouble[paramInt1];
        }

        private double m82(double[] paramArrayOfDouble, int paramInt)
        {
            double d = 0.0D;
            for (int i = 0; i < paramInt; i++)
            {
                d += paramArrayOfDouble[i];
            }
            if (paramInt > 0)
            {
                return d/paramInt;
            }
            return 0.0D;
        }

        private double m83(double[] paramArrayOfDouble, int paramInt, double paramDouble)
        {
            double d1 = 0.0D;
            double d2 = 0.0D;
            for (int i = 0; i < paramInt; i++)
            {
                double d3;
                if ((d3 = paramArrayOfDouble[i] - paramDouble) > d2)
                {
                    d2 = d3;
                }
                else if (d3 < d1)
                {
                    d1 = d3;
                }
            }
            return d2 - d1;
        }

        private void m84()
        {
            double d = -20.0D;
            m73(this.f90, this.f91, 500.0D, 500.0D);
            m73(0.0D, 0.0D, 500.0D, 500.0D);
            for (int i = 0; i < f18; i++)
            {
                if ((i != this.f24) && (m3(i)))
                {
                    m88(f9[i], f10[i], d, 2.8D, true, true);
                }
            }
        }

        private void m85()
        {
            for (int i = 0; i < 4; i++)
            {
                m88(f7[i], f8[i], -0.5D, 2.3D, true, false);
            }
        }

        private void m86(double paramDouble)
        {
            m88(this.f53, this.f54, paramDouble*700.0D, 3.0D, false, false);
        }

        private bool m87()
        {
            double d1 = 999999.0D;
            double d2 = 0.0D;
            double d5;
            double d4;
            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -2; j <= 2; j++)
                {
                    d4 = this.f51 + i*90 + j*15;
                    if (((d5 = SDK.Scan((int)SDK.Round(d4), 15)) > 500.0D) && (d5 < d1))
                    {
                        d1 = d5;
                        d2 = d4;
                    }
                }
            }
            if ((d1 < 500.0D) || (d1 > 1500.0D))
            {
                return false;
            }
            double d3 = this.f90 + d1*m76(d2);
            d4 = this.f91 + d1*m75(d2);
            if (m65(d3, d4, d1, 15.0D))
            {
                return false;
            }
            double d6 = (d5 = 750.0D) + 10.0D;
            double d7;
            if ((d7 = d1 > d6 ? 1 : d1 < d5 ? -40 : 0) != 0.0D)
            {
                m88(d3, d4, d7*700.0D, 3.0D, true, false);
                return true;
            }
            return false;
        }

        private void m88(double paramDouble1, double paramDouble2, double paramDouble3, double paramDouble4, bool parambool1, bool parambool2)
        {
            double d1 = 3.0D;
            if ((parambool2) && ((this.f90 < d1) || (this.f90 > 1000.0D - d1) || (this.f91 < d1) || (this.f91 > 1000.0D - d1)))
            {
                return;
            }
            double d2 = paramDouble1 - this.f90;
            double d3 = paramDouble2 - this.f91;
            double d4 = m73(paramDouble1, paramDouble2, this.f90, this.f91);
            double d5 = SDK.Exp(paramDouble4*SDK.Log(d4));
            double d6 = d2*paramDouble3/d5;
            double d7 = d3*paramDouble3/d5;
            if (parambool1)
            {
                double d8 = m76(this.f51);
                double d9 = m75(this.f51);
                double d10 = d8*d6 + d9*d7;
                d6 -= d10*d8;
                d7 -= d10*d9;
            }
            this.f97[this.f99] = d6;
            this.f98[this.f99] = d7;
            this.f99 += 1;
        }

        private double m89()
        {
            double d1 = 0.0D;
            double d2 = 0.0D;
            for (int i = 0; i < this.f99; i++)
            {
                d1 += this.f97[i];
                d2 += this.f98[i];
            }
            return m74(d1, d2);
        }
    }
}
