﻿namespace SDK
{
    public interface ISDKCheat
    {
        void FindNearestEnemy(out double degrees, out double range, out double x, out double y);
        void FireAt(double targetX, double targetY);
        void Teleport(double locX, double locY);
        void SetDamage(int damage);
    }
}
