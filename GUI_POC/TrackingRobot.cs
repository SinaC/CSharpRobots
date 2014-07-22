using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GUI_POC
{
    public class TrackingRobot : RobotBase
    {
        private double _destinationX;
        private double _destinationY;

        public RobotBase Target { get; set; }

        public bool TrackTargetEnabled { get; set; }
        public bool MoveEnabled { get; set; }

        public Line LineToTarget { get; set; }
        public TextBlock LineLabel { get; set; }

        public TrackingRobot(Canvas battlefieldCanvas, int team, int id, double locX, double locY, double speed, double heading, bool trackTargetEnabled, bool moveEnabled)
            : base(battlefieldCanvas, team, id, locX, locY, speed, heading)
        {
            TrackTargetEnabled = trackTargetEnabled;
            MoveEnabled = moveEnabled;

            LineToTarget = new Line
            {
                // X1, Y1, X2, Y2 will be set by main loop
                Stroke = TeamBrushes[team],
                StrokeThickness = 1,
                Visibility = Visibility.Hidden
            };
            BattlefieldCanvas.Children.Add(LineToTarget);
        }

        public override void Init()
        {
            _destinationX = 100;
            _destinationY = 100;
        }

        public override void Step(double dt, List<RobotBase> robots)
        {
            if (TrackTargetEnabled)
                TrackTarget(robots);

            // Move
            if (MoveEnabled)
                Move();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            if (Target != null)
            {
                LineToTarget.X1 = ConvertLocX(LocX);
                LineToTarget.Y1 = ConvertLocY(LocY);
                LineToTarget.X2 = ConvertLocX(Target.LocX);
                LineToTarget.Y2 = ConvertLocY(Target.LocY);
                LineToTarget.Visibility = Visibility.Visible;
            }
            else
                LineToTarget.Visibility = Visibility.Hidden;
        }

        private void TrackTarget(List<RobotBase> robots)
        {
            Target = null;

            // Scan
            double bestTeamEnemyX = 0;
            double bestTeamEnemyY = 0;
            double bestTeamDistance = Double.MaxValue;
            int bestRobotInEnemyRange = 0;
            foreach (RobotBase enemy in robots.Where(x => x.Team != Team))
            {
                double enemyX = enemy.LocX;
                double enemyY = enemy.LocY;

                double distanceToEnemy = Distance(enemyX, enemyY, LocX, LocY);
                if (distanceToEnemy > MinRange && distanceToEnemy < MaxRange)
                {

                    // Compute distance from target to every alive team members
                    double totalDistance = 0;
                    int robotInEnemyRange = 0;
                    foreach (RobotBase friend in robots.Where(x => x.Team == Team))
                    {
                        double distance = Distance(enemyX, enemyY, friend.LocX, friend.LocY);

                        if (distance > MinRange && distance < MaxRange)
                        {
                            totalDistance += distance;
                            robotInEnemyRange++;
                        }
                    }

                    // It's better to have many robot on the same target than minimizing total distance
                    if (robotInEnemyRange > bestRobotInEnemyRange || ((robotInEnemyRange == bestRobotInEnemyRange) && totalDistance < bestTeamDistance))
                    {
                        bestTeamDistance = totalDistance;
                        bestTeamEnemyX = enemyX;
                        bestTeamEnemyY = enemyY;
                        bestRobotInEnemyRange = robotInEnemyRange;

                        Target = enemy;
                    }
                }
            }
        }

        private void Move()
        {
            double distanceToDestination = Distance(LocX, LocY, _destinationX, _destinationY);
            if (distanceToDestination < 20)
            {
                if (LocX <= 100 && LocY <= 100) // top left, go to top right
                {
                    _destinationX = ArenaSize - 100;
                    LocX = 105;
                    LocY = 100;
                }
                else if (LocX >= ArenaSize - 100 && LocY <= 100) // top right, go to bottom right
                {
                    _destinationY = ArenaSize - 100;
                    LocX = ArenaSize - 100;
                    LocY = 105;
                }
                else if (LocX >= ArenaSize - 100 && LocY >= ArenaSize - 100) // bottom right, go to bottom left
                {
                    _destinationX = 100;
                    LocX = ArenaSize - 100 - 5;
                    LocY = ArenaSize - 100;
                }
                else if (LocX <= 100 && LocY >= ArenaSize - 100) // bottom left, go to top left
                {
                    _destinationY = 100;
                    LocX = 100;
                    LocY = ArenaSize - 100 - 5;
                }
                double angle = Angle(LocX, LocY, _destinationX, _destinationY);
                Drive(angle, 100);
            }
            else
            {
                double angle = Angle(LocX, LocY, _destinationX, _destinationY);
                Drive(angle, 100);
            }
        }
    }
}
