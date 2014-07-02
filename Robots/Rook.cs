using SDK;

namespace Robots
{
    // Rook scans the battlefield like a rook, i.e., only 0,90,180,270; move horizontally only, but looks horz and vertically
    public class Rook : Robot
    {
        int _course;
        int _boundary;
        int _damage;

        public override void Main()
        {
            if (SDK.LocY < 500)
            {
                SDK.Drive(90, 70);
                while (SDK.LocY - 500 < 20 && SDK.Speed > 0)
                    ;
            }
            else
            {
                SDK.Drive(270, 70);
                while (SDK.LocY - 500 > 20 && SDK.Speed > 0)
                    ;
            }
            SDK.Drive(0, 0);

            _damage = SDK.Damage;
            _course = 0;
            _boundary = 995;
            SDK.Drive(_course, 30);

            while (true)
            {

                Look(0);
                Look(90);
                Look(180);
                Look(270);

                if (_course == 0)
                {
                    if (SDK.LocX > _boundary || SDK.Speed == 0)
                        Change();
                }
                else
                {
                    if (SDK.LocX < _boundary || SDK.Speed == 0)
                        Change();
                }
            }
        }

        private void Look(int deg)
        {
            int range;

            while ((range = SDK.Scan(deg, 2)) > 0 && range <= 700)
            {
                SDK.Drive(_course, 0);
                SDK.Cannon(deg, range);
                if (_damage + 20 != SDK.Damage)
                {
                    _damage = SDK.Damage;
                    Change();
                }
            }
        }


        private void Change()
        {
            if (_course == 0)
            {
                _boundary = 5;
                _course = 180;
            }
            else
            {
                _boundary = 995;
                _course = 0;
            }
            SDK.Drive(_course, 30);
        }
    }
}
