namespace Robots
{
    public class Surveyor : SDK.Robot
    {
        public override void Main()
        {
            MeasureSpeedAndAcceleration();
        }

        private void MeasureSpeedAndAcceleration()
        {
            double initialTime = SDK.Time;
            int initX = SDK.LocX;
            int initY = SDK.LocY;

            double lastTime = initialTime;
            double lastX = initX;
            double lastY = initY;
            int lastDamage = 0;

            int previousSpeed = 0;

            SDK.Drive(0, 100);

            while(true)
            {
                double currentTime = SDK.Time;
                int currentX = SDK.LocX;
                int currentY = SDK.LocY;
                int currentSpeed = SDK.Speed;
                int currentDamage = SDK.Damage;

                //System.Diagnostics.Debug.WriteLine("Speed X:{0:0.0000} Y:{1:0.0000} - Time {2:0.0000}", speedX, speedY, diffTime);

                double diffLastTime = currentTime - lastTime;
                if (diffLastTime > 1 || lastDamage < currentDamage)
                {
                    double diffTime = currentTime - initialTime;

                    double diffX = currentX - initX;
                    double diffY = currentY - initY;
                    double speedX = diffX / diffTime;
                    double speedY = diffY / diffTime;
                    
                    double actualSpeedX = (currentX - lastX) / diffLastTime;
                    double actualSpeedY = (currentY - lastY) / diffLastTime;

                    int diffSpeed = currentSpeed - previousSpeed;
                    double acceleration = diffSpeed / diffLastTime;

                    System.Diagnostics.Debug.WriteLine("TICK:{0:0.00} | Loc:{1},{2} | Instant speed X:{3:0.00} Y:{4:0.00} - Elapsed {5:0.00}  diff X:{6:0.00} Y:{7:0.00}  Dmg:{8} acceleration:{9:0.00}", SDK.Time, currentX, currentY, actualSpeedX, actualSpeedY, diffTime, diffX, diffY, SDK.Damage, acceleration);

                    lastTime = currentTime;
                    lastX = currentX;
                    lastY = currentY;
                    previousSpeed = currentSpeed;
                    lastDamage = currentDamage;
                }
            }
        }
    }
}

/* STINGER
//
// JJRobots (c) 2000 L.Boselli - boselli@uno.it
//
public class __Stinger_ extends JJRobot {

private static int counter;

private static int locX[] = new int[8];
private static int locY[] = new int[8];

private static int driveAngle = 5;

private double oldTargetX;
private double oldTargetY;
private double targetX;
private double targetY;
private double speedX;
private double speedY;
private double lastTime;
private int range;
private int scan;
private int drive;
private int id;

void main() {
  if((id = id()) == 0) {
    counter = 1;
  } else {
    counter = id+1;
  }
  targetX = targetY = -1000;
  speedX = speedY = 0;
  lastTime = 0;
  drive(drive=rand(360),100);
  while(true) {
    do {
      if(findNearestEnemy(0)) shoot();
    } while(scan-driveAngle < drive && drive < scan+driveAngle);
    stopAndGo();
  }
}

private boolean findNearestEnemy(int minDistance) {
  int startAngle = 0;
  int endAngle = 360;
  int nearestAngle = 0;
  int nearestDistance = 0;
  for(int resAngle = 16; resAngle >= 1; resAngle /= 2) {
    nearestDistance = 2000;
    for(
      scan = startAngle;
      scan <= endAngle;
      scan += resAngle
    ) {
      range = scan(scan,resAngle);
      if(range > minDistance+40 && range < nearestDistance) {
        nearestDistance = range;
        nearestAngle = scan;
      }
    }
    startAngle = nearestAngle-resAngle;
    endAngle = startAngle+2*resAngle;
  }
  range = nearestDistance;
  scan = nearestAngle;
  if(range > 0) {
    double time;
    double deltaT = (time = time()) - lastTime;
    targetX = (locX[id]=loc_x())+range*cos(scan)/100000.0;
    targetY = (locY[id]=loc_y())+range*sin(scan)/100000.0;
    if(isTargetAFriend()) return findNearestEnemy(range);
    if(deltaT > 0.5) {
      double theSpeedX = (targetX-oldTargetX)/deltaT;
      double theSpeedY = (targetY-oldTargetY)/deltaT;
      oldTargetX = targetX;
      oldTargetY = targetY;
      double speed2 = theSpeedX*theSpeedX + theSpeedY*theSpeedY;
      if(speed2 > 0) {
        if(speed2 < 1600) {
          speedX = theSpeedX;
          speedY = theSpeedY;
        } else {
          speedX = speedY = 0;
        }
      }
      lastTime = time;
    }
    return true;
  }
  return false;
}

private boolean isTargetAFriend() {
  if(counter > 1) {
    for(int ct = 0; ct < counter; ct++) {
      if(ct != id) {
        int dx = (int)(targetX-locX[ct]);
        int dy = (int)(targetY-locY[ct]);
        if(dx*dx+dy*dy < 6400) return true;
      }
    }
  }
  return false;
}

private void stopAndGo() {
  drive(drive=scan,49);
  while(speed() >= 50) {
    findNearestEnemy(0);
    shoot();
  }
  int dx = (int)(targetX-(locX[id] = loc_x()));
  int dy = (int)(targetY-(locY[id] = loc_y()));
  if(dx == 0) {
    drive = dy > 0? 90: 270;
  } else {
    drive = atan(dy*100000/dx);
    if(dx < 0) drive += 180;
  }
  drive(drive,100);
}

private void shoot() {
  if(range > 50 && range <= 800) {
    fireToTarget(targetX,targetY,speedX,speedY,lastTime);
  } else if(range > 0 && range <= 50) {
    cannon(scan,45);
  }
}

private void fireToTarget(double x, double y, double sx, double sy, double t) {
  double Dx, Dy;
  double deltaT = time()-t;
  if(deltaT > 0) {
    x += sx*deltaT;
    y += sy*deltaT;
    Dx = x-(locX[id]=loc_x());
    Dy = y-(locY[id]=loc_y());
  } else {
    Dx = x-(locX[id]=loc_x());
    Dy = y-(locY[id]=loc_y());
  }
  double dxsymdysx = Dx*sy-Dy*sx;
  double tp =
    (d_sqrt((Dx*Dx+Dy*Dy)*90000-dxsymdysx*dxsymdysx)+Dx*sx+Dy*sy)/
    (90000-sx*sx-sy*sy)
  ;
  double rx = Dx+sx*tp;
  double ry = Dy+sy*tp;
  double r2 = rx*rx+ry*ry;
  if(r2 > 1600 && r2 < 547600) {
    double angle;
    if(rx == 0) {
      angle = ry > 0? 1.5708: 4.7124;
    } else {
      angle = d_atan(ry/rx);
      if(rx < 0) angle += 3.1416;
    }
    int degrees = (int)(angle*180/3.1416);
    cannon(degrees,(int)(d_sqrt(r2)+0.5));
  }
}

}
*/

/* Phalanx
//
// JJRobots (c) 2000 L.Boselli - boselli@uno.it
//
public class __Phalanx_ extends JJRobot {

private static int counter;
private static int firstCorner;

private static int[] cornerX = {50,950,950,50};
private static int[] cornerY = {50,50,950,950};
private static double lastTargetX;
private static double lastTargetY;
private static double lastTargetSpeedX;
private static double lastTargetSpeedY;
private static double lastTargetTime;
private static int locX[] = new int[8];
private static int locY[] = new int[8];

private double oldTargetX;
private double oldTargetY;
private double targetX;
private double targetY;
private double speedX;
private double speedY;
private double lastTime;
private double lastShotTime;
private boolean foundEnemy;
private int resolution;
private int corner;
private int driveStatus;
private int range;
private int scan;
private int drive;
private int id;

void main() {
  if((id = id()) == 0) {
    counter = 1;
    firstCorner = rand(4);
    lastTargetX = lastTargetY = -1000;
    lastTargetSpeedX = lastTargetSpeedY = 0;
    lastTargetTime = 0;
  } else {
    counter = id+1;
  }
  targetX = targetY = -1000;
  speedX = speedY = 0;
  lastTime = 0;
  resolution = 8;
  corner = firstCorner;
  stopAndGo();
  while(farFromCorner()) {
    findAndShoot();
    changeDrive();
  }
  while(true) {
    if(time() - lastShotTime < 30) {
      if(++corner == 4) corner = 0;
    } else {
      if((corner += 2) >= 4) corner -= 4;
    }
    stopAndGo();
    while(farFromSide()) {
      findAndShoot();
      changeDrive();
    }
  }
}

private void stopAndGo() {
  drive(drive,0);
  while(speed() >= 50) findAndShoot();
  int dx = cornerX[corner]-(locX[id] = loc_x());
  int dy = cornerY[corner]-(locY[id] = loc_y());
  if(dx == 0) {
    drive = dy > 0? 90: 270;
  } else {
    drive = atan(dy*100000/dx);
    if(dx < 0) drive += 180;
  }
  drive(drive,100);
  driveStatus = 0;
}

private void changeDrive() {
  int speed = speed();
  switch(driveStatus) {
    default:
    case 0: {
      if(speed >= 100) {
        driveStatus++;
        drive(drive,48);
      }
      break;
    }
    case 1: {
      if(speed <= 49) {
        driveStatus++;
        drive(drive+10,100);
      }
      break;
    }
    case 2: {
      if(speed >= 100) {
        driveStatus++;
        drive(drive,48);
      }
      break;
    }
    case 3: {
      if(speed <= 49) {
        driveStatus = 0;
        drive(drive-10,100);
      }
      break;
    }
  }
}

private boolean findEnemy() {
  if((range = scan(scan,resolution)) == 0) return false;
  double time;
  double deltaT = (time = time()) - lastTime;
  targetX = (locX[id]=loc_x())+range*cos(scan)/100000.0;
  targetY = (locY[id]=loc_y())+range*sin(scan)/100000.0;
  if(isTargetAFriend()) {
    scan += resolution;
    return false;
  }
  if(resolution == 1 && deltaT > 0.5) {
    double theSpeedX = (targetX-oldTargetX)/deltaT;
    double theSpeedY = (targetY-oldTargetY)/deltaT;
    lastTargetX = oldTargetX = targetX;
    lastTargetY = oldTargetY = targetY;
    double speed2 = theSpeedX*theSpeedX + theSpeedY*theSpeedY;
    if(speed2 > 0) {
      if(speed2 < 1600) {
        lastTargetSpeedX = speedX = theSpeedX;
        lastTargetSpeedY = speedY = theSpeedY;
      } else {
        lastTargetSpeedX = lastTargetSpeedY = speedX = speedY = 0;
      }
    }
    lastTargetTime = lastTime = time;
  }
  return true;
}

private void findAndShoot() {
  if(findEnemy()) {
    if(resolution > 1) resolution /= 2;
    foundEnemy = true;
  } else {
    if(foundEnemy) {
      if((scan -= resolution) < 0) scan += 360;
      foundEnemy = false;
    } else {
      if((scan += resolution) > 360) scan -= 360;
      if(resolution < 8) resolution *= 2;
    }
  }
  if(range > 40 && range <= 740) {
    if(!isTargetAFriend()) fireToTarget();
  } else {
    if(range > 740) {
      scan += resolution*2;
    } else {
      if(!isLastTargetAFriend()) fireToLastTarget();
    }
  }
}

private void fireToTarget() {
  fireTo(targetX,targetY,speedX,speedY,lastTime);
}

private void fireToLastTarget() {
  fireTo(
    lastTargetX,lastTargetY,lastTargetSpeedX,lastTargetSpeedY,lastTargetTime
  );
}

private void fireTo(double x, double y, double sx, double sy, double t) {
  double deltaT = time()-t;
  double Dx, Dy;
  if(deltaT > 0) {
    x += sx*deltaT;
    y += sy*deltaT;
    Dx = x-(locX[id]=loc_x());
    Dy = y-(locY[id]=loc_y());
  } else {
    Dx = x-locX[id];
    Dy = y-locY[id];
  }
  double dxsymdysx = Dx*sy-Dy*sx;
  double tp =
    (d_sqrt((Dx*Dx+Dy*Dy)*90000-dxsymdysx*dxsymdysx)+Dx*sx+Dy*sy)/
    (90000-sx*sx-sy*sy)
  ;
  double rx = Dx+sx*tp;
  double ry = Dy+sy*tp;
  double r2 = rx*rx+ry*ry;
  if(r2 > 1600 && r2 < 547600) {
    double angle;
    if(rx == 0) {
      angle = ry > 0? 1.5708: 4.7124;
    } else {
      angle = d_atan(ry/rx);
      if(rx < 0) angle += 3.1416;
    }
    int degrees = (int)(angle*180/3.1416);
    if(cannon(degrees,(int)(d_sqrt(r2)+0.5)) != 0) lastShotTime = time();
  }
}

private boolean isTargetAFriend() {
  if(counter > 1) {
    for(int ct = 0; ct < counter; ct++) {
      if(ct != id) {
        int dx = (int)(targetX-locX[ct]);
        int dy = (int)(targetY-locY[ct]);
        if(dx*dx+dy*dy < 6400) return true;
      }
    }
  }
  return false;
}

private boolean isLastTargetAFriend() {
  if(counter > 1) {
    for(int ct = 0; ct < counter; ct++) {
      if(ct != id) {
        int dx = (int)(lastTargetX-locX[ct]);
        int dy = (int)(lastTargetY-locY[ct]);
        if(dx*dx+dy*dy < 6400) return true;
      }
    }
  }
  return false;
}

private boolean farFromCorner() {
  switch(corner) {
    default:
    case 0: return locX[id] > 150 || locY[id] > 150;
    case 1: return locX[id] < 850 || locY[id] > 150;
    case 2: return locX[id] < 850 || locY[id] < 850;
    case 3: return locX[id] > 150 || locY[id] < 850;
  }
}

private boolean farFromSide() {
  switch(corner) {
    default:
    case 0: return locY[id] > 150;
    case 1: return locX[id] < 850;
    case 2: return locY[id] < 850;
    case 3: return locX[id] > 150;
  }
}

}
*/

/* Platoon
//
// JJRobots (c) 2000 L.Boselli - boselli@uno.it
//
public class __Platoon_ extends JJRobot {

private static int count;
private static int[] cornerX = {50,950,950,50};
private static int[] cornerY = {50,50,950,950};
private static int targetX = 500;
private static int targetY = 500;
private static int locX[] = new int[8];
private static int locY[] = new int[8];
private static int corner1;

private int nCorner;
private int scan;
private int id;

void main() {
  if((id = id()) == 0) {
    count = 1;
    corner1 = rand(4);
  } else {
    count = id+1;
  }
  nCorner = corner1;
  int dx = cornerX[nCorner]-(locX[id]=loc_x());
  int dy = cornerY[nCorner]-(locY[id]=loc_y());
  int angle;
  if(dx == 0) {
    angle = dy > 0? 90: 270;
  } else {
    angle = atan(dy*100000/dx);
  }
  if(dx < 0) angle += 180;
  drive(angle,100);
  switch(nCorner) {
    default:
    case 0: while(locX[id] > 150 || locY[id] > 150) fire2(); break;
    case 1: while(locX[id] < 850 || locY[id] > 150) fire2(); break;
    case 2: while(locX[id] < 850 || locY[id] < 850) fire2(); break;
    case 3: while(locX[id] > 150 || locY[id] < 850) fire2(); break;
  }
  do {
    drive(0,0);
    while(speed() >= 50) fire1();
    if(++nCorner == 4) nCorner = 0;
    dx = cornerX[nCorner]-loc_x();
    dy = cornerY[nCorner]-loc_y();
    if(dx == 0) {
      angle = dy > 0? 90: 270;
    } else {
      angle = atan(dy*100000/dx);
    }
    if(dx < 0) angle += 180;
    drive(angle,100);
    switch(nCorner) {
      default:
      case 0: while(locY[id] > 150) fire1(); break;
      case 1: while(locX[id] < 850) fire1(); break;
      case 2: while(locY[id] < 850) fire1(); break;
      case 3: while(locX[id] > 150) fire1(); break;
    }
  } while(true);
}

private void fire1() {
  switch(nCorner) {
    default:
    case 0: if(++scan > 470 || scan < 240) scan = 250; break;
    case 1: if(++scan > 200 || scan < -30) scan = -20; break;
    case 2: if(++scan > 290 || scan <  60) scan = 70; break;
    case 3: if(++scan > 380 || scan < 150) scan = 160; break;
  }
  fire();
}

private void fire2() {
  if(++scan > 360) scan = 0;
  fire();
}

private void fire() {
  locX[id] = loc_x();
  locY[id] = loc_y();
  int range;
  if((range = scan(scan,1)) > 40 && range <= 740) {
    if (count > 1) {
      boolean shot = true;
      int shotX = locX[id]+range*cos(scan)/100000;
      int shotY = locY[id]+range*sin(scan)/100000;
      for(int ct = 0; ct < count; ct++) {
        if(ct != id) {
          int dx = shotX-locX[ct];
          int dy = shotY-locY[ct];
          if(dx*dx+dy*dy < 1600) {
            shot = false;
            break;
          }
        }
      }
      if(shot) {
        targetX = shotX;
        targetY = shotY;
        cannon(scan,range);
        scan -= 10;
      } else {
        int dx = targetX-locX[id];
        int dy = targetY-locY[id];
        int dist2 = dx*dx+dy*dy;
        if(dist2 > 1600 && dist2 <= 547600) {
          int angle;
          if(dx == 0) {
            angle = dy > 0? 90: 270;
          } else {
            angle = atan(dy*100000/dx);
            if(dx < 0) angle += 180;
          }
          cannon(angle,sqrt(dist2));
        }
      }
    } else {
      cannon(scan,range);
      scan -= 10;
    }
  }
}


}
*/