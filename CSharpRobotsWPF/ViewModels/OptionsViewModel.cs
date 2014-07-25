using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using Arena;
using CSharpRobotsWPF.MVVM;

namespace CSharpRobotsWPF.ViewModels
{
    public class OptionsViewModel : ObservableObject
    {
        private List<string> _lastSelectedRobots = new List<string>();
        private bool _lastShowTraces;
        private bool _lastShowMissileExplosion;
        private bool _lastShowMissileTarget;
        private ArenaModes _lastMode;

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { Set(() => IsVisible, ref _isVisible, value); }
        }
        
        private bool _showTraces;
        public bool ShowTraces
        {
            get { return _showTraces; }
            set { Set(() => ShowTraces, ref _showTraces, value); }
        }

        private bool _showMissileExplosion;
        public bool ShowMissileExplosion
        {
            get { return _showMissileExplosion; }
            set { Set(() => ShowMissileExplosion, ref _showMissileExplosion, value); }
        }

        private bool _showMissileTarget;
        public bool ShowMissileTarget
        {
            get { return _showMissileTarget; }
            set { Set(() => ShowMissileTarget, ref _showMissileTarget, value); }
        }

        #region Mode radio buttons

        private ArenaModes _mode;
        public ArenaModes Mode
        {
            get { return _mode; }
            set { Set(() => Mode, ref _mode, value); }
        }

        public bool ArenaModeSolo
        {
            get { return Mode == ArenaModes.Solo; }
            set { Mode = value ? ArenaModes.Solo : Mode; }
        }

        public bool ArenaModeSingle
        {
            get { return Mode == ArenaModes.Single; }
            set { Mode = value ? ArenaModes.Single : Mode; }
        }

        public bool ArenaModeSingle4
        {
            get { return Mode == ArenaModes.Single4; }
            set { Mode = value ? ArenaModes.Single4 : Mode; }
        }

        public bool ArenaModeDouble
        {
            get { return Mode == ArenaModes.Double; }
            set { Mode = value ? ArenaModes.Double : Mode; }
        }

        public bool ArenaModeDouble4
        {
            get { return Mode == ArenaModes.Double4; }
            set { Mode = value ? ArenaModes.Double4 : Mode; }
        }

        public bool ArenaModeTeam
        {
            get { return Mode == ArenaModes.Team; }
            set { Mode = value ? ArenaModes.Team : Mode; }
        }

        public bool ArenaModeFree
        {
            get { return Mode == ArenaModes.Free; }
            set { Mode = value ? ArenaModes.Free : Mode; }
        }

        #endregion

        private List<RobotInfoItem> _robotInfos;
        public List<RobotInfoItem> RobotInfos
        {
            get { return _robotInfos; }
            set { Set(() => RobotInfos, ref _robotInfos, value); }
        }

        private ICommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                _okCommand = _okCommand ?? new RelayCommand(Ok);
                return _okCommand;
            }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                _cancelCommand = _cancelCommand ?? new RelayCommand(Cancel);
                return _cancelCommand;
            }
        }

        public void Ok()
        {
            // Save options
            _lastSelectedRobots = _robotInfos.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            _lastShowTraces = ShowTraces;
            _lastShowMissileExplosion = ShowMissileExplosion;
            _lastShowMissileTarget = ShowMissileTarget;
            _lastMode = Mode;
            
            IsVisible = false;
        }

        public void Cancel()
        {
            // Revert options
            foreach (RobotInfoItem item in RobotInfos)
                item.IsSelected = _lastSelectedRobots.FirstOrDefault(x => x == item.Name) != null;
            ShowTraces = _lastShowTraces;
            ShowMissileExplosion = _lastShowMissileExplosion;
            ShowMissileTarget = _lastShowMissileTarget;
            Mode = _lastMode;

            IsVisible = false;
        }

        public void RefreshRobotList()
        {
            string path = ConfigurationManager.AppSettings["RobotsPath"];
            if (String.IsNullOrWhiteSpace(path))
                path = AppDomain.CurrentDomain.BaseDirectory;
            if (!String.IsNullOrWhiteSpace(path))
            {
                List<Type> robots = LoadRobots.LoadRobotsFromPath(path);
                if (robots.Count > 0)
                {
                    RobotInfos = robots.Select(x => new RobotInfoItem
                    {
                        Type = x,
                        IsSelected = false, // TODO: retrieve previously selected value
                        Name = x.Name,
                    }).ToList();
                }
            }
        }
    }

    public class OptionsViewModelDesignData : OptionsViewModel
    {
        public OptionsViewModelDesignData()
        {
            RobotInfos = new List<RobotInfoItem>
                {
                    new RobotInfoItem
                        {
                            Name = "SinaC",
                            IsSelected = false,
                        },
                        new RobotInfoItem
                        {
                            Name = "Target",
                            IsSelected = true,
                        },
                        new RobotInfoItem
                        {
                            Name = "Surveyor",
                            IsSelected = false,
                        },
                        new RobotInfoItem
                        {
                            Name = "Rabbit",
                            IsSelected = true,
                        },
                };
            Mode = ArenaModes.Double;
            ShowMissileExplosion = true;
            ShowMissileTarget = true;
            ShowTraces = false;
        }
    }
}
