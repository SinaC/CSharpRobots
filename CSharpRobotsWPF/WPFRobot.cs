using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace CSharpRobotsWPF
{
    public class WPFRobot : INotifyPropertyChanged
    {
        public FrameworkElement UIElement { get; set; }

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

        public string _state;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
