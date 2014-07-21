using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robots
{
    public class Mouth : SDK.Robot
    {
        private static int f1;
        private static int[] f2 = new int[8];
        private static int[] f3 = new int[8];
        private static int[] f4 = { 100, 900, 900, 100 };
        private static int[] f5 = { 100, 100, 900, 900 };
        private static int f6;
        private double f7;
        private double f8;
        private double f9;
        private double f10;
        private double f11;
        private double f12;
        private int f13;
        private double f14;
        private double f15;
        private double[] f16 = new double[4];
        private double[] f17 = new double[4];
        private int f18;
        private int f19;
        private int f20;
        private int f21;
        private int f22;
        private int f23;
        private int f24;
        private int f25;
        private int f26;
        private int f27;
        private int f28;
        private int f29;
        private int f30;
        private int f31;
        private int f32;
        private bool f33;
        private bool f34;
        private bool f35;
        private int f36;
        private int f37;

        public override void Init()
        {
            this.f37 = 1;
            this.f11 = (SDK.Time + 0.5D);
            this.f12 = SDK.Time;
            this.f33 = true;
            this.f27 = (m11(500.0D, 500.0D) - 180);
            this.f13 = SDK.Damage;
            this.f15 = 0.0D;
            this.f30 = 800;
            this.f34 = false;
            this.f35 = false;
            if ((this.f19 = SDK.Id) == 0)
            {
                f1 = 1;
            }
            else
            {
                f1 = this.f19 + 1;
            }
            f2[this.f19] = SDK.LocX;
            f3[this.f19] = SDK.LocY;
            m2();
            if ((f1 == 8) && (SDK.Time < 0.05D))
            {
                this.f31 = (f2[0] + f2[1] + f2[2] + f2[3] + f2[4] + f2[5] + f2[6] + f2[7]);
                this.f32 = (f3[0] + f3[1] + f3[2] + f3[3] + f3[4] + f3[5] + f3[6] + f3[7]);
                if ((this.f31 >> 3 < 500) && (this.f32 >> 3 < 500))
                {
                    f6 = 0;
                }
                if ((this.f31 >> 3 > 500) && (this.f32 >> 3 < 500))
                {
                    f6 = 1;
                }
                if ((this.f31 >> 3 < 500) && (this.f32 >> 3 > 500))
                {
                    f6 = 3;
                }
                if ((this.f31 >> 3 > 500) && (this.f32 >> 3 > 500))
                {
                    f6 = 2;
                }
            }
            this.f22 = this.f21;
            m1();
        }

        public override void Step()
        {
            if (this.f12 + 1.0D < SDK.Time)
            {
                m2();
                this.f12 = SDK.Time;
            }
            if ((f1 == 1) || (f1 == 2))
            {
                m5();
            }
            else
            {
                if (this.f37 == 1)
                {
                    m3();
                }
                if (this.f37 == 2)
                {
                    m4();
                }
            }
        }


        private void m1()
        {
            this.f29 = m11(f4[f6], f5[f6]);
        }

        private void m2()
        {
            this.f23 = 1800;
            this.f24 = 360;
            for (this.f21 = 0; this.f21 < 360; this.f21 += 1)
            {
               Mouth tmp24_22 = this;
                this.f20 = tmp24_22.SDK.Scan(tmp24_22.f21, 1);
                if ((this.f20 < this.f23) && (this.f20 != 0))
                {
                    this.f7 = ((f2[this.f19] = SDK.LocX) + this.f20 * SDK.Cos(this.f21) / 100000.0D);
                    this.f8 = ((f3[this.f19] = SDK.LocY) + this.f20 * SDK.Sin(this.f21) / 100000.0D);
                    if (!m8())
                    {
                        this.f23 = this.f20;
                        this.f24 = this.f21;
                    }
                }
            }
            this.f11 = SDK.Time;
            if (m12() > 3)
            {
                this.f30 -= 2;
            }
            if ((this.f23 > 0) && (this.f23 < 1799))
            {
                this.f21 = this.f24;
                this.f20 = this.f23;
                m6();
                this.f9 = (SDK.LocX + this.f20 * SDK.Cos(this.f24) / 100000);
                this.f10 = (SDK.LocY + this.f20 * SDK.Sin(this.f24) / 100000);
            }
        }

        private void m3()
        {
            SDK.Drive(this.f29, 100);
            if (m10(SDK.LocX, SDK.LocY, f4[f6], f5[f6]) < 180.0D)
            {
                this.f37 = 2;
            }
        }

        private void m4()
        {
            if (m10(SDK.LocX, SDK.LocY, this.f25, this.f26) < this.f28 / 50 + 6)
            {
                SDK.Drive(this.f27, 100);
                return;
            }
            if (SDK.Speed > 48)
            {
                SDK.Drive(this.f27, 40);
                return;
            }
            m9();
            if (this.f28 < this.f30)
            {
                if (this.f36 == 0)
                {
                    this.f27 = (this.f24 - 180 - 30 - SDK.Rand(40));
                    this.f36 = 1;
                }
                else if (this.f36 == 1)
                {
                    this.f27 = (this.f24 - 180 + 90);
                    this.f36 = 2;
                }
                else if (this.f36 == 2)
                {
                    this.f27 = (this.f24 - 180 + 30 + SDK.Rand(40));
                    this.f36 = 3;
                }
                else if (this.f36 == 3)
                {
                    this.f27 = (this.f24 - 180 - 90);
                    this.f36 = 0;
                }
            }
            else
            {
                this.f27 = this.f24;
            }
            if (m7(50))
            {
                if (SDK.LocX < 100)
                {
                    this.f27 = 0;
                    return;
                }
                if (SDK.LocX > 900)
                {
                    this.f27 = 180;
                    return;
                }
                if (SDK.LocY < 100)
                {
                    this.f27 = 90;
                    return;
                }
                if (SDK.LocY > 900)
                {
                    this.f27 = 270;
                }
            }
        }

        private void m5()
        {
            if ((SDK.Damage < 88) && (SDK.Time > 100.0D) && (this.f28 > 640) && (!this.f35))
            {
                if ((SDK.Speed > 49) && (!this.f34))
                {
                    SDK.Drive(this.f27, 40);
                    return;
                }
                this.f34 = true;
                SDK.Drive(this.f24, 100);
                return;
            }
            if ((SDK.Damage > 88) && (this.f28 > 700))
            {
                if (this.f34 == true)
                {
                    this.f35 = true;
                }
                if (SDK.Speed > 49)
                {
                    SDK.Drive(this.f27, 40);
                    return;
                }
                if (!m7(5))
                {
                    if (this.f28 < 780)
                    {
                        SDK.Drive(this.f24 - 180, 49);
                        return;
                    }
                    if (this.f28 < 785)
                    {
                        SDK.Drive(this.f24, 49);
                        return;
                    }
                    SDK.Drive(this.f24, 49);
                    return;
                }
                SDK.Drive(this.f24, 49);
                return;
            }
            if (this.f34 == true)
            {
                this.f35 = true;
            }
            if (m10(SDK.LocX, SDK.LocY, this.f25, this.f26) < this.f28 / 50 + 6)
            {
                SDK.Drive(this.f27, 100);
                return;
            }
            if (SDK.Speed > 48)
            {
                SDK.Drive(this.f27, 40);
                return;
            }
            m9();
            if (this.f28 < this.f30)
            {
                if (this.f36 == 0)
                {
                    this.f27 = (this.f24 - 180 - 10 - SDK.Rand(40));
                    this.f36 = 1;
                }
                else if (this.f36 == 1)
                {
                    this.f27 = (this.f24 - 180 + 90);
                    this.f36 = 2;
                }
                else if (this.f36 == 2)
                {
                    this.f27 = (this.f24 - 180 + 10 + SDK.Rand(40));
                    this.f36 = 3;
                }
                else if (this.f36 == 3)
                {
                    this.f27 = (this.f24 - 180 - 90);
                    this.f36 = 0;
                }
            }
            else
            {
                this.f27 = this.f24;
            }
            if (m7(50))
            {
                if (SDK.LocX < 100)
                {
                    this.f27 = 0;
                    return;
                }
                if (SDK.LocX > 900)
                {
                    this.f27 = 180;
                    return;
                }
                if (SDK.LocY < 100)
                {
                    this.f27 = 90;
                    return;
                }
                if (SDK.LocY > 900)
                {
                    this.f27 = 270;
                }
            }
        }

        private void m6()
        {
            double d2 = SDK.LocX + this.f23 * SDK.Cos((SDK.Deg2Rad(this.f24)));
            double d3 = SDK.LocY + this.f23 * SDK.Sin((SDK.Deg2Rad(this.f24)));
            double d4 = d2;
            double d5 = d3;
            double d6 = d2;
            double d7 = d3;
            if (this.f18 < 4)
            {
                this.f16[this.f18] = d2;
                this.f17[this.f18] = d3;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    this.f16[i] = this.f16[(i + 1)];
                    this.f17[i] = this.f17[(i + 1)];
                    if (this.f16[i] < d4)
                    {
                        d4 = this.f16[i];
                    }
                    if (this.f17[i] < d5)
                    {
                        d5 = this.f17[i];
                    }
                    if (this.f16[i] > d6)
                    {
                        d6 = this.f16[i];
                    }
                    if (this.f17[i] > d7)
                    {
                        d7 = this.f17[i];
                    }
                }
                this.f16[3] = d2;
                this.f17[3] = d3;
            }
            double d8 = d6 - d4;
            double d9 = d7 - d5;
            double tmp259_257 = d8;
            double d10 = tmp259_257 * tmp259_257;
            double tmp265_263 = d9;
            double d11 = tmp265_263 * tmp265_263;
            Mouth tmp271_269 = this;
            this.f28 = ((int)tmp271_269.m10(tmp271_269.SDK.LocX, SDK.LocY, (int)d2, (int)d3));
            double d12 = this.f9 - d2;
            double d13 = this.f10 - d3;
            double tmp314_312 = d12;
            double d14 = tmp314_312 * tmp314_312;
            double tmp320_318 = d13;
            double d15 = tmp320_318 * tmp320_318;
            double d1;
            if (((d14 > 169.0D) || (d15 <= 169.0D)) || (m10((int)d2, (int)d3, (int)this.f9, (int)this.f10) < 10.0D))
            {
                d1 = 0.0D;
            }
            else if (m10((int)d2, (int)d3, (int)this.f9, (int)this.f10) < 14.0D)
            {
                d1 = 0.15D;
            }
            else if (m10((int)d2, (int)d3, (int)this.f9, (int)this.f10) < 18.0D)
            {
                d1 = 0.3D;
            }
            else if (m10((int)d2, (int)d3, (int)this.f9, (int)this.f10) < 25.0D)
            {
                d1 = 0.6D;
            }
            else if (m10((int)d2, (int)d3, (int)this.f9, (int)this.f10) < 37.0D)
            {
                d1 = 1.0D;
            }
            else
            {
                d1 = 0.0D;
            }
            if (this.f28 < 5)
            {
                d1 = 1.0D;
            }
            if ((this.f18 > 3) && (d10 < 1000.0D) && (d11 < 1000.0D))
            {
                Mouth tmp574_573 = this;
                SDK.Cannon(m11((int)d2, (int)d3), (int)tmp574_573.m10(tmp574_573.SDK.LocX, SDK.LocY, (int)d2, (int)d3));
                this.f18 += 1;
                return;
            }
            double d16 = tmp314_312 / 300.0D * this.f23 * d1;
            double d17 = d13 / 300.0D * this.f23 * d1;
            d2 -= d16;
            d3 -= d17;
            if (d2 < 0.0D)
            {
                d2 = 1.0D;
            }
            if (d2 > 1000.0D)
            {
                d2 = 999.0D;
            }
            if (d3 < 0.0D)
            {
                d3 = 1.0D;
            }
            if (d3 > 1000.0D)
            {
                d3 = 999.0D;
            }
            Mouth tmp711_710 = this;
            SDK.Cannon(m11((int)d2, (int)d3), (int)tmp711_710.m10(tmp711_710.SDK.LocX, SDK.LocY, (int)d2, (int)d3));
            this.f18 += 1;
        }

        private bool m7(int paramInt)
        {
            return (SDK.LocX < paramInt) || (SDK.LocX > 1000 - paramInt) || (SDK.LocY < paramInt) || (SDK.LocY > 1000 - paramInt);
        }

        private bool m8()
        {
            if (f1 > 1)
            {
                for (int i = 0; i < f1; i++)
                {
                    if (i != this.f19)
                    {
                        int j = (int)(this.f7 - f2[i]);
                        int k = (int)(this.f8 - f3[i]);
                        int tmp47_46 = j;
                        int tmp50_49 = k;
                        if (tmp47_46 * tmp47_46 + tmp50_49 * tmp50_49 < 1200)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void m9()
        {
            this.f25 = SDK.LocX;
            this.f26 = SDK.LocY;
        }

        private double m10(double paramDouble1, double paramDouble2, double paramDouble3, double paramDouble4)
        {
            double d1 = paramDouble1 - paramDouble3;
            double d2 = paramDouble2 - paramDouble4;
            double tmp14_12 = d1;
            double tmp18_16 = d2;
            return SDK.Sqrt(tmp14_12 * tmp14_12 + tmp18_16 * tmp18_16);
        }

        private int m11(double paramDouble1, double paramDouble2)
        {
            double d1 = SDK.LocX - paramDouble1;
            double d2 = SDK.LocY - paramDouble2;
            double d3;
            if (d1 == 0.0D)
            {
                d3 = d2 <= 0.0D ? 4.7124D : 1.5708D;
            }
            else
            {
                d3 = SDK.ATan(d2 / d1);
                if (d1 < 0.0D)
                {
                    d3 += 3.1416D;
                }
            }
            return (int)(d3 * 180.0D / 3.1416D - 180.0D);
        }

        private int m12()
        {
            if (this.f13 != SDK.Damage)
            {
                this.f15 = SDK.Time;
                this.f13 = SDK.Damage;
                if (this.f30 < 770)
                {
                    this.f30 += 20;
                }
            }
            this.f14 = (SDK.Time - this.f15);
            return (int)this.f14;
        }
    }
}
