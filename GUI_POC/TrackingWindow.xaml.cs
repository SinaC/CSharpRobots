using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI_POC
{
    /// <summary>
    /// Interaction logic for TrackingWindow.xaml
    /// </summary>
    public partial class TrackingWindow : Window
    {
        private const int StepDelay = 25;
        public const double SimulationAccelerationFactor = 10;
        private const double ArenaSize = 1000.0;

        private readonly List<RobotBase> _robots;

        public TrackingWindow()
        {
            InitializeComponent();

            // Add background grid
            Grid backgroundGrid = CreateBackgroundGrid();
            Canvas.SetTop(backgroundGrid, 0);
            Canvas.SetLeft(backgroundGrid, 0);
            Panel.SetZIndex(backgroundGrid, -10);
            BattlefieldCanvas.Children.Add(backgroundGrid);

            // Create robots
            _robots = new List<RobotBase>
            {
                new TrackingRobot(BattlefieldCanvas, 1, 1, 100, 100, 0, 0, true, true),
                new TrackingRobot(BattlefieldCanvas, 1, 2, 100, 400, 0, 0, true, true),
                new TrackingRobot(BattlefieldCanvas, 1, 3, 900, 900, 0, 0, true, true),
                //new TrackingRobot(BattlefieldCanvas, 1, 4, 900, 100, 0, 0, true, true),

                new RobotBase(BattlefieldCanvas, 2, 1, 200, 100, 0, 0),
                new RobotBase(BattlefieldCanvas, 2, 2, 200, 300, 0, 0),
                new RobotBase(BattlefieldCanvas, 2, 3, 500, 300, 0, 0),
                //new RobotBase(BattlefieldCanvas, 2, 4, 800, 400, 0, 0),
                //new RobotBase(BattlefieldCanvas, 2, 5, 200, 800, 0, 0),
                //new RobotBase(BattlefieldCanvas, 2, 5, 700, 700, 0, 0),
            };

            //
            Task.Factory.StartNew(MainLoop);
        }

        private void MainLoop()
        {
            foreach (RobotBase robot in _robots)
                robot.Init();

            while (true)
            {
                ExecuteOnUIThread.InvokeAsync(Refresh);

                Thread.Sleep(StepDelay);
            }
        }

        private void Refresh()
        {
            foreach (RobotBase robot in _robots)
                UpdateRobot(robot);
        }

        private void UpdateRobot(RobotBase robot)
        {
            robot.UpdateSimulation(StepDelay * SimulationAccelerationFactor);
            //
            robot.Step(StepDelay * SimulationAccelerationFactor, _robots);
            //
            robot.UpdateUI();
        }

        private Grid CreateBackgroundGrid()
        {
            // Create a grid with lines every 100m
            int lineCount = (int)(ArenaSize / 100.0);
            double cellWidth = BattlefieldCanvas.Width / lineCount;
            double cellHeight = BattlefieldCanvas.Height / lineCount;
            Grid grid = new Grid();
            for (int i = 0; i < lineCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(cellWidth)
                });
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(cellHeight)
                });
            }
            for (int y = 0; y < lineCount; y++)
                for (int x = 0; x < lineCount; x++)
                {
                    Thickness thickness = new Thickness(0, 0, x == lineCount - 1 ? 0 : 1, y == lineCount - 1 ? 0 : 1);
                    Border border = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.DarkGray),
                        BorderThickness = thickness,
                        Background = new SolidColorBrush(Colors.Transparent)
                    };
                    Grid.SetColumn(border, x);
                    Grid.SetRow(border, y);
                    grid.Children.Add(border);
                }
            return grid;
        }
    }
}
