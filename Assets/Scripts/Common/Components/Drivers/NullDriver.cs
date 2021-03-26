namespace Common.Components.Drivers
{
    public class NullDriver : Driver
    {
        protected override void UpdateDriver() { }

        protected override void FixedUpdateDriver()
        {
            Turn(0);
        }
    }
}