using System.Collections.Generic;

namespace SDK
{
    //http://jrobots.sourceforge.net/jjr_info.shtml

    public interface ISDKRobot
    {
        /// <summary>
        /// Returns the damage points of the robot: 0 to 99 means alive, 100 means dead (the robot will never read this value).
        /// </summary>
        int Damage { get; }

        /// <summary>
        /// Returns the X coordinate of the robot in the battlefield (the origin is in the upper-left corner and X coordinates increase to the right).
        /// </summary>
        int LocX { get; }

        /// <summary>
        /// Returns the Y coordinate of the robot in the battlefield (the origin is in the upper-left corner and Y coordinates increase to the bottom).
        /// </summary>
        int LocY { get; }

        /// <summary>
        /// Returns the speed of the robot in percent: 0 means 0 m/s, 100 means 30 m/s.
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// Returns the elapsed seconds from the beginning of the fight (this function wasn't included in the original Crobots game. It's available to get better timing control).
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Returns the identification number of the robot in the team. If there are n robots in the team, this number goes from 0, the first robot created, to n-1, the last one (this function wasn't included in the original Crobots game. It's available to distinguish between robots in the team and double matches).
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Fire a missile if not reloading.
        /// </summary>
        /// <param name="degrees">is the direction in degrees of the shot (angles start from 3 o'clock and increase clockwise).</param>
        /// <param name="range">is the distance where the missile explodes.</param>
        /// <Returns>1 if the missile was fired, 0 if not (due to reload time).</Returns>
        int Cannon(int degrees, int range);

        /// <summary>
        /// Change robot's direction and speed.
        /// </summary>
        /// <param name="degrees">is the direction of movement of the robot (angles start from 3 o'clock and increase clockwise). You must remember that robots can change their direction only if the speed is lower that 50% (say 15 m/s).</param>
        /// <param name="speed">is the speed in percent that the robot must reach: 0 means 0 m/s, 100 means 30 m/s.</param>
        void Drive(int degrees, int speed);

        /// <summary>
        /// Scan battlefield for robots.
        /// </summary>
        /// <param name="degrees">is the direction in degrees of the scan (angles start from 3 o'clock and increase clockwise).</param>
        /// <param name="resolution">is the width of the scan in degrees (scan start degrees-resolution/2 to degrees+resolution/2). Its value must be greater than 0 and lesser than 21 degrees.</param>
        /// <Returns>the distance of the nearest robot found in that sector, or zero if no robot was found.</Returns>
        int Scan(int degrees, int resolution);

        /// <summary>
        /// Gives 1 for Single, 2 for Double and 8 for Team
        /// </summary>
        int FriendsCount { get; }

        /// <summary>
        /// Stores arena/robot parameters such as MaxSpeed, ArenaSize, ...
        /// </summary>
        IReadOnlyDictionary<string, int> Parameters { get; }

        #region Math

        /// <summary>
        /// Get random int value in range [0, limit[
        /// </summary>
        /// <param name="limit">is the max value.</param>
        /// <Returns>a value between 0 and <paramref name="limit"/>-1.</Returns>
        int Rand(int limit);

        /// <summary>
        /// Get squared-root of specified value.
        /// </summary>
        /// <param name="value">is the value</param>
        /// <Returns>the squared-root of <paramref name="value"/>.</Returns>
        int Sqrt(int value);

        /// <summary>
        /// Get sine of specified value.
        /// </summary>
        /// <param name="degrees">is an angle in degrees.</param>
        /// <Returns>the sine of <paramref name="degrees"/>*100000.</Returns>
        int Sin(int degrees);

        /// <summary>
        /// Get cosine of specified value.
        /// </summary>
        /// <param name="degrees">is an angle in degrees.</param>
        /// <Returns>the cosine of <paramref name="degrees"/>*100000.</Returns>
        int Cos(int degrees);

        /// <summary>
        /// Get tangent of specified value.
        /// </summary>
        /// <param name="degrees">is an angle in degrees.</param>
        /// <Returns>the tangent of <paramref name="degrees"/>*100000.</Returns>
        int Tan(int degrees);

        /// <summary>
        /// Get arctangent of specified value.
        /// </summary>
        /// <param name="value">is the value (must be 100000 times the real value).</param>
        /// <Returns>the arctangent of <paramref name="value"/> in degrees.</Returns>
        int ATan(int value);

        /// <summary>
        /// Get squared-root of specified value.
        /// </summary>
        /// <param name="value">is the value</param>
        /// <Returns>the squared-root of <paramref name="value"/>.</Returns>
        double Sqrt(double value);

        /// <summary>
        /// Get sine of specified value.
        /// </summary>
        /// <param name="radians">is an angle in radians.</param>
        /// <Returns>the sine of <paramref name="radians"/></Returns>
        double Sin(double radians);

        /// <summary>
        /// Get cosine of specified value.
        /// </summary>
        /// <param name="radians">is an angle in radians.</param>
        /// <Returns>the cosine of <paramref name="radians"/></Returns>
        double Cos(double radians);

        /// <summary>
        /// Get tangent of specified value.
        /// </summary>
        /// <param name="radians">is an angle in radians.</param>
        /// <Returns>the tangent of <paramref name="radians"/></Returns>
        double Tan(double radians);

        /// <summary>
        /// Get arctangent of specified value.
        /// </summary>
        /// <param name="value">is the value.</param>
        /// <Returns>the arctangent of <paramref name="value"/> in radians.</Returns>
        double ATan(double value);

        /// <summary>
        /// Convert specfied value from degrees to radians.
        /// </summary>
        /// <param name="degrees">is the angle to convert.</param>
        /// <Returns>the angle converted in radians.</Returns>
        double Deg2Rad(double degrees);

        /// <summary>
        /// Convert specfied value from radians to degrees.
        /// </summary>
        /// <param name="radians">is the angle to convert.</param>
        /// <returns>the angle converted in degrees.</returns>
        double Rad2Deg(double radians);

        /// <summary>
        /// Get absolute value of a specified number.
        /// </summary>
        /// <param name="value">is a value</param>
        /// <returns>the absolute value of <paramref name="value"/>.</returns>
        double Abs(double value);

        /// <summary>
        /// Round specified number to double
        /// </summary>
        /// <param name="value">is a value</param>
        /// <returns>the rounded value of <paramref name="value"/></returns>
        double Round(double value);

        /// <summary>
        /// Get e raised the specified power
        /// </summary>
        /// <param name="power">is the power</param>
        /// <returns>e raised to specified power</returns>
        double Exp(double power);

        /// <summary>
        /// Get the natural logarithm of a specified number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        double Log(double value);

        /// <summary>
        /// Returns the angle whose tangent is the quotient of two specified numbers.
        /// </summary>
        /// <param name="x">The y coordinate of a point.</param>
        /// <param name="y">The x coordinate of a point.</param>
        /// <returns>An angle, θ, measured in radians, such that -π≤θ≤π, and tan(θ) = y / x, where
        ///     (x, y) is a point in the Cartesian plane. Observe the following: For (x,
        ///     y) in quadrant 1, 0 &lt; θ &lt; π/2.For (x, y) in quadrant 2, π/2 &lt; θ≤π.For (x,
        ///     y) in quadrant 3, -π &lt; θ &lt; -π/2.For (x, y) in quadrant 4, -π/2 &lt; θ &lt; 0.For
        ///     points on the boundaries of the quadrants, the return value is the following:If
        ///     y is 0 and x is not negative, θ = 0.If y is 0 and x is negative, θ = π.If
        ///     y is positive and x is 0, θ = π/2.If y is negative and x is 0, θ = -π/2.If
        ///     x or y is System.Double.NaN, or if x and y are either System.Double.PositiveInfinity
        ///     or System.Double.NegativeInfinity, the method returns System.Double.NaN.</returns>
        double ATan2(double y, double x);

        #endregion
    }
}
