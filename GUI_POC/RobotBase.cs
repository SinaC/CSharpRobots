using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI_POC
{
    public class RobotBase
    {
        public static readonly SolidColorBrush[] TeamBrushes =
        {
            new SolidColorBrush(Colors.Black),
            new SolidColorBrush(Colors.Blue),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Magenta)
        };

        public const double MaxSpeed = 30; // m/s
        public const double ArenaSize = 1000;
        public int Team { get; set; }
        public int Id { get; set; }
        public double LocX { get; set; }
        public double LocY { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public double CosHeading { get; set; }
        public double SinHeading { get; set; }

        public Canvas BattlefieldCanvas { get; set; }

        public FrameworkElement RobotUIElement { get; set; }
        public FrameworkElement LabelUIElement { get; set; }

        public RobotBase(Canvas battlefieldCanvas, int team, int id, double locX, double locY, double speed, double heading)
        {
            BattlefieldCanvas = battlefieldCanvas;

            Team = team;
            Id = id;

            LocX = locX;
            LocY = locY;
            Speed = speed;
            Heading = heading;
            CosHeading = Math.Cos(Heading);
            SinHeading = Math.Sin(Heading);

            RobotUIElement = new Rectangle
            {
                Width = 8,
                Height = 8,
                Fill = TeamBrushes[team],
            };
            LabelUIElement = new TextBlock
            {
                Width = 120,
                Height = 10,
                Text = String.Format("{0}[{1}]", team, id),
                FontSize = 8,
            };
            Panel.SetZIndex(RobotUIElement, 100);
            Panel.SetZIndex(LabelUIElement, 100);
            battlefieldCanvas.Children.Add(RobotUIElement);
            battlefieldCanvas.Children.Add(LabelUIElement);
        }

        public virtual void Init()
        {
        }

        public virtual void Step(double dt, List<RobotBase> robots)
        {
        }

        public virtual void UpdateSimulation(double dt)
        {
            //
            LocX += CosHeading * Speed * (MaxSpeed / 100.0) * (dt / 1000.0);
            LocY += SinHeading * Speed * (MaxSpeed / 100.0) * (dt / 1000.0);
            //
            if (LocX < 0)
                LocX = 0;
            else if (LocX > ArenaSize)
                LocX = ArenaSize;
            if (LocY < 0)
                LocY = 0;
            else if (LocY > ArenaSize)
                LocY = ArenaSize;
        }

        public virtual void UpdateUI()
        {
            UpdateUIPosition(RobotUIElement, LocX, LocY);
            UpdateUIPositionRelative(LabelUIElement, -5, 5, RobotUIElement);
        }

        protected double ConvertLocX(double locX)
        {
            return locX / (ArenaSize / BattlefieldCanvas.Width);
        }

        protected double ConvertLocY(double locY)
        {
            return locY / (ArenaSize / BattlefieldCanvas.Height);
        }

        protected void UpdateUIPosition(FrameworkElement element, double locX, double locY)
        {
            double posX = locX / (ArenaSize / BattlefieldCanvas.Width) - element.Width / 2.0;
            double posY = locY / (ArenaSize / BattlefieldCanvas.Height) - element.Height / 2.0;
            Canvas.SetTop(element, posY);
            Canvas.SetLeft(element, posX);
        }

        protected void UpdateUIPositionRelative(FrameworkElement element, double stepX, double stepY, FrameworkElement relativeTo)
        {
            double posX = Canvas.GetLeft(relativeTo) + stepX;
            double posY = Canvas.GetTop(relativeTo) + stepY;
            Canvas.SetTop(element, posY);
            Canvas.SetLeft(element, posX);
        }

        protected void Drive(double angle, double speed)
        {
            Speed = speed;
            Heading = angle;
            CosHeading = Math.Cos(Heading);
            SinHeading = Math.Sin(Heading);
        }
        
        protected static double Angle(double x1, double y1, double x2, double y2)
        {
            double diffX = x2 - x1;
            double diffY = y2 - y1;
            double angleRadians = Math.Atan2(diffY, diffX);
            if (angleRadians >= Math.PI)
                angleRadians = 2 * Math.PI - angleRadians;
            return angleRadians;
        }

        protected static double Distance(double x1, double y1, double x2, double y2)
        {
            double diffX = x2 - x1;
            double diffY = y2 - y1;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
