namespace Common.Components.Drivers
{
    public class NullDriver : Driver
    {
        protected override void UpdateDriver()
        {
            Turn(0);
        }
    }
}