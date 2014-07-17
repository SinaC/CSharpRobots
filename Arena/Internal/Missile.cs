using Common;

namespace Arena.Internal
{
    internal class Missile : IReadonlyMissile
    {
        private readonly Tick _launchTick;

        private readonly Tick _matchStart;

        // When a missile has exploded, it stays in state Explosed during x milliseconds
        private Tick _explosionTick;
        // Current distance
        public double CurrentDistance { get; private set; }

        public double LaunchLocX { get; private set; }
        public double LaunchLocY { get; private set; }

        // Current location
        public double LocX { get; private set; }
        public double LocY { get; private set; }

        #region IReadonlyMissile

        // Id
        public int Id { get; private set; }

        // State
        public MissileStates State { get; private set; }

        // Robot shooting the missile
        public IReadonlyRobot Robot { get; private set; }

        // Launch location, heading, range
        int IReadonlyMissile.LaunchLocX { get { return (int)LaunchLocX; } }
        int IReadonlyMissile.LaunchLocY { get { return (int)LaunchLocY; } }

        public int Heading { get; private set; }
        public int Range { get; private set; }

        // Current location
        int IReadonlyMissile.LocX { get { return (int) LocX; } }
        int IReadonlyMissile.LocY { get { return (int)LocY; } }

        #endregion

        internal Missile(IReadonlyRobot robot, Tick matchStart, int id, double locX, double locY, int heading, int range)
        {
            _launchTick = Tick.Now;

            Robot = robot;
            _matchStart = matchStart;

            Id = id;

            LaunchLocX = locX;
            LaunchLocY = locY;
            Heading = heading;
            Range = range;

            LocX = locX;
            LocY = locY;
            CurrentDistance = 0;

            State = MissileStates.Flying;
        }

        public void UpdatePosition(double realStepTime)
        {
            // Update distance
            CurrentDistance += (ParametersSingleton.MissileSpeed * realStepTime) / 1000.0;
            if (CurrentDistance > Range) // if missile goes too far, get it back :)
                CurrentDistance = Range;
            // Update location
            double newLocX, newLocY;
            Math.ComputePoint(LaunchLocX, LaunchLocY, CurrentDistance, Heading, out newLocX, out newLocY);
            LocX = newLocX;
            LocY = newLocY;

            //Log.WriteLine(Log.LogLevels.Debug, "Missile {0} location updated. CurrentDistance {1} LaunchX {2} LaunchY {3} LocX {4} LocY {5} Heading {6}", Id, CurrentDistance, LaunchLocX, LaunchLocY, LocX, LocY, Heading);
        }

        public void TargetReached()
        {
            //// Check speed
            //double elapsed = Tick.ElapsedMilliseconds(_launchTick);
            //double diffX = LocX - LaunchLocX;
            //double diffY = LocY - LaunchLocY;
            //double distance = System.Math.Sqrt(diffX*diffX + diffY*diffY);
            //double speed = distance/elapsed*1000.0; // in m/s
            //double tick = Tick.ElapsedSeconds(_matchStart);
            //Log.WriteLine(Log.LogLevels.Debug, "Missile {0} : {1:0.00}  | target reached. Speed {2:0.000} Distance {3:0.000} Range {4}  loc:{5:0.000},{6:0.000}", Id, tick, speed, distance, Range, LocX, LocY);

            State = MissileStates.Exploding;
        }

        public void CollisionWall(double newLocX, double newLocY)
        {
            State = MissileStates.Exploding;
            LocX = newLocX;
            LocY = newLocY;
        }

        public void UpdateExploding()
        {
            _explosionTick = Tick.Now;
            State = MissileStates.Exploded;
        }

        public void UpdateExploded(int explosionDisplayDelay)
        {
            if (Tick.ElapsedMilliseconds(_explosionTick) > explosionDisplayDelay)
                State = MissileStates.Deleted;
        }
    }
}


// missile damage must be computed before moving robots

////-----------------------------------------------------------------
// // calculate robots damages due to the exploding missiles
// //-----------------------------------------------------------------

// double misds = dt*MS_SP;        // missile can move so match during full time step dt

// for(int ct = 0; ct < jr.length; ct++) {     // for all robots
//    for(int mis = 0; mis < CN_MS; mis++) {   // check their missiles
//      if((range = msr[ct][mis]) > 0) {       // if missile is alive
//        if(range > misds) {                  // should it still fly?
//          msr[ct][mis] -= misds;
//          msx[ct][mis] += misds*cmsd[ct][mis];// missile new position
//          msy[ct][mis] += misds*smsd[ct][mis];
//        } else {                             // explode the missile
//                  dte = range * MS_SP_I;             // expolding time
//          msr[ct][mis] = -2;                 // missile inactive
//          msx[ct][mis] += range*cmsd[ct][mis]; // x coordinate
//          msy[ct][mis] += range*smsd[ct][mis]; // y coordinate

//                    // exploding missile may affect all robots
//          for(int ct1 = 0; ct1 < jr.length; ct1++) {
//            if(std[ct1] < 100) { // if robot is alive it can be damaged by missile
//             double dx = lcx[ct1]-msx[ct][mis]; // last known robot position used
//             double dy = lcy[ct1]-msy[ct][mis];
//             double dist = dx*dx + dy*dy;
//                         double ec = 40.0 + lcd[ct1]*dte + JR_AC_H*dte*dte;  // max robot escape range
//                         int d=0;                   // damage taken by robot
//                         if(dist < ec*ec)           // robot close enough for damage
//                            {d = damageByMissile(dte,ct1,ct,mis);}
//                         if(d>0){                   // robot damaged
//              std[ct1] +=d;
//                          boolean killed = false;
//              if (std[ct1] >= 100){
//                            killed = true;
//              fd = true;
//                }
//                if(draw){ // do not keep count drawing not required
//               int firingTeam  = ct  / mode;
//               int damagedTeam = ct1 / mode;
//               if (firingTeam != damagedTeam){
//                damageInflicted[firingTeam] += d;
//                if (killed)
//                   kills[firingTeam] += 1;
//               }      //not self inflicted damage
//                }      //draw information needed
//             }      //damage happend
//              }      //if robots is alive
//          }       //for all efected robots
//        }        //for exploding missile
//      }         //for all active missiles
//    }          //for all robots missiles
//  }           //for all robots
 // ----------------------------------------------------------------

//final private int damageByMissile(double dte, int id, int ct, int mis)
//{
//    // double dte  - exact time of the missile explosion
//    // int id      - index of the robot exposed to the explosion
//    // int ct      - missile belongs to robot of this id
//    // int mis     - index of the missile
//  double rd;                           // robot distance change;
//  double vnext;                        // robot's speed after this step
//  double t1;                           // over/understepped time
//  double t2;                           // remaining time to finish this step (t1+t2 = dt)
//    double lx;                           // robot position on the dte time
//    double ly;                           //
//  double delta = dte*JR_AC;            // robots speed increase due to acceleration    v = a*t
//  double ra    = 0.5*delta*dte;        // robots distance increase due to acceletation d = 0.5*a*t*t
//  double rs    = dte*lcs[id];          // distance increases due to robot speed d = v *t
//  double sdif  = sts[id]-lcs[id];      // robot speed difference between current and required speed

//  if(sdif==0)                          // if robot reached required speed
//  {
//  lx = lcx[id] + rs*clcd[id];          // increase along x axis
//  ly = lcy[id] + rs*slcd[id];          // increase along y axis
//  }
//  else                                 // robot accelerates or deaccelerats
//  {
//    if( sdif > 0)                        // if robot accelerates
//    {                                    // check if we overstepped required speed sts[id]
//     vnext = lcs[id] + delta;            // next speed
//     if(vnext > sts[id]){                // yes we overstepped
//       t1 = sdif*JR_AC_I;                // t1 time when we would overstep
//       t2 = dte - t1;                    // remaining time to finish the dte step
//       rd = lcs[id]*t1 + JR_AC_H*t1*t1 + sts[id]*t2;
//       lx = lcx[id] + rd*clcd[id];       // relocation along x axis
//       ly = lcy[id] + rd*slcd[id];       // relocation along y axis
//     }
//     else{                               // no we would not overstepped
//       lx = lcx[id] + (rs+ra)*clcd[id];  // relocation along x axis
//       ly = lcy[id] + (rs+ra)*slcd[id];  // relocation along y axis
//     }
//    }
//    else                                 // robot deaccelerates
//    {
//     vnext = lcs[id] - delta;            // next speed
//     if(vnext < sts[id]){                // yes we overstepped
//      t1 = -sdif*JR_AC_I;                // sign change is faster than abs fun
//      t2 = dte - t1;                     // remaining time to finish the explosion step
//      rd = lcs[id]*t1 - JR_AC_H*t1*t1 + sts[id]*t2;
//      lx = lcx[id] + rd*clcd[id];        // relocation along x axis
//      ly = lcy[id] + rd*slcd[id];        // relocation along y axis
//     }
//     else{                               // no we would not understepped
//     lx = lcx[id] + (rs-ra)*clcd[id];    // relocation along x axis
//     ly = lcy[id] + (rs-ra)*slcd[id];    // relocation along y axis
//     }
//    }
// }

// // check if robot hit the wall
//  if(lx < 0) {
//        if(clcd[id]==0)  lx = 0;
//        else{
//    ly -= lx*slcd[id]/clcd[id];  // tan(a) = sin(a)/cos(a)
//    lx = 0;
//        }
//  } else if(lx > BF_SZ) {
//        if(clcd[id]==0)  lx = BF_SZ;
//        else{
//    ly += (BF_SZ-lx)*slcd[id]/clcd[id];
//    lx = BF_SZ;
//        }
//  }
//  if(ly < 0) {
//        if(slcd[id]==0)  ly = 0;
//        else{
//     lx -= ly*clcd[id]/slcd[id];
//     ly = 0;
//        }
//  } else if(ly > BF_SZ) {
//         if(slcd[id]==0) ly = BF_SZ;
//         else{
//      lx += (BF_SZ-ly)*clcd[id]/slcd[id];
//      ly = BF_SZ;
//         }
//  }
//    //----------------------------------------------------------------

// double dx = lx-msx[ct][mis]; //real robot position used
// double dy = ly-msy[ct][mis]; //
// double dist = dx*dx + dy*dy;

// // calculate the damage:
// int d = dist<1600.0? (dist<400.0? (dist<25.0? 10: 5): 3): 0;
// return d;
//}