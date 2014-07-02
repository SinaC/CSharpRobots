using System;

namespace SDK
{
    public class Vector
    {
        /// <summary>
        /// x-component of vector.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// y-component of vector.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// t-component of vector.
        /// </summary>
        public double T { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes to X=0, Y=0, T=1.
        /// </summary>
        public Vector() : this(0, 0, 1)
        {
        }

        /// <summary>
        /// Initializes to x, y, T=1.
        /// </summary>
        /// <param name="x">is the x-component.</param>
        /// <param name="y">is the y-component.</param>
        public Vector(double x, double y) : this(x, y, 1)
        {
        }

        /// <summary>
        /// Initializes to x, y, t.
        /// </summary>
        /// <param name="x">is the x-component.</param>
        /// <param name="y">is the y-component.</param>
        /// <param name="t">is the t-component.</param>
        public Vector(double x, double y, double t)
        {
            X = x;
            Y = y;
            T = t;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="v">is the original vector.</param>
        public Vector(Vector v) : this(v.X, v.Y, v.T)
        {
        }

        #endregion

        #region Factory
        
        /// <summary>
        /// Construct a vector using polar coordinates instead of cartesian.
        /// </summary>
        /// <param name="radius">is the radius.</param>
        /// <param name="angle">is the angle in radians.</param>
        /// <returns>the vector in cartesian coordinates.</returns>
        public static Vector Polar(double radius, double angle)
        {
            return Polar(radius, angle, 1);
        }

        /// <summary>
        /// Construct a vector using polar coordinates instead of cartesian.
        /// </summary>
        /// <param name="radius">is the radius.</param>
        /// <param name="angle">is the angle in radians.</param>
        /// <param name="t">is the t-component.</param>
        /// <returns>the vector in cartesian coordinates.</returns>
        public static Vector Polar(double radius, double angle, double t)
        {
            return new Vector(radius * Math.Cos(angle), radius * Math.Sin(angle), t);
        }

        #endregion

        #region Math

        /// <summary>
        /// Returns magnitude of vector.
        /// </summary>
        public double Mag
        {
            get { return Math.Sqrt(X * X + Y * Y + T * T); }
        }

        /// <summary>
        /// Returns angle of vector.
        /// </summary>
        public double Angle
        {
            get { return Math.Atan2(Y, X); }
        }

        /// <summary>
        /// Returns radius in polar-coordinates of vector (same as Mag).
        /// </summary>
        public double R
        {
            get { return Mag; }
        }

        /// <summary>
        /// Angle in polar-coordinates of vector (same as Angle).
        /// </summary>
        public double A
        {
            get { return Angle; }
        }

        /// <summary>
        /// Dot product between 2 vectors.
        /// </summary>
        /// <param name="vector">is a vector.</param>
        /// <returns>the dot product between this and <paramref name="vector"/>.</returns>
        public double Dot(Vector vector)
        {
            return Dot(this, vector);
        }

        /// <summary>
        /// Dot product between 2 vectors.
        /// </summary>
        /// <param name="v1">is a vector.</param>
        /// <param name="v2">is a vector.</param>
        /// <returns>the dot product between <paramref name="v1"/> and <paramref name="v2"/>.</returns>
        public static double Dot(Vector v1, Vector v2)
        {
            return v1.X*v2.X + v1.Y*v2.Y + v1.T*v2.T;
        }

        /// <summary>
        /// Returns Speed represented by the vector (Magnitude/T).
        /// </summary>
        public double Speed
        {
            get { return R/T; }
        }
        
        /// <summary>
        /// Computes the velocity vector, which is just the vector pointing in the same direction as "this", but with both time and distance scaled, such as T == 1.
        /// </summary>
        public Vector Velocity
        {
            get { return new Vector(X/T, Y/T, 1); }
        }

        #endregion

        #region Vector Arithmetics

        /// <summary>
        /// Sums 2 vectors (affects X, Y and T).
        /// </summary>
        /// <param name="v1">is a vector.</param>
        /// <param name="v2">is a vector.</param>
        /// <returns>the sum of <paramref name="v1"/> and <paramref name="v2"/>.</returns>
        public static Vector operator+(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.T + v2.T);
        }

        /// <summary>
        /// Substracts 2 vectors (affects X, Y and T).
        /// </summary>
        /// <param name="v1">is a vector.</param>
        /// <param name="v2">is a vector.</param>
        /// <returns>the substraction of <paramref name="v1"/> and <paramref name="v2"/>.</returns>
        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y, v1.T - v2.T);
        }

        /// <summary>
        /// Scales a vector (affects X, Y and T).
        /// </summary>
        /// <param name="v">is a vector.</param>
        /// <param name="d">is the scaler.</param>
        /// <returns>the vector <paramref name="v"/> multiplied by <paramref name="d"/>.</returns>
        public static Vector operator*(Vector v, double d)
        {
            return new Vector(v.X*d, v.Y*d, v.T*d);
        }

        /// <summary>
        /// Rotates a vector by an angle (affects X and Y, not T)
        /// </summary>
        /// <param name="v">is a vector</param>
        /// <param name="degrees">is an angle in degrees</param>
        /// <returns>the vector <paramref name="v"/> rotates by <paramref name="degrees"/> degrees.</returns>
        public static Vector Rotate(Vector v, int degrees)
        {
            double radians = Common.Helpers.Math.ToRadians(degrees);
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            return new Vector(v.X*cos - v.Y*sin, v.X*sin + v.Y*cos);
        }

        #endregion

        #region Helpful functions

        /// <summary>
        /// Compute the distance between 2 vectors.
        /// </summary>
        /// <param name="v">is a vector.</param>
        /// <returns>the distance between this and <paramref name="v"/>.</returns>
        public double Dist(Vector v)
        {
            return Dist(this, v);
        }

        /// <summary>
        /// Compute the distance between 2 vectors.
        /// </summary>
        /// <param name="v1">is a vector.</param>
        /// <param name="v2">is a vector.</param>
        /// <returns>the distance between <paramref name="v1"/> and <paramref name="v2"/>.</returns>
        public double Dist(Vector v1, Vector v2)
        {
            return (v1 - v2).Mag;
        }

        #endregion
    }
}
