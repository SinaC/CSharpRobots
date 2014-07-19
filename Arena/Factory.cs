﻿namespace Arena
{
    public class Factory
    {
        public static IReadonlyArena CreateArena()
        {
            return new Internal.Arena();
        }

        public static IReadonlyArena CreateDoublePrecisionArena()
        {
            return new Internal.DoublePrecisionArena();
        }
    }
}
