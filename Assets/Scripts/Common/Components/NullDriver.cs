namespace Common.Components
{
    public class NullDriver : Driver
    {
        protected override void UpdateDriver()
        {
            Turn(0);
        }
    }
}