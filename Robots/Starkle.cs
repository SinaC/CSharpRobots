using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK;

namespace Robots
{
    // TODO: doesn't work
    public class Starkle : SDK.Robot
    {
        private const double tick = 0.1D;
        private readonly Vector middle = new Vector(500.0D, 500.0D);
        private static Vector[] team = new Vector[8];
        private static Vector[] target = new Vector[8];
        private static double[] imAlive = new double[8];
        private static int teamCount;
        private static int initCount = 0;
        private int lastPr;
        private double lastTurnTime;
        private int myDamage = 0;
        private double lastHitTime;
        private double enemyShotTime;
        private double lastShotTime;
        private int shotCount = 0;
        private Vector me = new Vector();
        private Vector victim = new Vector();
        private Vector victim0 = new Vector();
        private Vector victimBack = new Vector();
        private Vector[] enemyPos = new Vector[1850];
        private Vector prediction = new Vector();
        private Vector goTo = new Vector();
        private const int nStrat = 4;
        private const int qSz1 = 10;
        private Vector[,] evQ = new Vector[4,10];
        private const int dEsz = 3;
        private double[,] distSum = new double[4,3];
        private int[,] damSum = new int[4,3];
        private int damageEst = 0;
        private bool hangFire = false;
        private int dartMode = 0;
        private double dartModeStartTime;
        private int dartModeDamage = 0;
        private int lurkDist = 743;

        public override void Init()
        {
            int i;
            teamCount = SDK.FriendsCount;
            if (initCount == 0)
            {
                for (i = 0; i < teamCount; i++)
                {
                    team[i] = new Vector();
                    target[i] = new Vector();
                    imAlive[i] = 0.0D;
                }
            }
            this.me = location();
            this.goTo = middle - this.me;
            SDK.Drive((int) SDK.Round(this.goTo.A), 60);
            team[SDK.Id] = this.me;
            for (i = 0; i < 1849.0D; i++)
            {
                this.enemyPos[i] = new Vector(0.0D, 0.0D, 0.0D);
            }
            int j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 10; j++)
                {
                    this.evQ[i, j] = new Vector(0.0D, 0.0D, 0.0D);
                }
            }
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    this.distSum[i, j] = 99999.0D;
                    this.damSum[i, j] = 0;
                }
            }
            if (++initCount == teamCount)
            {
                initCount = 0;
            }
        }

        public override void Step()
        {
            while (initCount != 0)
                ;

            this.lastTurnTime = 0.0D;
            findVictim();

            this.me = location();
            team[SDK.Id] = this.me;
            imAlive[SDK.Id] = SDK.Time;
            drive();
            evQdispatch();
            if (this.lastShotTime + 1.0D - SDK.Time > 0.7D)
            {
                this.victim0 = this.victim;
            }
            this.prediction = predict(this.me, this.victim, getVel());
            int i = (int) SDK.Round((this.prediction - this.me).A);
            int j = (int) SDK.Round((this.prediction - this.me).R);
            if ((!this.hangFire) && (j < 770) && (SDK.Cannon(i, j) != 0))
            {
                this.lastShotTime = SDK.Time;
                shot(this.prediction);
                findVictim();
            }
            if (!trackVictim())
            {
                findVictim();
            }
            if (this.myDamage != SDK.Damage)
            {
                int k = SDK.Damage - this.myDamage;
                this.myDamage = SDK.Damage;
                this.lastHitTime = SDK.Time;
                this.enemyShotTime = (this.lastHitTime - this.me.Dist(this.victim)/300.0D);
                if (this.dartMode != 0)
                {
                    this.dartModeDamage += k;
                }
            }
        }
        private void findVictim()
        {
            int i = 0;
            int j = 0;
            Vector localJJVector1 = new Vector();
            Vector localJJVector2 = new Vector();
            int k = 99999;
            for (;;)
            {
                i = 0;
                k = 99999;
                for (;;)
                {
                    i++;
                    if (i > 360)
                    {
                        break;
                    }
                    if ((j = SDK.Scan(i, 1)) != 0)
                    {
                        double d = i;
                        if (SDK.Scan(i + 1, 2) != 0)
                        {
                            d += 0.25D;
                        }
                        else if (SDK.Scan(i - 1, 2) != 0)
                        {
                            d -= 0.25D;
                        }
                        localJJVector1 = Vector.Polar(j, d);
                        localJJVector1.T = SDK.Time;
                        this.me = location();
                        localJJVector2 = localJJVector1 + this.me;
                        localJJVector2.T = SDK.Time;
                        if ((isEnemy(localJJVector2)) && (j < k))
                        {
                            k = j;
                            this.victim = localJJVector2;
                            edgeAdjust(this.victim);
                        }
                    }
                }
                if (k != 99999)
                {
                    break;
                }
            }
            target[SDK.Id] = this.victim;
            if ((teamCount == 2) && (liveTeamCount() == 2))
            {
                int m = SDK.Id == 0 ? 1 : 0;
                if ((target[m].Dist(this.victim) > 10.0D) && (this.me.Dist(this.victim) > target[m].Dist(team[m])) && (this.me.Dist(target[m]) < 740.0D) && (this.me.Dist(this.victim) / this.me.Dist(target[m]) > 0.5D))
                {
                    this.victim = target[m];
                    this.victim0 = this.victim;
                }
            }
        }

        bool isEnemy(Vector paramJJVector)
        {
            for (int i = 0; i < teamCount; i++)
            {
                if (paramJJVector.Dist(team[i]) < 10.0D)
                {
                    return false;
                }
            }
            return true;
        }

        bool trackVictim()
        {
            int i = (int)SDK.Round((this.victim - this.me).A);
            int j = (int)SDK.Round((this.victim - this.me).R);
            int k = 0;
            int m = 1;
            int n = 0;
            for (int i1 = 0; i1 < 3; i1++)
            {
                while (((n = SDK.Scan(i, 1)) == 0) && (k < 10))
                {
                    if (m > 0)
                    {
                        k++;
                    }
                    i += k * m;
                    m = -m;
                }
                if ((n != 0) && (SDK.Abs(n - j) < 10))
                {
                    double d = i;
                    if (SDK.Scan(i + 1, 2) != 0)
                    {
                        d += 0.25D;
                    }
                    else if (SDK.Scan(i - 1, 2) != 0)
                    {
                        d -= 0.25D;
                    }
                    this.me = location();
                    this.victim = this.me + Vector.Polar(n, d);
                    this.victim.T = SDK.Time;
                    edgeAdjust(this.victim);
                    historyPut(this.victim);
                    target[SDK.Id] = this.victim;
                    return true;
                }
            }
            return false;
        }

        void historyPut(Vector paramJJVector)
        {
            int i = timeIndex(paramJJVector.T);
            if (this.enemyPos[i].X == 0.0D)
            {
                this.enemyPos[i] = paramJJVector;
            }
        }

        Vector vel(Vector paramJJVector1, Vector paramJJVector2)
        {
            Vector localJJVector = new Vector();
            double d1 = paramJJVector2.T - paramJJVector1.T;
            double d2 = paramJJVector2.X - paramJJVector1.X;
            double d3 = paramJJVector2.Y - paramJJVector1.Y;
            if (d1 == 0.0D)
            {
                localJJVector = new Vector(0.0D, 0.0D, paramJJVector2.T);
            }
            else
            {
                localJJVector = new Vector(d2 / d1, d3 / d1, paramJJVector2.T);
            }
            return localJJVector;
        }

        Vector getVel()
        {
            return vel(this.victim0, this.victim);
        }

        Vector p1(Vector paramJJVector1, Vector paramJJVector2, Vector paramJJVector3)
        {
            Vector localJJVector = new Vector();
            double d1 = paramJJVector2.X - paramJJVector1.X;
            double d2 = paramJJVector2.Y - paramJJVector1.Y;
            double d3 = 90000.0D * (d1 * d1 + d2 * d2);
            double d4 = (d1 * paramJJVector3.Y - d2 * paramJJVector3.X) * (d1 * paramJJVector3.Y - d2 * paramJJVector3.X);
            double d5 = d1 * paramJJVector3.X + d2 * paramJJVector3.Y;
            double d6 = 90000.0D - (paramJJVector3.X * paramJJVector3.X + paramJJVector3.Y * paramJJVector3.Y);
            double d7 = SDK.Sqrt(d3 - d4 - d5) / d6;
            localJJVector = paramJJVector2 + (paramJJVector3 * d7);
            localJJVector.T  = SDK.Time + d7;
            edgeAdjust(localJJVector);
            return localJJVector;
        }

        Vector p2(Vector paramJJVector1, Vector paramJJVector2, Vector paramJJVector3)
        {
            Vector localJJVector = new Vector();
            localJJVector = paramJJVector2;
            localJJVector.T = SDK.Time + paramJJVector1.Dist(paramJJVector2) / 300.0D;
            edgeAdjust(localJJVector);
            return localJJVector;
        }

        Vector p3(Vector paramJJVector1, Vector paramJJVector2, Vector paramJJVector3)
        {
            Vector localJJVector = new Vector(Vector.Polar(paramJJVector3.R / 2.0D, paramJJVector3.A));
            return p1(paramJJVector1, paramJJVector2, localJJVector);
        }

        void shot(Vector paramJJVector)
        {
            qPut(0, pAdjust(paramJJVector));
            qPut(1, pAdjust(p1(this.me, this.victim, getVel())));
            qPut(2, pAdjust(p2(this.me, this.victim, getVel())));
            qPut(3, pAdjust(p3(this.me, this.victim, getVel())));
            this.shotCount += 1;
        }

        Vector pAdjust(Vector paramJJVector)
        {
            Vector localJJVector = new Vector();
            double d1 = (paramJJVector - this.me).A;
            double d2 = (paramJJVector - this.me).R;
            if (d2 > 700.0D)
            {
                d2 = 700.0D;
            }
            double d3 = d2 / 300.0D;
            localJJVector = this.me + Vector.Polar(d2, d1);
            localJJVector.T = SDK.Time + d3;
            return localJJVector;
        }

        void evQdispatch()
        {
            for (int i = 0; i < 4; i++)
            {
                if ((SDK.Time > this.evQ[i,0].T) && (this.evQ[i,0].T != 0.0D))
                {
                    switch (i)
                    {
                        case 0:
                            shotLandedCB();
                            break;
                        case 1:
                            p1CB();
                            break;
                        case 2:
                            p2CB();
                            break;
                        case 3:
                            p3CB();
                            break;
                    }
                }
            }
        }

        void shotLandedCB()
        {
            trackVictim();
            double d = this.evQ[0,0].Dist(this.victim);
            int i = r2dam(d);
            this.damageEst += i;
            for (int j = 0; j < 2; j++)
            {
                this.distSum[0,j] = this.distSum[0,j + 1];
                this.damSum[0,j] = this.damSum[0,j + 1];
            }
            this.distSum[0,2] = d;
            this.damSum[0,2] = i;
            qPop(0);
        }

        void p1CB()
        {
            double d = this.evQ[1,0].Dist(this.victim);
            int i = r2dam(d);
            for (int j = 0; j < 2; j++)
            {
                this.distSum[1,j] = this.distSum[1,(j + 1)];
                this.damSum[1,j] = this.damSum[1,(j + 1)];
            }
            this.distSum[1,2] = d;
            this.damSum[1,2] = i;
            qPop(1);
        }

        void p2CB()
        {
            double d = this.evQ[2,0].Dist(this.victim);
            int i = r2dam(d);
            for (int j = 0; j < 2; j++)
            {
                this.distSum[2,j] = this.distSum[2,(j + 1)];
                this.damSum[2,j] = this.damSum[2,(j + 1)];
            }
            this.distSum[2,2] = d;
            this.damSum[2,2] = i;
            qPop(2);
        }

        void p3CB()
        {
            double d = this.evQ[3,0].Dist(this.victim);
            int i = r2dam(d);
            for (int j = 0; j < 2; j++)
            {
                this.distSum[3,j] = this.distSum[3,(j + 1)];
                this.damSum[3,j] = this.damSum[3,(j + 1)];
            }
            this.distSum[3,2] = d;
            this.damSum[3,2] = i;
            qPop(3);
        }

        int getBestPr()
        {
            double d1 = 1000000.0D;
            int i = 0;
            int j = 1;
            int k = 1;
            for (int m = 1; m < 4; m++)
            {
                double d2 = 0.0D;
                int n = 0;
                for (int i1 = 0; i1 < 3; i1++)
                {
                    d2 += this.distSum[m,i1];
                    n += this.damSum[m,i1];
                }
                if (d2 < d1)
                {
                    d1 = d2;
                    j = m;
                }
                if (n > i)
                {
                    i = n;
                    k = m;
                }
            }
            this.lastPr = (i == 0 ? j : k);
            return this.lastPr;
        }

        Vector predict(Vector paramJJVector1, Vector paramJJVector2, Vector paramJJVector3)
        {
            Vector localJJVector = new Vector();
            int i = getBestPr();
            switch (i)
            {
                case 1:
                    localJJVector = p1(paramJJVector1, paramJJVector2, paramJJVector3);
                    break;
                case 2:
                    localJJVector = p2(paramJJVector1, paramJJVector2, paramJJVector3);
                    break;
                case 3:
                    localJJVector = p3(paramJJVector1, paramJJVector2, paramJJVector3);
                    break;
                default:
                    localJJVector = new Vector(0.0D, 0.0D, 0.0D);
                    break;
            }
            return localJJVector;
        }

        void drive()
        {
            if (SDK.Speed == 0)
            {
                this.goTo = middle - this.me;
                SDK.Drive((int)this.goTo.A, 100);
                this.lastTurnTime = SDK.Time;
                return;
            }
            if ((teamCount == 1) && (this.damageEst > SDK.Damage + 40))
            {
                SDK.Drive((int)(this.victim - this.me).A, 100);
                return;
            }
            if ((this.me.Dist(this.victim) > 690.0D) && (distToEdge(this.me) > 30.0D))
            {
                lurk();
            }
            else
            {
                this.dartMode = 0;
                drunkWalk();
            }
        }

        void drunkWalk()
        {
            this.hangFire = false;
            double d1 = this.me.Dist(this.victim);
            double d2 = 1.0D;
            if (d1 > 300.0D)
            {
                d2 = 2.0D;
            }
            else if (d1 > 600.0D)
            {
                d2 = 3.0D;
            }
            if (actual_speed() > 15.0D)
            {
                if ((SDK.Time - this.lastTurnTime < 0.45D * d2) && (distToEdge(this.me) > 30.0D))
                {
                    return;
                }
                SDK.Drive((int)this.goTo.A, 50);
                return;
            }
            if (distToEdge(this.me) < 30.0D)
            {
                this.goTo = middle - this.me;
                SDK.Drive((int)this.goTo.A, 100);
                this.lastTurnTime = SDK.Time;
                return;
            }
            double d3 = this.enemyShotTime - (int)this.enemyShotTime;
            double d4 = SDK.Time - (int)SDK.Time;
            double d5 = SDK.Abs(d3 - d4);
            if (((d5 < 0.05D) || (d5 > 0.95D)) && (SDK.Time - this.lastTurnTime > 0.5D))
            {
                int i = SDK.Rand(2);
                switch (i)
                {
                    case 0:
                        this.goTo = Vector.Rotate(this.goTo, 90);
                        break;
                    case 1:
                    default:
                        this.goTo = Vector.Rotate(this.goTo, -90);
                        break;
                }
                SDK.Drive((int)this.goTo.A, 100);
                this.lastTurnTime = SDK.Time;
                return;
            }
        }

        void lurk()
        {
            int i = timeIndex(SDK.Time) - 20;
            if (i < 0)
            {
                i = 0;
            }
            this.victimBack = this.enemyPos[i];
            double d1 = this.me.Dist(this.victimBack);
            double d2 = (this.victimBack - this.me).A;
            double d3 = (middle - this.victim).A;
            double d4 = (this.me - middle).A;
            int j = 0;
            int k = (int)SDK.Round(d2 + 90.0D);
            double d5 = normalize(normalize(d3) - normalize(d4));
            if (d5 < 180.0D)
            {
                k = (int)SDK.Round(d2 - 90.0D);
                j = 1;
            }
            int m = SDK.Abs(d5) > 20.0D ? 0 : 1;
            if (d1 > 790.0D)
            {
                SDK.Drive((int)SDK.Round(d2), 100);
            }
            else if (m != 0)
            {
                dart((int)SDK.Round(d2), d1);
            }
            else if (d1 > this.lurkDist)
            {
                SDK.Drive(k + (j != 0 ? 20 : -20), 50);
                this.hangFire = false;
            }
            else if (d1 < this.lurkDist)
            {
                SDK.Drive(k + (j != 0 ? -45 : 45), 50);
                this.hangFire = false;
            }
            if (distToEdge(this.me) < 20.0D)
            {
                SDK.Drive((int)SDK.Round(d2), 50);
            }
        }

        void dart(int paramInt, double paramDouble)
        {
            switch (this.dartMode)
            {
                case -1:
                    if (distToEdge(this.me) > 20.0D)
                    {
                        SDK.Drive(paramInt - 180, 50);
                    }
                    else
                    {
                        SDK.Drive(paramInt, 50);
                    }
                    if (paramDouble >= this.lurkDist)
                    {
                        this.dartMode = 0;
                    }
                    this.hangFire = false;
                    break;
                case 1:
                    SDK.Drive(paramInt, 50);
                    if ((SDK.Time - this.dartModeStartTime > 0.45D) || (paramDouble < 736.0D))
                    {
                        this.hangFire = false;
                        this.dartMode = -1;
                    }
                    break;
                case 0:
                default:
                    this.hangFire = true;
                    if (SDK.Time - this.lastShotTime > 2.0D)
                    {
                        this.hangFire = false;
                    }
                    if ((paramDouble > this.lurkDist) || (distToEdge(this.me) < 20.0D))
                    {
                        SDK.Drive(paramInt, 50);
                    }
                    else
                    {
                        SDK.Drive(paramInt + 180, 50);
                    }
                    if ((this.dartModeDamage > 30) && (distToEdge(this.victim) > 25.0D))
                    {
                        this.hangFire = false;
                        return;
                    }
                    if (SDK.Damage >= 90)
                    {
                        this.hangFire = false;
                        return;
                    }
                    double d1 = SDK.Time - this.lastHitTime;
                    double d2 = d1 - (int)d1;
                    if ((d2 > 0.003D) && (d2 < 0.01D) && (SDK.Time - this.lastShotTime > 0.55D))
                    {
                        this.dartMode = 1;
                        this.dartModeStartTime = SDK.Time;
                    }
                    break;
            }
        }

        double distToEdge(Vector paramJJVector)
        {
            double[] arrayOfDouble = new double[4];
            arrayOfDouble[0] = paramJJVector.X;
            arrayOfDouble[1] = paramJJVector.Y;
            arrayOfDouble[2] = (1000.0D - paramJJVector.X);
            arrayOfDouble[3] = (1000.0D - paramJJVector.Y);
            double d = 9999.0D;
            for (int i = 0; i < 4; i++)
            {
                if (arrayOfDouble[i] < d)
                {
                    d = arrayOfDouble[i];
                }
            }
            return d;
        }

        void qInsert(int paramInt1, int paramInt2, Vector paramJJVector)
        {
            for (int i = 9; i > paramInt2; i--)
            {
                this.evQ[paramInt1,i] = this.evQ[paramInt1,(i - 1)];
            }
            this.evQ[paramInt1,paramInt2] = paramJJVector;
        }

        void qPop(int paramInt)
        {
            for (int i = 0; i < 9; i++)
            {
                this.evQ[paramInt,i] = this.evQ[paramInt,(i + 1)];
            }
            this.evQ[paramInt,9] = new Vector(0.0D, 0.0D, 0.0D);
        }

        void qPut(int paramInt, Vector paramJJVector)
        {
            for (int i = 0; i < 10; i++)
            {
                if ((paramJJVector.T < this.evQ[paramInt,i].T) || (this.evQ[paramInt,i].T == 0.0D))
                {
                    qInsert(paramInt, i, paramJJVector);
                    break;
                }
            }
        }

        int r2dam(double paramDouble)
        {
            if (paramDouble <= 5.0D)
            {
                return 10;
            }
            if (paramDouble <= 20.0D)
            {
                return 5;
            }
            if (paramDouble <= 40.0D)
            {
                return 3;
            }
            return 0;
        }

        int timeIndex(double paramDouble)
        {
            return (int)SDK.Round(paramDouble / 0.1D);
        }

        int abs(int paramInt)
        {
            return paramInt < 0 ? -paramInt : paramInt;
        }

        double abs(double paramDouble)
        {
            return paramDouble < 0.0D ? -paramDouble : paramDouble;
        }

        bool isAlive(int paramInt)
        {
            if (SDK.Id == paramInt)
            {
                return true;
            }
            if (paramInt >= teamCount)
            {
                return false;
            }
            return SDK.Time - imAlive[paramInt] <= 0.1D;
        }

        int liveTeamCount()
        {
            int i = 0;
            for (int j = 0; j < teamCount; j++)
            {
                if (isAlive(j))
                {
                    i++;
                }
            }
            return i;
        }

        double normalize(double paramDouble)
        {
            while (paramDouble < 0.0D)
            {
                paramDouble += 360.0D;
            }
            while (paramDouble > 360.0D)
            {
                paramDouble -= 360.0D;
            }
            return paramDouble;
        }

        void edgeAdjust(Vector paramJJVector)
        {
            if ((paramJJVector.X > 0.0D) && (paramJJVector.Y > 0.0D) && (paramJJVector.X < 1000.0D) && (paramJJVector.Y < 1000.0D))
            {
                return;
            }
            if (paramJJVector.X < 0.0D)
            {
                paramJJVector.X = 0.0D;
            }
            if (paramJJVector.X > 1000.0D)
            {
                paramJJVector.X = 1000.0D;
            }
            if (paramJJVector.Y < 0.0D)
            {
                paramJJVector.Y =0.0D;
            }
            if (paramJJVector.Y > 1000.0D)
            {
                paramJJVector.Y =1000.0;
            }
        }

        Vector location()
        {
            return new Vector(SDK.LocX, SDK.LocY, SDK.Time);
        }

        double actual_speed()
        {
            return SDK.Speed;
        }
        
    }
}

/* Original code

class __Starkle_
  extends JJRobot
{
  static final double tick = 0.1D;
  static final JJVector middle = new JJVector(500.0D, 500.0D);
  static JJVector[] team = new JJVector[8];
  static JJVector[] target = new JJVector[8];
  static double[] imAlive = new double[8];
  static int teamCount;
  static int initCount = 0;
  int lastPr;
  double lastTurnTime;
  int myDamage = 0;
  double lastHitTime;
  double enemyShotTime;
  double lastShotTime;
  int shotCount = 0;
  JJVector me = new JJVector();
  JJVector victim = new JJVector();
  JJVector victim0 = new JJVector();
  JJVector victimBack = new JJVector();
  JJVector[] enemyPos = new JJVector[1850];
  JJVector prediction = new JJVector();
  JJVector goTo = new JJVector();
  static final int nStrat = 4;
  static final int qSz1 = 10;
  JJVector[][] evQ = new JJVector[4][10];
  static final int dEsz = 3;
  double[][] distSum = new double[4][3];
  int[][] damSum = new int[4][3];
  int damageEst = 0;
  boolean hangFire = false;
  int dartMode = 0;
  double dartModeStartTime;
  int dartModeDamage = 0;
  int lurkDist = 743;
  
  void main()
  {
    init();
    while (initCount != 0) {}
    this.lastTurnTime = 0.0D;
    findVictim();
    for (;;)
    {
      this.me.set(location());
      team[id()].set(this.me);
      imAlive[id()] = time();
      drive();
      evQdispatch();
      if (this.lastShotTime + 1.0D - time() > 0.7D) {
        this.victim0.set(this.victim);
      }
      this.prediction.set(predict(this.me, this.victim, getVel()));
      int i = JJRobot.i_rnd(this.prediction.minus(this.me).a());
      int j = JJRobot.i_rnd(this.prediction.minus(this.me).r());
      if ((!this.hangFire) && (j < 770) && (cannon(i, j) != 0))
      {
        this.lastShotTime = time();
        shot(this.prediction);
        findVictim();
      }
      if (!trackVictim()) {
        findVictim();
      }
      if (this.myDamage != damage())
      {
        int k = damage() - this.myDamage;
        this.myDamage = damage();
        this.lastHitTime = time();
        this.enemyShotTime = (this.lastHitTime - this.me.dist(this.victim) / 300.0D);
        if (this.dartMode != 0) {
          this.dartModeDamage += k;
        }
      }
    }
  }
  
  void init()
  {
    teamCount = getFriendsCount();
    if (initCount == 0) {
      for (i = 0; i < teamCount; i++)
      {
        team[i] = new JJVector();
        target[i] = new JJVector();
        imAlive[i] = 0.0D;
      }
    }
    this.me.set(location());
    this.goTo = middle.minus(this.me);
    drive(JJRobot.i_rnd(this.goTo.a()), 60);
    team[id()].set(this.me);
    for (int i = 0; i < 1849.0D; i++) {
      this.enemyPos[i] = new JJVector(0.0D, 0.0D, 0.0D);
    }
    int j;
    for (i = 0; i < 4; i++) {
      for (j = 0; j < 10; j++) {
        this.evQ[i][j] = new JJVector(0.0D, 0.0D, 0.0D);
      }
    }
    for (i = 0; i < 4; i++) {
      for (j = 0; j < 3; j++)
      {
        this.distSum[i][j] = 99999.0D;
        this.damSum[i][j] = 0;
      }
    }
    if (++initCount == teamCount) {
      initCount = 0;
    }
  }
  
  void findVictim()
  {
    int i = 0;
    int j = 0;
    JJVector localJJVector1 = new JJVector();
    JJVector localJJVector2 = new JJVector();
    int k = 99999;
    for (;;)
    {
      i = 0;
      k = 99999;
      for (;;)
      {
        i++;
        if (i > 360) {
          break;
        }
        if ((j = scan(JJRobot.i_rnd(i), 1)) != 0)
        {
          double d = i;
          if (scan(JJRobot.i_rnd(i + 1), 2) != 0) {
            d += 0.25D;
          } else if (scan(JJRobot.i_rnd(i - 1), 2) != 0) {
            d -= 0.25D;
          }
          localJJVector1.set(JJVector.Polar(j, d));
          localJJVector1.set_t(time());
          this.me.set(location());
          localJJVector2.set(localJJVector1.plus(this.me));
          localJJVector2.set_t(time());
          if ((isEnemy(localJJVector2)) && (j < k))
          {
            k = j;
            this.victim.set(localJJVector2);
            edgeAdjust(this.victim);
          }
        }
      }
      if (k != 99999) {
        break;
      }
    }
    target[id()].set(this.victim);
    if ((teamCount == 2) && (liveTeamCount() == 2))
    {
      int m = id() == 0 ? 1 : 0;
      if ((target[m].dist(this.victim) > 10.0D) && (this.me.dist(this.victim) > target[m].dist(team[m])) && (this.me.dist(target[m]) < 740.0D) && (this.me.dist(this.victim) / this.me.dist(target[m]) > 0.5D))
      {
        this.victim.set(target[m]);
        this.victim0.set(this.victim);
      }
    }
  }
  
  boolean isEnemy(JJVector paramJJVector)
  {
    for (int i = 0; i < teamCount; i++) {
      if (paramJJVector.dist(team[i]) < 10.0D) {
        return false;
      }
    }
    return true;
  }
  
  boolean trackVictim()
  {
    int i = JJRobot.i_rnd(this.victim.minus(this.me).a());
    int j = JJRobot.i_rnd(this.victim.minus(this.me).r());
    int k = 0;
    int m = 1;
    int n = 0;
    for (int i1 = 0; i1 < 3; i1++)
    {
      while (((n = scan(JJRobot.i_rnd(i), 1)) == 0) && (k < 10))
      {
        if (m > 0) {
          k++;
        }
        i += k * m;
        m = -m;
      }
      if ((n != 0) && (abs(n - j) < 10))
      {
        double d = i;
        if (scan(JJRobot.i_rnd(i + 1), 2) != 0) {
          d += 0.25D;
        } else if (scan(JJRobot.i_rnd(i - 1), 2) != 0) {
          d -= 0.25D;
        }
        this.me.set(location());
        this.victim.set(this.me.plus(JJVector.Polar(n, d)));
        this.victim.set_t(time());
        edgeAdjust(this.victim);
        historyPut(this.victim);
        target[id()].set(this.victim);
        return true;
      }
    }
    return false;
  }
  
  void historyPut(JJVector paramJJVector)
  {
    int i = timeIndex(paramJJVector.t());
    if (this.enemyPos[i].x() == 0.0D) {
      this.enemyPos[i].set(paramJJVector);
    }
  }
  
  JJVector vel(JJVector paramJJVector1, JJVector paramJJVector2)
  {
    JJVector localJJVector = new JJVector();
    double d1 = paramJJVector2.t() - paramJJVector1.t();
    double d2 = paramJJVector2.x() - paramJJVector1.x();
    double d3 = paramJJVector2.y() - paramJJVector1.y();
    if (d1 == 0.0D) {
      localJJVector.set(0.0D, 0.0D, paramJJVector2.t());
    } else {
      localJJVector.set(d2 / d1, d3 / d1, paramJJVector2.t());
    }
    return localJJVector;
  }
  
  JJVector getVel()
  {
    return vel(this.victim0, this.victim);
  }
  
  JJVector p1(JJVector paramJJVector1, JJVector paramJJVector2, JJVector paramJJVector3)
  {
    JJVector localJJVector = new JJVector();
    double d1 = paramJJVector2.x() - paramJJVector1.x();
    double d2 = paramJJVector2.y() - paramJJVector1.y();
    double d3 = 90000.0D * (d1 * d1 + d2 * d2);
    double d4 = (d1 * paramJJVector3.y() - d2 * paramJJVector3.x()) * (d1 * paramJJVector3.y() - d2 * paramJJVector3.x());
    double d5 = d1 * paramJJVector3.x() + d2 * paramJJVector3.y();
    double d6 = 90000.0D - (paramJJVector3.x() * paramJJVector3.x() + paramJJVector3.y() * paramJJVector3.y());
    double d7 = JJRobot.d_sqrt(d3 - d4 - d5) / d6;
    localJJVector.set(paramJJVector2.plus(paramJJVector3.mult(d7)));
    localJJVector.set_t(time() + d7);
    edgeAdjust(localJJVector);
    return localJJVector;
  }
  
  JJVector p2(JJVector paramJJVector1, JJVector paramJJVector2, JJVector paramJJVector3)
  {
    JJVector localJJVector = new JJVector();
    localJJVector.set(paramJJVector2);
    localJJVector.set_t(time() + paramJJVector1.dist(paramJJVector2) / 300.0D);
    edgeAdjust(localJJVector);
    return localJJVector;
  }
  
  JJVector p3(JJVector paramJJVector1, JJVector paramJJVector2, JJVector paramJJVector3)
  {
    JJVector localJJVector = new JJVector(JJVector.Polar(paramJJVector3.r() / 2.0D, paramJJVector3.a()));
    return p1(paramJJVector1, paramJJVector2, localJJVector);
  }
  
  void shot(JJVector paramJJVector)
  {
    qPut(0, pAdjust(paramJJVector));
    qPut(1, pAdjust(p1(this.me, this.victim, getVel())));
    qPut(2, pAdjust(p2(this.me, this.victim, getVel())));
    qPut(3, pAdjust(p3(this.me, this.victim, getVel())));
    this.shotCount += 1;
  }
  
  JJVector pAdjust(JJVector paramJJVector)
  {
    JJVector localJJVector = new JJVector();
    double d1 = paramJJVector.minus(this.me).a();
    double d2 = paramJJVector.minus(this.me).r();
    if (d2 > 700.0D) {
      d2 = 700.0D;
    }
    double d3 = d2 / 300.0D;
    localJJVector.set(this.me.plus(JJVector.Polar(d2, d1)));
    localJJVector.set_t(time() + d3);
    return localJJVector;
  }
  
  void evQdispatch()
  {
    for (int i = 0; i < 4; i++) {
      if ((time() > this.evQ[i][0].t()) && (this.evQ[i][0].t() != 0.0D)) {
        switch (i)
        {
        case 0: 
          shotLandedCB();
          break;
        case 1: 
          p1CB();
          break;
        case 2: 
          p2CB();
          break;
        case 3: 
          p3CB();
        }
      }
    }
  }
  
  void shotLandedCB()
  {
    trackVictim();
    double d = this.evQ[0][0].dist(this.victim);
    int i = r2dam(d);
    this.damageEst += i;
    for (int j = 0; j < 2; j++)
    {
      this.distSum[0][j] = this.distSum[0][(j + 1)];
      this.damSum[0][j] = this.damSum[0][(j + 1)];
    }
    this.distSum[0][2] = d;
    this.damSum[0][2] = i;
    qPop(0);
  }
  
  void p1CB()
  {
    double d = this.evQ[1][0].dist(this.victim);
    int i = r2dam(d);
    for (int j = 0; j < 2; j++)
    {
      this.distSum[1][j] = this.distSum[1][(j + 1)];
      this.damSum[1][j] = this.damSum[1][(j + 1)];
    }
    this.distSum[1][2] = d;
    this.damSum[1][2] = i;
    qPop(1);
  }
  
  void p2CB()
  {
    double d = this.evQ[2][0].dist(this.victim);
    int i = r2dam(d);
    for (int j = 0; j < 2; j++)
    {
      this.distSum[2][j] = this.distSum[2][(j + 1)];
      this.damSum[2][j] = this.damSum[2][(j + 1)];
    }
    this.distSum[2][2] = d;
    this.damSum[2][2] = i;
    qPop(2);
  }
  
  void p3CB()
  {
    double d = this.evQ[3][0].dist(this.victim);
    int i = r2dam(d);
    for (int j = 0; j < 2; j++)
    {
      this.distSum[3][j] = this.distSum[3][(j + 1)];
      this.damSum[3][j] = this.damSum[3][(j + 1)];
    }
    this.distSum[3][2] = d;
    this.damSum[3][2] = i;
    qPop(3);
  }
  
  int getBestPr()
  {
    double d1 = 1000000.0D;
    int i = 0;
    int j = 1;
    int k = 1;
    for (int m = 1; m < 4; m++)
    {
      double d2 = 0.0D;
      int n = 0;
      for (int i1 = 0; i1 < 3; i1++)
      {
        d2 += this.distSum[m][i1];
        n += this.damSum[m][i1];
      }
      if (d2 < d1)
      {
        d1 = d2;
        j = m;
      }
      if (n > i)
      {
        i = n;
        k = m;
      }
    }
    this.lastPr = (i == 0 ? j : k);
    return this.lastPr;
  }
  
  JJVector predict(JJVector paramJJVector1, JJVector paramJJVector2, JJVector paramJJVector3)
  {
    JJVector localJJVector = new JJVector();
    int i = getBestPr();
    switch (i)
    {
    case 1: 
      localJJVector.set(p1(paramJJVector1, paramJJVector2, paramJJVector3));
      break;
    case 2: 
      localJJVector.set(p2(paramJJVector1, paramJJVector2, paramJJVector3));
      break;
    case 3: 
      localJJVector.set(p3(paramJJVector1, paramJJVector2, paramJJVector3));
      break;
    default: 
      localJJVector.set(0.0D, 0.0D, 0.0D);
    }
    return localJJVector;
  }
  
  void drive()
  {
    if (speed() == 0)
    {
      this.goTo.set(middle.minus(this.me));
      drive(JJRobot.i_rnd(this.goTo.a()), 100);
      this.lastTurnTime = time();
      return;
    }
    if ((teamCount == 1) && (this.damageEst > damage() + 40))
    {
      drive(JJRobot.i_rnd(this.victim.minus(this.me).a()), 100);
      return;
    }
    if ((this.me.dist(this.victim) > 690.0D) && (distToEdge(this.me) > 30.0D))
    {
      lurk();
    }
    else
    {
      this.dartMode = 0;
      drunkWalk();
    }
  }
  
  void drunkWalk()
  {
    this.hangFire = false;
    double d1 = this.me.dist(this.victim);
    double d2 = 1.0D;
    if (d1 > 300.0D) {
      d2 = 2.0D;
    } else if (d1 > 600.0D) {
      d2 = 3.0D;
    }
    if (actual_speed() > 15.0D)
    {
      if ((time() - this.lastTurnTime < 0.45D * d2) && (distToEdge(this.me) > 30.0D)) {
        return;
      }
      drive(JJRobot.i_rnd(this.goTo.a()), 50);
      return;
    }
    if (distToEdge(this.me) < 30.0D)
    {
      this.goTo.set(middle.minus(this.me));
      drive(JJRobot.i_rnd(this.goTo.a()), 100);
      this.lastTurnTime = time();
      return;
    }
    double d3 = this.enemyShotTime - (int)this.enemyShotTime;
    double d4 = time() - (int)time();
    double d5 = abs(d3 - d4);
    if (((d5 < 0.05D) || (d5 > 0.95D)) && (time() - this.lastTurnTime > 0.5D))
    {
      int i = JJRobot.rand(2);
      switch (i)
      {
      case 0: 
        this.goTo.set(this.goTo.rotate(90.0D));
        break;
      case 1: 
      default: 
        this.goTo.set(this.goTo.rotate(-90.0D));
      }
      drive(JJRobot.i_rnd(this.goTo.a()), 100);
      this.lastTurnTime = time();
      return;
    }
  }
  
  void lurk()
  {
    int i = timeIndex(time()) - JJRobot.i_rnd(20.0D);
    if (i < 0) {
      i = 0;
    }
    this.victimBack = this.enemyPos[i];
    double d1 = this.me.dist(this.victimBack);
    double d2 = this.victimBack.minus(this.me).a();
    double d3 = middle.minus(this.victim).a();
    double d4 = this.me.minus(middle).a();
    int j = 0;
    int k = JJRobot.i_rnd(d2 + 90.0D);
    double d5 = normalize(normalize(d3) - normalize(d4));
    if (d5 < 180.0D)
    {
      k = JJRobot.i_rnd(d2 - 90.0D);
      j = 1;
    }
    int m = abs(d5) > 20.0D ? 0 : 1;
    if (d1 > 790.0D)
    {
      drive(JJRobot.i_rnd(d2), 100);
    }
    else if (m != 0)
    {
      dart(JJRobot.i_rnd(d2), d1);
    }
    else if (d1 > this.lurkDist)
    {
      drive(k + (j != 0 ? 20 : -20), 50);
      this.hangFire = false;
    }
    else if (d1 < this.lurkDist)
    {
      drive(k + (j != 0 ? -45 : 45), 50);
      this.hangFire = false;
    }
    if (distToEdge(this.me) < 20.0D) {
      drive(JJRobot.i_rnd(d2), 50);
    }
  }
  
  void dart(int paramInt, double paramDouble)
  {
    switch (this.dartMode)
    {
    case -1: 
      if (distToEdge(this.me) > 20.0D) {
        drive(paramInt - 180, 50);
      } else {
        drive(paramInt, 50);
      }
      if (paramDouble >= this.lurkDist) {
        this.dartMode = 0;
      }
      this.hangFire = false;
      break;
    case 1: 
      drive(paramInt, 50);
      if ((time() - this.dartModeStartTime > 0.45D) || (paramDouble < 736.0D))
      {
        this.hangFire = false;
        this.dartMode = -1;
      }
      break;
    case 0: 
    default: 
      this.hangFire = true;
      if (time() - this.lastShotTime > 2.0D) {
        this.hangFire = false;
      }
      if ((paramDouble > this.lurkDist) || (distToEdge(this.me) < 20.0D)) {
        drive(paramInt, 50);
      } else {
        drive(paramInt + 180, 50);
      }
      if ((this.dartModeDamage > 30) && (distToEdge(this.victim) > 25.0D))
      {
        this.hangFire = false;
        return;
      }
      if (damage() >= 90)
      {
        this.hangFire = false;
        return;
      }
      double d1 = time() - this.lastHitTime;
      double d2 = d1 - (int)d1;
      if ((d2 > 0.003D) && (d2 < 0.01D) && (time() - this.lastShotTime > 0.55D))
      {
        this.dartMode = 1;
        this.dartModeStartTime = time();
      }
      break;
    }
  }
  
  double distToEdge(JJVector paramJJVector)
  {
    double[] arrayOfDouble = new double[4];
    arrayOfDouble[0] = paramJJVector.x();
    arrayOfDouble[1] = paramJJVector.y();
    arrayOfDouble[2] = (1000.0D - paramJJVector.x());
    arrayOfDouble[3] = (1000.0D - paramJJVector.y());
    double d = 9999.0D;
    for (int i = 0; i < 4; i++) {
      if (arrayOfDouble[i] < d) {
        d = arrayOfDouble[i];
      }
    }
    return d;
  }
  
  void qInsert(int paramInt1, int paramInt2, JJVector paramJJVector)
  {
    for (int i = 9; i > paramInt2; i--) {
      this.evQ[paramInt1][i].set(this.evQ[paramInt1][(i - 1)]);
    }
    this.evQ[paramInt1][paramInt2].set(paramJJVector);
  }
  
  void qPop(int paramInt)
  {
    for (int i = 0; i < 9; i++) {
      this.evQ[paramInt][i].set(this.evQ[paramInt][(i + 1)]);
    }
    this.evQ[paramInt][9].set(0.0D, 0.0D, 0.0D);
  }
  
  void qPut(int paramInt, JJVector paramJJVector)
  {
    for (int i = 0; i < 10; i++) {
      if ((paramJJVector.t() < this.evQ[paramInt][i].t()) || (this.evQ[paramInt][i].t() == 0.0D))
      {
        qInsert(paramInt, i, paramJJVector);
        break;
      }
    }
  }
  
  int r2dam(double paramDouble)
  {
    if (paramDouble <= 5.0D) {
      return 10;
    }
    if (paramDouble <= 20.0D) {
      return 5;
    }
    if (paramDouble <= 40.0D) {
      return 3;
    }
    return 0;
  }
  
  int timeIndex(double paramDouble)
  {
    return JJRobot.i_rnd(paramDouble / 0.1D);
  }
  
  int abs(int paramInt)
  {
    return paramInt < 0 ? -paramInt : paramInt;
  }
  
  double abs(double paramDouble)
  {
    return paramDouble < 0.0D ? -paramDouble : paramDouble;
  }
  
  boolean isAlive(int paramInt)
  {
    if (id() == paramInt) {
      return true;
    }
    if (paramInt >= teamCount) {
      return false;
    }
    return time() - imAlive[paramInt] <= 0.1D;
  }
  
  int liveTeamCount()
  {
    int i = 0;
    for (int j = 0; j < teamCount; j++) {
      if (isAlive(j)) {
        i++;
      }
    }
    return i;
  }
  
  double normalize(double paramDouble)
  {
    while (paramDouble < 0.0D) {
      paramDouble += 360.0D;
    }
    while (paramDouble > 360.0D) {
      paramDouble -= 360.0D;
    }
    return paramDouble;
  }
  
  void edgeAdjust(JJVector paramJJVector)
  {
    if ((paramJJVector.x() > 0.0D) && (paramJJVector.y() > 0.0D) && (paramJJVector.x() < 1000.0D) && (paramJJVector.y() < 1000.0D)) {
      return;
    }
    if (paramJJVector.x() < 0.0D) {
      paramJJVector.set_x(0.0D);
    }
    if (paramJJVector.x() > 1000.0D) {
      paramJJVector.set_x(1000.0D);
    }
    if (paramJJVector.y() < 0.0D) {
      paramJJVector.set_y(0.0D);
    }
    if (paramJJVector.y() > 1000.0D) {
      paramJJVector.set_y(1000.0D);
    }
  }
}
*/