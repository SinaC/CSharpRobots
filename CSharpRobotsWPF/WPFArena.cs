using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Arena;
using Common;

namespace CSharpRobotsWPF
{
    public class WPFArena
    {
        public bool ShowTraces = true;
        public bool ShowMissileTarget = true;
        public bool ShowMissileExplosion = true;

        private static readonly SolidColorBrush[] TeamBrushes = new[]
            {
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Magenta)
            };
        private static readonly Brush TargetBrush = new SolidColorBrush(Colors.Black);
        private static readonly Brush Explosion5Brush = new SolidColorBrush(Colors.Red);
        private static readonly Brush Explosion20Brush = new SolidColorBrush(Colors.Orange);
        private static readonly Brush Explosion40Brush = new SolidColorBrush(Colors.Yellow);

        private readonly MainWindow _mainWindow;
        private readonly Grid _backgroundGrid;

        private readonly IReadonlyArena _arena;

        private ObservableCollection<WPFRobot> _wpfRobots;
        private ObservableCollection<WPFRobot> _wpfDeadRobots;
        private ObservableCollection<WPFMissile> _wpfMissiles;

        public WPFArena(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            //_arena = Factory.CreateCRobotArena();
            _arena = Factory.CreateJRobotArena();
            _arena.ArenaStarted += OnArenaStarted;
            _arena.ArenaStopped += OnArenaStopped;
            _arena.ArenaStep += OnArenaStep;

            _backgroundGrid = CreateBackgroundGrid();
            Canvas.SetTop(_backgroundGrid, 0);
            Canvas.SetLeft(_backgroundGrid, 0);
            Panel.SetZIndex(_backgroundGrid, -10);
            _mainWindow.BattlefieldCanvas.Children.Add(_backgroundGrid);
        }

        public void StartStop()
        {
            // Test mode without selection needed
            //StartStopInternal(arena => arena.InitializeDoubleMatch(typeof(Robots.SinaC), typeof(Robots.Stinger)));
            //StartStopInternal(arena => arena.InitializeDoubleMatch(typeof(Robots.SinaC), typeof(Robots.Rabbit)));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Stinger), 500, 500, 50, 150));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Rabbit), 500, 500, 50, 150));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Target), 10, 10, 400, 500));
            StartStopInternal(arena => arena.InitializeSolo(typeof(Robots.SinaC), 10, 10, 0, 0));
            //StartStopInternal(arena => arena.InitializeFreeMode(3, GetFreeModeCoordinates, typeof(Robots.SinaC), typeof(Robots.Target)));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Target), 10, 10, 400, 500));
            //StartStopInternal(arena => arena.InitializeFreeMode(3, GetFreeModeCoordinates, typeof(Robots.SinaC), typeof(Robots.Target)));
            //StartStopInternal(arena => arena.InitializeDoubleMatch(typeof(Robots.SinaC), typeof(Robots.Maruko)));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Target), 475, 500, 50, 150));
            //StartStopInternal(arena => arena.InitializeSingleMatch(typeof(Robots.SinaC), typeof(Robots.Maruko)));
        }
        
        private static Tuple<int, int> GetFreeModeCoordinates(int teamId, int robotId)
        {
            switch(teamId)
            {
                case 0:
                    switch(robotId)
                    {
                        case 0:
                            return new Tuple<int, int>(100, 100);
                        case 1:
                            return new Tuple<int, int>(100, 400);
                        case 2:
                            return new Tuple<int, int>(900, 900);
                    }
                    break;
                case 1:
                    switch (robotId)
                    {
                        case 0:
                            return new Tuple<int, int>(200, 100);
                        case 1:
                            return new Tuple<int, int>(200, 300);
                        case 2:
                            return new Tuple<int, int>(900, 300);
                    }
                    break;
            }
            return new Tuple<int, int>(900, 900);
        }

        public void StartSolo(Type type)
        {
            StartStopInternal(arena => arena.InitializeSolo(type, 500, 500, 0, 0));
        }

        public void StartSingle(Type robot1, Type robot2)
        {
            StartStopInternal(arena => arena.InitializeSingleMatch(robot1, robot2));
        }

        public void StartSingle4(Type robot1, Type robot2, Type robot3, Type robot4)
        {
            StartStopInternal(arena => arena.InitializeSingle4Match(robot1, robot2, robot3, robot4));
        }

        public void StartDouble(Type robot1, Type robot2)
        {
            StartStopInternal(arena => arena.InitializeDoubleMatch(robot1, robot2));
        }

        public void StartDouble4(Type robot1, Type robot2, Type robot3, Type robot4)
        {
            StartStopInternal(arena => arena.InitializeDouble4Match(robot1, robot2, robot3, robot4));
        }

        public void StartTeam(Type robot1, Type robot2, Type robot3, Type robot4)
        {
            StartStopInternal(arena => arena.InitializeTeamMatch(robot1, robot2, robot3, robot4));
        }

        private void StartStopInternal(Action<IReadonlyArena> initializeArenaAction)
        {
            if (_arena == null)
                return;
            if (_arena.State == ArenaStates.Running)
                _arena.StopMatch();
            else
            {
                if (_arena.State == ArenaStates.Error)
                    _mainWindow.StatusText.Text = "Error while creating match";
                else
                {
                    // Initialize
                    initializeArenaAction(_arena);

                    // Reset battlefield
                    _mainWindow.BattlefieldCanvas.Children.Clear();
                    Canvas.SetTop(_backgroundGrid, 0);
                    Canvas.SetLeft(_backgroundGrid, 0);
                    Panel.SetZIndex(_backgroundGrid, -10);
                    _mainWindow.BattlefieldCanvas.Children.Add(_backgroundGrid);
                    // Create WPF robots
                    _wpfMissiles = new ObservableCollection<WPFMissile>();
                    _wpfRobots = new ObservableCollection<WPFRobot>();
                    foreach (IReadonlyRobot robot in _arena.Robots.OrderBy(x => x.Team).ThenBy(x => x.Id))
                        CreateRobot(robot);
                    _wpfDeadRobots = new ObservableCollection<WPFRobot>();

                    //
                    _mainWindow.AliveRobotInformationsList.DataContext = _wpfRobots;
                    _mainWindow.DeadRobotInformationsList.DataContext = _wpfDeadRobots;

                    // Start match
                    _arena.StartMatch();
                }
            }
        }

        private void OnArenaStep(IReadonlyArena arena)
        {
            ExecuteOnUIThread.InvokeAsync(Refresh); // TODO: invoke sync or async
        }

        private void OnArenaStopped(IReadonlyArena arena)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                _mainWindow.StartButton.Content = "Start";
                UpdateStatus();
            });
        }

        private void OnArenaStarted(IReadonlyArena arena)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                _mainWindow.StartButton.Content = "Stop";
                UpdateStatus();
            });
        }

        private void Refresh()
        {
            //Log.WriteLine(Log.LogLevels.Debug, "WPF - REFRESH");

            // Display missiles
            List<IReadonlyMissile> missiles = _arena.Missiles;
            // Update existing or delete missing
            foreach (WPFMissile wpfMissile in _wpfMissiles)
            {
                IReadonlyMissile missile = missiles.FirstOrDefault(x => x.Id == wpfMissile.Id);
                if (missile != null)
                    UpdateMissile(wpfMissile, missile);
                else
                    DeleteMissile(wpfMissile);
            }
            // Create new missiles
            foreach (IReadonlyMissile missile in missiles)
            {
                WPFMissile wpfMissile = _wpfMissiles.FirstOrDefault(x => x.Id == missile.Id);
                if (wpfMissile == null)
                    CreateMissile(missile);
            }
            // Display robots
            List<IReadonlyRobot> robots = _arena.Robots;
            foreach (WPFRobot wpfRobot in _wpfRobots)
            {
                IReadonlyRobot robot = robots.FirstOrDefault(x => x.Id == wpfRobot.Id && x.Team == wpfRobot.Team);
                if (robot != null)
                    UpdateRobot(wpfRobot, robot);
                else
                    DeleteRobot(wpfRobot);
            }

            // Update status
            UpdateStatus();

            // Update alive/dead robots
            List<WPFRobot> deadRobots = _wpfRobots.Where(x => !x.IsAlive).ToList();
            foreach(WPFRobot robot in deadRobots)
            {
                _wpfDeadRobots.Insert(0, robot); // head-insertion
                _wpfRobots.Remove(robot);
            }
        }

        private void UpdateStatus()
        {
            switch (_arena.State)
            {
                case ArenaStates.Running:
                    _mainWindow.StatusText.Text = "Running";
                    break;
                case ArenaStates.Winner:
                    _mainWindow.StatusText.Text = String.Format("And the winner is Team {0} in {1:0.00} seconds", _arena.WinningTeamName, _arena.MatchTime);
                    break;
                case ArenaStates.Draw:
                    _mainWindow.StatusText.Text = "Draw - No winner";
                    break;
                case ArenaStates.Timeout:
                    _mainWindow.StatusText.Text = "Timeout";
                    break;
                case ArenaStates.Stopped:
                    _mainWindow.StatusText.Text = "Stopped";
                    break;
                default:
                    _mainWindow.StatusText.Text = String.Format("State : {0}", _arena.State);
                    break;
            }
            if (_arena.State == ArenaStates.Running)
            {
                double maxMatchTime = _arena.Parameters["MaxMatchTime"];
                double elapsedSeconds = Tick.ElapsedSeconds(_arena.MatchStart);
                double timeLeft = maxMatchTime - elapsedSeconds;
                _mainWindow.Title = String.Format("C# Robots - {0:0.00} seconds left", timeLeft);
            }
            else
                _mainWindow.Title = "C# Robots";
        }

        private void UpdateMissile(WPFMissile wpfMissile, IReadonlyMissile missile)
        {
            if (missile.State == MissileStates.Deleted)
                DeleteMissile(wpfMissile);
            else
            {
                UpdateUIPosition(wpfMissile.FlyingUIElement, missile.LocX, missile.LocY);
                UpdateUIPosition(wpfMissile.ExplosionUIElement, missile.LocX, missile.LocY);
                // No need to update target
                if (missile.State == MissileStates.Flying)
                {
                    wpfMissile.FlyingUIElement.Visibility = Visibility.Visible;
                    if (ShowMissileTarget)
                        wpfMissile.TargetUIElement.Visibility = Visibility.Visible;
                    wpfMissile.ExplosionUIElement.Visibility = Visibility.Hidden;
                }
                else if (missile.State == MissileStates.Exploding || missile.State == MissileStates.Exploded)
                {
                    wpfMissile.FlyingUIElement.Visibility = Visibility.Hidden;
                    wpfMissile.TargetUIElement.Visibility = Visibility.Hidden;
                    if (ShowMissileExplosion)
                        wpfMissile.ExplosionUIElement.Visibility = Visibility.Visible;
                }
            }
        }

        private void DeleteMissile(WPFMissile wpfMissile)
        {
            wpfMissile.FlyingUIElement.Visibility = Visibility.Hidden;
            wpfMissile.TargetUIElement.Visibility = Visibility.Hidden;
            wpfMissile.ExplosionUIElement.Visibility = Visibility.Hidden;
        }

        private void CreateMissile(IReadonlyMissile missile)
        {
            WPFMissile wpfMissile = new WPFMissile
            {
                Id = missile.Id,
                FlyingUIElement = new Ellipse
                {
                    Width = 2,
                    Height = 2,
                    Fill = TeamBrushes[missile.Robot.Team]
                },
                TargetUIElement = new Ellipse
                {
                    Width = 80 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"],
                    Height = 80 * _mainWindow.BattlefieldCanvas.Height / _arena.Parameters["ArenaSize"],
                    Stroke = TargetBrush,
                    Visibility = Visibility.Hidden
                },
                ExplosionUIElement = CreateExplosionUIElement()
            };
            _mainWindow.BattlefieldCanvas.Children.Add(wpfMissile.FlyingUIElement);
            _mainWindow.BattlefieldCanvas.Children.Add(wpfMissile.TargetUIElement);
            _mainWindow.BattlefieldCanvas.Children.Add(wpfMissile.ExplosionUIElement);
            UpdateMissile(wpfMissile, missile);
            // Set target
            double explosionX = missile.ExplosionX;
            double explosionY = missile.ExplosionY;
            //double explosionX, explosionY;
            //Common.Math.ComputePoint(missile.LaunchLocX, missile.LaunchLocY, missile.Range, missile.Heading, out explosionX, out explosionY);
            UpdateUIPosition(wpfMissile.TargetUIElement, explosionX, explosionY);
            //
            _wpfMissiles.Add(wpfMissile);
        }

        private void UpdateRobot(WPFRobot wpfRobot, IReadonlyRobot robot)
        {
            wpfRobot.State = robot.State.ToString();
            wpfRobot.Damage = robot.Damage;
            wpfRobot.LocX = robot.LocX;
            wpfRobot.LocY = robot.LocY;
            wpfRobot.Heading = robot.Heading;
            wpfRobot.Speed = robot.Speed;
            wpfRobot.CannonCount = robot.CannonCount;
            lock(robot.Statistics)
                wpfRobot.Statistics = robot.Statistics.Select(x => x).ToDictionary(x => x.Key, x => x.Value);

            if (ShowTraces)
            {
                TimeSpan ts = DateTime.Now - wpfRobot.LastTrace;
                if (ts.TotalMilliseconds > 500)
                {
                    double transposedLocX = ConvertLocX(robot.LocX);
                    double transposedLocY = ConvertLocY(robot.LocY);
                    wpfRobot.TraceUIElement.Points.Add(new Point(transposedLocX, transposedLocY));
                    wpfRobot.LastTrace = DateTime.Now;
                }
            }

            wpfRobot.IsAlive = robot.Damage < _arena.Parameters["MaxDamage"];
            if (robot.State != RobotStates.Running)
                DeleteRobot(wpfRobot);
            else
            {
                wpfRobot.RobotUIElement.Visibility = Visibility.Visible;
                UpdateUIPosition(wpfRobot.RobotUIElement, robot.LocX, robot.LocY);
                UpdateUIPositionRelative(wpfRobot.LabelUIElement, -5, 5, wpfRobot.RobotUIElement);
            }
            if (wpfRobot.IsAlive)
                wpfRobot.LabelUIElement.FontWeight = FontWeights.Bold;
            else
                wpfRobot.LabelUIElement.FontWeight = FontWeights.Normal;
        }

        private void DeleteRobot(WPFRobot wpfRobot)
        {
            wpfRobot.RobotUIElement.Visibility = Visibility.Hidden;
        }

        private void CreateRobot(IReadonlyRobot robot)
        {
            WPFRobot wpfRobot = new WPFRobot
                {
                    Id = robot.Id,
                    Team = robot.Team,
                    Name = robot.TeamName,
                    Color = TeamBrushes[robot.Team],
                    RobotUIElement = new Rectangle
                        {
                            Width = 4,
                            Height = 4,
                            Fill = TeamBrushes[robot.Team],
                        },
                    LabelUIElement = new TextBlock
                        {
                            Width = 120,
                            Height = 10,
                            Text = String.Format("{0}[{1}]", robot.TeamName, robot.Id),
                            FontSize = 8,
                        },
                    TraceUIElement = new Polyline
                        {
                            Stroke = TeamBrushes[robot.Team],
                            StrokeThickness = 1,
                            Opacity = 0.5,
                        }
                };
            Panel.SetZIndex(wpfRobot.RobotUIElement, 100);
            Panel.SetZIndex(wpfRobot.LabelUIElement, 100);
            Panel.SetZIndex(wpfRobot.TraceUIElement, 0);
            _mainWindow.BattlefieldCanvas.Children.Add(wpfRobot.RobotUIElement);
            _mainWindow.BattlefieldCanvas.Children.Add(wpfRobot.LabelUIElement);
            _mainWindow.BattlefieldCanvas.Children.Add(wpfRobot.TraceUIElement);
            UpdateRobot(wpfRobot, robot);
            _wpfRobots.Add(wpfRobot);
        }

        private static void UpdateUIPositionRelative(UIElement element, double stepX, double stepY, UIElement relativeTo)
        {
            double posX = Canvas.GetLeft(relativeTo) + stepX;
            double posY = Canvas.GetTop(relativeTo) + stepY;
            Canvas.SetTop(element, posY);
            Canvas.SetLeft(element, posX);
        }

        private void UpdateUIPosition(FrameworkElement element, double locX, double locY)
        {
            double posX = ConvertLocX(locX) - element.Width / 2.0;
            double posY = ConvertLocY(locY) - element.Height / 2.0;
            Canvas.SetTop(element, posY);
            Canvas.SetLeft(element, posX);
        }

        protected double ConvertLocX(double locX)
        {
            return locX / (_arena.Parameters["ArenaSize"] / _mainWindow.BattlefieldCanvas.Width);
        }

        protected double ConvertLocY(double locY)
        {
            return locY / (_arena.Parameters["ArenaSize"] / _mainWindow.BattlefieldCanvas.Height);
        }

        private FrameworkElement CreateExplosionUIElement()
        {
            double width = 80 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            double height = 80 * _mainWindow.BattlefieldCanvas.Height / _arena.Parameters["ArenaSize"];
            Grid grid = new Grid
            {
                Width = width,
                Height = height,
                Visibility = Visibility.Hidden
            };
            double width5 = 10 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            double height5 = 10 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            Ellipse ellipse5 = new Ellipse
            {
                Width = width5,
                Height = height5,
                Fill = Explosion5Brush,
                Visibility = Visibility.Visible,
            };
            double width20 = 40 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            double height20 = 40 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            Ellipse ellipse20 = new Ellipse
            {
                Width = width20,
                Height = height20,
                Fill = Explosion20Brush,
                Visibility = Visibility.Visible,
            };
            double width40 = 80 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            double height40 = 80 * _mainWindow.BattlefieldCanvas.Width / _arena.Parameters["ArenaSize"];
            Ellipse ellipse40 = new Ellipse
            {
                Width = width40,
                Height = height40,
                Fill = Explosion40Brush,
                Visibility = Visibility.Visible,
            };
            grid.Children.Add(ellipse40);
            grid.Children.Add(ellipse20);
            grid.Children.Add(ellipse5);
            return grid;
        }

        private Grid CreateBackgroundGrid()
        {
            // Create a grid with lines every 100m
            int lineCount = _arena.Parameters["ArenaSize"] / 100;
            double cellWidth = _mainWindow.BattlefieldCanvas.Width / lineCount;
            double cellHeight = _mainWindow.BattlefieldCanvas.Height / lineCount;
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
                    Thickness thickness = new Thickness(0, 0, x == lineCount-1 ? 0 : 1, y == lineCount-1 ? 0 : 1);
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
