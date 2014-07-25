using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Arena;
using CSharpRobotsWPF.ViewModels;

namespace CSharpRobotsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WPFArena _wpfArena;
        public OptionsViewModel OptionsViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            OptionsViewModel = new OptionsViewModel();
            OptionsView.DataContext = OptionsViewModel;

            _wpfArena = new WPFArena(this);

            OptionsViewModel.ShowTraces = _wpfArena.ShowTraces;
            OptionsViewModel.ShowMissileTarget = _wpfArena.ShowMissileTarget;
            OptionsViewModel.ShowMissileExplosion = _wpfArena.ShowMissileExplosion;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Options
            _wpfArena.ShowTraces = OptionsViewModel.ShowTraces;
            _wpfArena.ShowMissileTarget = OptionsViewModel.ShowMissileTarget;
            _wpfArena.ShowMissileExplosion = OptionsViewModel.ShowMissileExplosion;
            // Team/Mode
            if (OptionsViewModel.RobotInfos == null || OptionsViewModel.RobotInfos.Count(x => x.IsSelected) == 0)
                _wpfArena.StartStop();
            else
            {
                List<Type> selectedRobots = OptionsViewModel.RobotInfos.Where(x => x.IsSelected).Select(x => x.Type).ToList();
                switch(OptionsViewModel.Mode)
                {
                    case ArenaModes.Solo:
                        _wpfArena.StartSolo(selectedRobots[0]);
                        break;
                    case ArenaModes.Single:
                        if (selectedRobots.Count >= 2)
                            _wpfArena.StartSingle(selectedRobots[0], selectedRobots[1]);
                        break;
                        case ArenaModes.Single4:
                        if (selectedRobots.Count >= 4)
                            _wpfArena.StartSingle4(selectedRobots[0], selectedRobots[1], selectedRobots[2], selectedRobots[3]);
                        break;
                    case ArenaModes.Double:
                        if (selectedRobots.Count >= 2)
                            _wpfArena.StartDouble(selectedRobots[0], selectedRobots[1]);
                        break;
                    case ArenaModes.Double4:
                        if (selectedRobots.Count >= 4)
                            _wpfArena.StartDouble4(selectedRobots[0], selectedRobots[1], selectedRobots[2], selectedRobots[3]);
                        break;
                    case ArenaModes.Team:
                        if (selectedRobots.Count >= 4)
                            _wpfArena.StartTeam(selectedRobots[0], selectedRobots[1], selectedRobots[2], selectedRobots[3]);
                        break;
                    case ArenaModes.Free:
                        break;
                }
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsViewModel.RefreshRobotList();
            OptionsViewModel.IsVisible = true;
        }
    }
}
