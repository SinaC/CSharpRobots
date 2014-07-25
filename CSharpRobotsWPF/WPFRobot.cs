using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CSharpRobotsWPF.MVVM;

namespace CSharpRobotsWPF
{
    public class WPFRobot : ObservableObject
    {
        public FrameworkElement RobotUIElement { get; set; }
        public TextBlock LabelUIElement { get; set; }
        public Polyline TraceUIElement { get; set; }

        public DateTime LastTrace { get; set; }

        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set { Set(() => Color, ref _color, value); }
        }

        private bool _isAlive;
        public bool IsAlive
        {
            get { return _isAlive; }
            set { Set(() => IsAlive, ref _isAlive, value); }
        }

        private int _id;
        public int Id
        {
            get { return _id; }
            set { Set(() => Id, ref _id, value); }
        }

        private int _team;
        public int Team
        {
            get { return _team; }
            set { Set(() => Team, ref _team, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        private int _damage;
        public int Damage
        {
            get { return _damage; }
            set { Set(() => Damage, ref _damage, value); }
        }

        private int _locX;
        public int LocX
        {
            get { return _locX; }
            set { Set(() => LocX, ref _locX, value); }
        }

        private int _locY;
        public int LocY
        {
            get { return _locY; }
            set { Set(() => LocY, ref _locY, value); }
        }

        private int _heading;
        public int Heading
        {
            get { return _heading; }
            set { Set(() => Heading, ref _heading, value); }
        }

        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set { Set(() => Speed, ref _speed, value); }

        }

        private string _state;
        public string State
        {
            get { return _state; }
            set { Set(() => State, ref _state, value); }
        }

        private int _cannonCount;
        public int CannonCount
        {
            get { return _cannonCount; }
            set { Set(() => CannonCount, ref _cannonCount, value); }
        }

        private IReadOnlyDictionary<string, int> _statistics;
        public IReadOnlyDictionary<string, int> Statistics
        {
            get { return _statistics; }
            set
            {
                Set(() => Statistics, ref _statistics, value);
            }
        }
    }
}
