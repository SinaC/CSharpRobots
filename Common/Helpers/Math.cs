namespace Common.Helpers
{
    public static class Math
    {
        public static double ToRadians(double degrees)
        {
            return degrees * System.Math.PI / 180.0;
        }

        public static double ToDegrees(double radians)
        {
            return (int)(radians * 180.0 / System.Math.PI);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            return System.Math.Sqrt(x*x + y*y);
        }

        public static void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = degrees * System.Math.PI / 180.0;
            x = centerX + distance * System.Math.Cos(radians);
            y = centerY - distance * System.Math.Sin(radians);
        }

        private static double Sign(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            return (p1X - p3X) * (p2Y - p3Y) - (p2X - p3X) * (p1Y - p3Y);
        }

        private static bool IsPointInTriangle(double ptX, double ptY, double v1X, double v1Y, double v2X, double v2Y, double v3X, double v3Y)
        {
            bool b1 = Sign(ptX, ptY, v1X, v1Y, v2X, v2Y) < 0.0f;
            bool b2 = Sign(ptX, ptY, v2X, v2X, v3X, v3Y) < 0.0f;
            bool b3 = Sign(ptX, ptY, v3X, v3Y, v1X, v1Y) < 0.0f;

            return (b1 == b2) && (b2 == b3);
        }

        public static bool IsInSector(double centerX, double centerY, int degrees, int resolution, double pointX, double pointY)
        {
            const double arbitratryLength = 2000.0; // greater than battlefield

            double angleFromDegrees = (degrees - resolution / 2.0);
            double angleToDegrees = (degrees + resolution / 2.0);

            double pointFromX, pointFromY;
            ComputePoint(centerX, centerY, arbitratryLength, angleFromDegrees, out pointFromX, out pointFromY);
            double pointToX, pointToY;
            ComputePoint(centerX, centerY, arbitratryLength, angleToDegrees, out pointToX, out pointToY);

            return IsPointInTriangle(pointX, pointY, centerX, centerY, pointFromX, pointFromY, pointToX, pointToY);
        }
    }
}
