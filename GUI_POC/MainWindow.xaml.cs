using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI_POC
{
    public class Item : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const double CenterX = 250;
        public const double CenterY = 250;
        public const int SectorAngle = 70;
        public const int SectorResolution = 20;

        public List<Item> Items;

        public MainWindow()
        {
            InitializeComponent();

            Items = new List<Item>
                {
                    new Item
                        {
                            Name = "Name1",
                            IsSelected = false,
                        },
                    new Item
                        {
                            Name = "Name2",
                            IsSelected = false,
                        }
                };
            TestList.DataContext = Items;
        }

        public void MainWindow2()
        {
            InitializeComponent();

            // Create a background to enable hit test
            Rectangle background = new Rectangle
                {
                    Width = ArenaCanvas.Width,
                    Height = ArenaCanvas.Height,
                    Fill = new SolidColorBrush(Colors.White)
                };
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);
            ArenaCanvas.Children.Add(background);

            for (int i = 0; i < 360; i += 10)
                ComputeAndDisplayPoint(i, CenterX, CenterY);

            DisplaySector(CenterX, CenterY, SectorAngle, SectorResolution);
        }

        private void ComputeAndDisplayPoint(int degrees, double centerX, double centerY)
        {
            const double arbitratryLength = 150.0;

            double pointX, pointY;
            ComputePoint(centerX, centerY, arbitratryLength, degrees, out pointX, out pointY);
            DisplayText(degrees, pointX, pointY);
        }

        private void DisplayText(int degrees, double x, double y)
        {
            TextBlock textBlock = new TextBlock
                {
                    Text = String.Format("{0}", degrees)
                };
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            ArenaCanvas.Children.Add(textBlock);
        }

        private void DisplayText(string txt, double x, double y)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = txt
            };
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            ArenaCanvas.Children.Add(textBlock);
        }

        private void DisplaySector(double centerX, double centerY, int degrees, int resolution)
        {
            const double arbitratryLength = 200.0;

            double angleFromDegrees = (degrees - resolution/2.0);
            double angleToDegrees = (degrees + resolution/2.0);

            double pointFromX, pointFromY;
            ComputePoint(centerX, centerY, arbitratryLength, angleFromDegrees, out pointFromX, out pointFromY);
            double pointToX, pointToY;
            ComputePoint(centerX, centerY, arbitratryLength, angleToDegrees, out pointToX, out pointToY);

            Path path = new Path
                {
                    Stroke = Brushes.Black,
                    //Fill = Brushes.Red,
                    StrokeThickness = 1,
                    Data = Geometry.Parse(String.Format("M {0},{1} L {2},{3} L {4}, {5}Z", centerX, centerY, Math.Round(pointFromX), Math.Round(pointFromY), Math.Round(pointToX), Math.Round(pointToY)))
                };
            Canvas.SetLeft(path, 0);
            Canvas.SetTop(path, 0);
            ArenaCanvas.Children.Add(path);

            DisplayText((int) angleFromDegrees, pointFromX, pointFromY);
            DisplayText((int) angleToDegrees, pointToX, pointToY);
        }

        private bool CheckSector(double centerX, double centerY, int degrees, int resolution, double pointX, double pointY)
        {
            const double arbitratryLength = 250.0; // greater than battlefield

            double angleFromDegrees = (degrees - resolution/2.0);
            double angleToDegrees = (degrees + resolution/2.0);

            double pointFromX, pointFromY;
            ComputePoint(centerX, centerY, arbitratryLength, angleFromDegrees, out pointFromX, out pointFromY);
            double pointToX, pointToY;
            ComputePoint(centerX, centerY, arbitratryLength, angleToDegrees, out pointToX, out pointToY);

            DisplayText("from", pointFromX, pointFromY);
            DisplayText("to", pointToX, pointToY);

            return IsPointInTriangle(pointX, pointY, centerX, centerY, pointFromX, pointFromY, pointToX, pointToY);
        }

        private static double Sign(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            return (p1X - p3X)*(p2Y - p3Y) - (p2X - p3X)*(p1Y - p3Y);
        }

        private static int _posY = 0;

        private bool IsPointInTriangle2(double ptX, double ptY, double v1X, double v1Y, double v2X, double v2Y, double v3X, double v3Y)
        {
            bool b1 = Sign(ptX, ptY, v1X, v1Y, v2X, v2Y) > 0.0f;
            bool b2 = Sign(ptX, ptY, v2X, v2X, v3X, v3Y) < 0.0f;
            bool b3 = Sign(ptX, ptY, v3X, v3Y, v1X, v1Y) > 0.0f;

            DisplayText(String.Format("{0}{1}{2}", b1, b2, b3), 10, _posY);
            _posY += 10;

            return (b1 == b2) && (b2 == b3);
        }

        private bool IsPointInTriangle(double ptX, double ptY, double t1X, double t1Y, double t2X, double t2Y, double t3X, double t3Y)
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
            double dot00 = v0X*v0X + v0Y*v0Y;
            double dot01 = v0X*v1X + v0Y*v1Y;
            double dot02 = v0X*v2X + v0Y*v2Y;
            double dot11 = v1X*v1X + v1Y*v1Y;
            double dot12 = v1X*v2X + v1Y*v2Y;

            // Compute barycentric coordinates
            double invDenom = 1.0/(dot00*dot11 - dot01*dot01);
            double u = (dot11*dot02 - dot01*dot12)*invDenom;
            double v = (dot00*dot12 - dot01*dot02)*invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        private static void ComputePoint(double centerX, double centerY, double distance, double degrees, out double x, out double y)
        {
            double radians = degrees*Math.PI/180.0;
            x = centerX + distance*Math.Cos(radians);
            y = centerY + distance*Math.Sin(radians);
        }

        private void ArenaCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(ArenaCanvas);
            bool isInSector = CheckSector(CenterX, CenterY, SectorAngle, SectorResolution, point.X, point.Y);

            Ellipse dot = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = isInSector ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
                };
            Canvas.SetLeft(dot, point.X);
            Canvas.SetTop(dot, point.Y);
            ArenaCanvas.Children.Add(dot);
        }
    }
}
