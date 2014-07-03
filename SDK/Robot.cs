namespace SDK
{
    public abstract class Robot
    {
        public ISDKRobot SDK { get; set; }

        public abstract void Main();
    }
}
