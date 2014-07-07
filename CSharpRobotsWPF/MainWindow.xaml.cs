using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Arena;

namespace CSharpRobotsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        private readonly IReadonlyArena _arena;

        private List<WPFRobot> _wpfRobots;
        private List<WPFMissile> _wpfMissiles;

        //private List<Type> _robotTypes;

        public MainWindow()
        {
            ExecuteOnUIThread.Initialize();

            InitializeComponent();

            _arena = Factory.CreateArena();
            _arena.ArenaStarted += OnArenaStarted;
            _arena.ArenaStopped += OnArenaStopped;
            _arena.ArenaStep += OnArenaStep;

            //_robotTypes = LoadRobots.LoadRobotsFromPath(@"D:\GitHub\CSharpRobots\Robots\bin\Debug\");
        }

        private void OnArenaStep(IReadonlyArena arena)
        {
            ExecuteOnUIThread.Invoke(Refresh);
        }

        private void OnArenaStopped(IReadonlyArena arena)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                StartButton.Content = "Start";
                UpdateStatus();
            });
        }

        private void OnArenaStarted(IReadonlyArena arena)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                StartButton.Content = "Stop";
                UpdateStatus();
            });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_arena == null)
                return;
            if (_arena.State == ArenaStates.Running)
                _arena.StopMatch();
            else
            {
                // Initialize robots
                //_arena.InitializeSolo(typeof(CrazyCannon), 500, 500, 0, 0);
                //_arena.InitializeSingleMatch(typeof (SinaC), typeof (Stinger));
                //_arena.InitializeSingleMatch(typeof(Robots.Phalanx), typeof(Robots.Stinger));
                //_arena.InitializeSingleMatch(typeof(Robots.Rook), typeof(Robots.Rabbit));
                //_arena.InitializeSingleMatch(_robotTypes.FirstOrDefault(x => x.Name.Contains("Phalanx")), _robotTypes.FirstOrDefault(x => x.Name.Contains("SinaC")));
                _arena.InitializeDoubleMatch(typeof(Robots.SinaC), typeof(Robots.Stinger));

                if (_arena.State == ArenaStates.Error)
                    StatusText.Text = "Error while creating match";
                else
                {
                    // Create WPF robots
                    BattlefieldCanvas.Children.Clear();
                    _wpfMissiles = new List<WPFMissile>();
                    _wpfRobots = new List<WPFRobot>();
                    foreach (IReadonlyRobot robot in _arena.Robots)
                        CreateRobot(robot);

                    //
                    RobotInformationsList.DataContext = _wpfRobots;

                    // Start match
                    _arena.StartMatch();
                }
            }
        }

        private void Refresh()
        {
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

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            switch (_arena.State)
            {
                case ArenaStates.Running:
                    StatusText.Text = "Running";
                    break;
                case ArenaStates.Winner:
                    StatusText.Text = String.Format("And the winner is Team {0}", _arena.WinningTeam);
                    break;
                case ArenaStates.NoWinner:
                    StatusText.Text = "Draw - No winner";
                    break;
                case ArenaStates.Stopped:
                    StatusText.Text = "Stopped";
                    break;
                default:
                    StatusText.Text = String.Format("State : {0}", _arena.State);
                    break;
            }
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
                    wpfMissile.TargetUIElement.Visibility = Visibility.Visible;
                    wpfMissile.ExplosionUIElement.Visibility = Visibility.Hidden;
                }
                else if (missile.State == MissileStates.Exploding || missile.State == MissileStates.Exploded)
                {
                    wpfMissile.FlyingUIElement.Visibility = Visibility.Hidden;
                    wpfMissile.TargetUIElement.Visibility = Visibility.Hidden;
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
                            Width = 80*BattlefieldCanvas.Width/1000.0,
                            Height = 80*BattlefieldCanvas.Height/1000.0,
                            Stroke = TargetBrush,
                            Visibility = Visibility.Hidden
                        },
                    ExplosionUIElement = CreateExplosionUIElement()
                };
            BattlefieldCanvas.Children.Add(wpfMissile.FlyingUIElement);
            BattlefieldCanvas.Children.Add(wpfMissile.TargetUIElement);
            BattlefieldCanvas.Children.Add(wpfMissile.ExplosionUIElement);
            UpdateMissile(wpfMissile, missile);
            // Set target
            double destX, destY;
            Common.Helpers.Math.ComputePoint(missile.LaunchLocX, missile.LaunchLocY, missile.Range, missile.Heading, out destX, out destY);
            UpdateUIPosition(wpfMissile.TargetUIElement, destX, destY);
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

            UpdateUIPosition(wpfRobot.UIElement, robot.LocX, robot.LocY);
        }

        private void DeleteRobot(WPFRobot wpfRobot)
        {
            wpfRobot.UIElement.Visibility = Visibility.Hidden;
        }

        private void CreateRobot(IReadonlyRobot robot)
        {
            WPFRobot wpfRobot = new WPFRobot
                {
                    Id = robot.Id,
                    Team = robot.Team,
                    Name = robot.Name,
                    Color = TeamBrushes[robot.Team],
                    UIElement = new Rectangle
                        {
                            Width = 4,
                            Height = 4,
                            Fill = TeamBrushes[robot.Team],
                        }
                };
            Panel.SetZIndex(wpfRobot.UIElement, 100);
            BattlefieldCanvas.Children.Add(wpfRobot.UIElement);
            UpdateRobot(wpfRobot, robot);
            _wpfRobots.Add(wpfRobot);
        }

        private void UpdateUIPosition(FrameworkElement element, double locX, double locY)
        {
            double posX = locX / (1000.0 / BattlefieldCanvas.Width) - element.Width/2.0;
            double posY = locY / (1000.0 / BattlefieldCanvas.Height) - element.Height/2.0;
            Canvas.SetTop(element, posX);
            Canvas.SetLeft(element, posY);
        }

        private FrameworkElement CreateExplosionUIElement()
        {
            //Ellipse ellipse = new Ellipse
            //{
            //    Width = 80*BattlefieldCanvas.Width/1000.0,
            //    Height = 80*BattlefieldCanvas.Height/1000.0,
            //    Fill = Explosion5Brush,
            //    Visibility = Visibility.Hidden
            //};
            //return ellipse;
            double width = 80 * BattlefieldCanvas.Width / 1000.0;
            double height = 80 * BattlefieldCanvas.Height / 1000.0;
            Grid grid = new Grid
            {
                Width = width,
                Height = height,
                Visibility = Visibility.Hidden
            };
            double width5 = 10 * BattlefieldCanvas.Width / 1000.0;
            double height5 = 10 * BattlefieldCanvas.Width / 1000.0;
            Ellipse ellipse5 = new Ellipse
            {
                Width = width5,
                Height = height5,
                Fill = Explosion5Brush,
                Visibility = Visibility.Visible,
            };
            double width20 = 40 * BattlefieldCanvas.Width / 1000.0;
            double height20 = 40 * BattlefieldCanvas.Width / 1000.0;
            Ellipse ellipse20 = new Ellipse
            {
                Width = width20,
                Height = height20,
                Fill = Explosion20Brush,
                Visibility = Visibility.Visible,
            };
            double width40 = 80 * BattlefieldCanvas.Width / 1000.0;
            double height40 = 80 * BattlefieldCanvas.Width / 1000.0;
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
    }
}
