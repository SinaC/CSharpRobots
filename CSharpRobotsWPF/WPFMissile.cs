using System.Windows;

namespace CSharpRobotsWPF
{
    public class WPFMissile
    {
        public FrameworkElement FlyingUIElement { get; set; }
        public FrameworkElement TargetUIElement { get; set; }
        public FrameworkElement ExplosionUIElement { get; set; }

        public int Id { get; set; }
    }
}
