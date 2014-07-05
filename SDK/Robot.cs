namespace SDK
{
    public abstract class Robot
    {
        public ISDKRobot SDK { get; set; }

        public abstract string Name { get; }

        public abstract void Main();
    }
}
