namespace Common
{
    public static class Math
    {
        public static double ToRadians(double degrees)
        {
            return degrees * System.Math.PI / 180.0;
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180.0 / System.Math.PI;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            return System.Math.Sqrt(x*x + y*y);
        }

        public static void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = ToRadians(degrees);
            x = centerX + distance * System.Math.Cos(radians);
            y = centerY + distance * System.Math.Sin(radians);
        }

        private static bool IsPointInTriangle(double ptX, double ptY, double t1X, double t1Y, double t2X, double t2Y, double t3X, double t3Y)
        {
            //http://www.blackpawn.com/texts/pointinpoly/
            // Compute vectors        
            double v0X = t3X - t1X;
            double v0Y = t3Y - t1Y;
            double v1X = t2X - t1X;
            double v1Y = t2Y - t1Y;
            double v2X = ptX - t1X;
            double v2Y = ptY - t1Y;

            // Compute dot products
            double dot00 = v0X * v0X + v0Y * v0Y;
            double dot01 = v0X * v1X + v0Y * v1Y;
            double dot02 = v0X * v2X + v0Y * v2Y;
            double dot11 = v1X * v1X + v1Y * v1Y;
            double dot12 = v1X * v2X + v1Y * v2Y;

            // Compute barycentric coordinates
            double invDenom = 1.0 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        public static bool IsInSector(double centerX, double centerY, int degrees, int resolution, double pointX, double pointY)
        {
            // Simulate a triangle bigger than sector and check on that triangle
            const double arbitratryLength = 2000.0; // greater than battlefield

            double angleFromDegrees = degrees - (resolution / 2.0);
            double angleToDegrees = degrees + (resolution / 2.0);

            double pointFromX, pointFromY;
            ComputePoint(centerX, centerY, arbitratryLength, angleFromDegrees, out pointFromX, out pointFromY);
            double pointToX, pointToY;
            ComputePoint(centerX, centerY, arbitratryLength, angleToDegrees, out pointToX, out pointToY);

            return IsPointInTriangle(pointX, pointY, centerX, centerY, pointFromX, pointFromY, pointToX, pointToY);
        }
    }
}
