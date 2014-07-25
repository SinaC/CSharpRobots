using System;
using CSharpRobotsWPF.MVVM;

namespace CSharpRobotsWPF.ViewModels
{
    public class RobotInfoItem : ObservableObject
    {
        public static RobotInfoItem NullObject = new RobotInfoItem
            {
                Type = null,
                IsSelected = false,
                Name = null
            };

        public Type Type { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(() => IsSelected, ref _isSelected, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

    }
}
