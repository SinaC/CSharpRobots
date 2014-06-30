using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public abstract class Robot
    {
        private readonly DateTime _matchStart;
        private readonly int _team;
        private readonly IBattlefield _battlefield;

        private DateTime _lastLaunchTime;

        #region Available Functions/Properties

        /// <summary>
        /// returns the damage points of the robot: 0 to 99 means alive, 100 means dead (the robot will never read this value).
        /// </summary>
        protected int Damage { get; private set; }

        /// <summary>
        /// returns the X coordinate of the robot in the battlefield (the origin is in the upper-left corner and X coordinates increase to the right).
        /// </summary>
        protected int LocX { get; private set; }

        /// <summary>
        /// returns the Y coordinate of the robot in the battlefield (the origin is in the upper-left corner and Y coordinates increase to the bottom).
        /// </summary>
        protected int LocY { get; private set; }

        /// <summary>
        /// returns the speed of the robot in percent: 0 means 0 m/s, 100 means 30 m/s.
        /// </summary>
        protected int Speed { get; private set; }

        /// <summary>
        /// returns the elapsed seconds from the beginning of the fight (this function wasn't included in the original Crobots game. It's available to get better timing control).
        /// </summary>
        protected double Time
        {
            get { return (DateTime.Now - _matchStart).TotalSeconds; }
        }

        /// <summary>
        /// returns the identification number of the robot in the team. If there are n robots in the team, this number goes from 0, the first robot created, to n-1, the last one (this function wasn't included in the original Crobots game. It's available to distinguish between robots in the team and double matches).
        /// </summary>
        protected int Id { get; private set; }

        /// <summary>
        /// Fire a missile if not reloading.
        /// </summary>
        /// <param name="degrees">is the direction in degrees of the shot (angles start from 3 o'clock and increase clockwise)</param>
        /// <param name="range">is the distance where the missile explodes.</param>
        /// <returns>1 if the missile was fired, 0 if not (due to reload time).</returns>
        protected int Cannon(int degrees, int range)
        {
            if (degrees < 0 || degrees > 359)
                return 0;
            if (range < 0 || range > 700)
                return 0;
            DateTime launchTime = DateTime.Now;// save time for further use
            if ((DateTime.Now - _lastLaunchTime).TotalSeconds > 1)
                return 0; // reload
            _lastLaunchTime = launchTime;
            return _battlefield.Cannon(this, degrees, range);
        }

        /// <summary>
        /// Change robot's direction and speed.
        /// </summary>
        /// <param name="degrees">is the direction of movement of the robot (angles start from 3 o'clock and increase clockwise). You must remember that robots can change their direction only if the speed is lower that 50% (say 15 m/s).</param>
        /// <param name="speed">is the speed in percent that the robot must reach: 0 means 0 m/s, 100 means 30 m/s.</param>
        protected void Drive(int degrees, int speed)
        {
            if (degrees < 0 || degrees > 359)
                return;
            if (speed < 0 || speed > 100)
                return;
            _battlefield.Drive(degrees, speed);
        }

        /// <summary>
        /// Scan battlefield for robots
        /// </summary>
        /// <param name="degrees">is the direction in degrees of the scan (angles start from 3 o'clock and increase clockwise).</param>
        /// <param name="resolution">is the width of the scan in degrees (scan start degrees-resolution/2 to degrees+resolution/2). Its value must be greater than 0 and lesser than 21 degrees.</param>
        /// <returns>the distance of the nearest robot found in that sector, or zero if no robot was found.</returns>
        protected int Scan(int degrees, int resolution)
        {
            if (degrees < 0 || degrees > 359)
                return 0;
            if (resolution <= 0 || resolution >= 21)
                return 0;
            return _battlefield.Scan(degrees, resolution);
        }

        #endregion

        internal Robot(IBattlefield battlefield, int id, int team, DateTime matchStart, int locX, int locY)
        {
            _battlefield = battlefield;
            Id = id;
            _team = team;
            _matchStart = matchStart;
            LocX = locX;
            LocY = locY;
            Damage = 0;
            Speed = 0;
        }
    }
}
