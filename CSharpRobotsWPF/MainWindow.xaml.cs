using System.Windows;
using Common;

namespace CSharpRobotsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private List<Type> _robotTypes;
        private readonly WPFArena _wpfArena;

        public MainWindow()
        {
            ExecuteOnUIThread.Initialize();

            Log.Initialize("TODO", "TODO", "TODO");

            InitializeComponent();

            //_robotTypes = LoadRobots.LoadRobotsFromPath(@"D:\GitHub\CSharpRobots\Robots\bin\Debug\");
            _wpfArena = new WPFArena(this);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _wpfArena.StartStop();
        }
    }
}
