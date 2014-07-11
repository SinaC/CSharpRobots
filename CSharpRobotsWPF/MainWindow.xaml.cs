using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common;
using Microsoft.Win32;

namespace CSharpRobotsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private List<Type> _robotTypes;
        private readonly WPFArena _wpfArena;
        private List<RobotOptionEntry> _tempEntries;
        public List<RobotOptionEntry> RobotOptionEntries;

        public MainWindow()
        {
            ExecuteOnUIThread.Initialize();

            Log.Initialize("TODO", "TODO", "TODO");

            InitializeComponent();

            _wpfArena = new WPFArena(this);

            OptionsView.OkButton.Click += OptionsOkButtonOnClick;
            OptionsView.CancelButton.Click += OptionsCancelButtonOnClick;
        }

        private void OptionsCancelButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            OptionsView.Visibility = Visibility.Hidden;
        }

        private void OptionsOkButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            OptionsView.Visibility = Visibility.Hidden;
            RobotOptionEntries = _tempEntries;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string radioTag = OptionsView.RadioButtonsStackPanel.Children.Cast<RadioButton>().Where(x => x.IsChecked.HasValue && x.IsChecked.Value).Select(x => x.Tag as String).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(radioTag) || RobotOptionEntries == null || RobotOptionEntries.Count == 0)
                _wpfArena.StartStop();
            else
            {
                List<Type> selectedRobots = RobotOptionEntries.Where(x => x.IsSelected).Select(x => x.Type).ToList();
                switch(radioTag)
                {
                        // Single
                    case "1":
                        if (selectedRobots.Count >= 2)
                            _wpfArena.StartSingle(selectedRobots[0], selectedRobots[1]);
                        break;
                        // Double
                    case "2":
                        if (selectedRobots.Count >= 2)
                            _wpfArena.StartDouble(selectedRobots[0], selectedRobots[1]);
                        break;
                        // Team
                    case "3":
                        if (selectedRobots.Count >= 4)
                            _wpfArena.StartTeam(selectedRobots[0], selectedRobots[1], selectedRobots[2], selectedRobots[3]);
                        break;
                        // Solo
                    case "4":
                        if (selectedRobots.Count >= 1)
                            _wpfArena.StartSolo(selectedRobots[0]);
                        break;
                }
            }

            //    void InitializeSolo(Type robotType, int locX, int locY, int heading, int speed);
        //void InitializeSingleMatch(Type team1, Type team2);
        //void InitializeSingleMatch(Type team1, Type team2, int locX1, int locY1, int locX2, int locY2);
        //void InitializeDoubleMatch(Type team1, Type team2);
        //void InitializeTeamMatch(Type team1, Type team2, Type team3, Type team4);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog dlg = new OpenFileDialog
            //    {
            //        DefaultExt = "*.dll",
            //        Filter = "DLL files (*.dll)|*.dll",
            //        Multiselect = true
            //    };
            //bool? result = dlg.ShowDialog();
            //if (result.Value)
            //{
            //    List<Type> robotTypes = new List<Type>();
            //    foreach(string s in dlg.FileNames)
            //        robotTypes.AddRange(LoadRobots.LoadRobotsFromFile(s));
            //}
            string path = ConfigurationManager.AppSettings["RobotsPath"];
            if (String.IsNullOrWhiteSpace(path))
                path = AppDomain.CurrentDomain.BaseDirectory;
            if (!String.IsNullOrWhiteSpace(path))
            {
                List<Type> robots = LoadRobots.LoadRobotsFromPath(path);
                if (robots.Count > 0)
                {
                    _tempEntries = robots.Select(x => new RobotOptionEntry
                        {
                            Type = x,
                            IsSelected = RobotOptionEntries != null && (RobotOptionEntries.FirstOrDefault(y => y.Name == x.Name) ?? RobotOptionEntry.NullObject).IsSelected,
                            Name = x.Name,
                        }).ToList();

                    OptionsView.RobotList.DataContext = _tempEntries;
                    OptionsView.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
