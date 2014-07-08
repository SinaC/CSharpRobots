namespace SDK
{
    public abstract class Robot
    {
        /// <summary>
        /// Robot SDK entry point
        /// </summary>
        public ISDKRobot SDK { get; set; }

        //public abstract void Main();

        /// <summary>
        /// Called when robot is created before starting match
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Called periodically, robot behaviour goes here (if this method execution time is too high, robot will be stopped and match suspended)
        /// </summary>
        public abstract void Step();
    }
}
