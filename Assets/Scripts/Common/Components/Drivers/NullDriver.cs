namespace Common.Components.Drivers
{
    public class NullDriver : Driver
    {
        protected override void DriverUpdate() { }

        protected override void DriverFixedUpdate()
        {
            Handbrake(1);
            Brake(1);
            Accelerate(0);
            Turn(0);
        }
    }
}