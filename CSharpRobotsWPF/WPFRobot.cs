using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSharpRobotsWPF
{
    public class WPFRobot : INotifyPropertyChanged
    {
        public FrameworkElement RobotUIElement { get; set; }
        public TextBlock LabelUIElement { get; set; }

        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isAlive;
        public bool IsAlive
        {
            get { return _isAlive; }
            set
            {
                if (_isAlive != value)
                {
                    _isAlive = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _team;
        public int Team
        {
            get { return _team; }
            set
            {
                if (_team != value)
                {
                    _team = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _damage;
        public int Damage
        {
            get { return _damage; }
            set
            {
                if (_damage != value)
                {
                    _damage = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _locX;
        public int LocX
        {
            get { return _locX; }
            set
            {
                if (_locX != value)
                {
                    _locX = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _locY;
        public int LocY
        {
            get { return _locY; }
            set
            {
                if (_locY != value)
                {
                    _locY = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _heading;
        public int Heading
        {
            get { return _heading; }
            set
            {
                if (_heading != value)
                {
                    _heading = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _state;
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _cannonCount;
        public int CannonCount
        {
            get { return _cannonCount; }
            set
            {
                if (_cannonCount != value)
                {
                    _cannonCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyDictionary<string, int> _statistics;
        public IReadOnlyDictionary<string, int> Statistics
        {
            get { return _statistics; }
            set
            {
                if (_statistics != value)
                {
                    _statistics = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
