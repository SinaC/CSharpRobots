namespace SDK
{
    public abstract class RobotStepByStep
    {
        public ISDKRobot SDK { get; set; }

        public abstract void Step(); // called periodically
    }
}
