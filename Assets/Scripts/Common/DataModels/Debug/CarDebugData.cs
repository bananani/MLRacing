namespace Common.DataModels.Debug
{
    public struct CarDebugData
    {
        // Velocity
        public VelocityDebugData Velocity { get; }

        // Engine
        public EngineDebugData Engine { get; }

        // Tyres
        public TyreDebugData LeftFrontTyre { get; }
        public TyreDebugData RightFrontTyre { get; }
        public TyreDebugData LeftRearTyre { get; }
        public TyreDebugData RightRearTyre { get; }

        // Aero
        public AeroDebugData Aero { get; }


        public CarDebugData(
            VelocityDebugData velocity,
            EngineDebugData engine,
            TyreDebugData leftFrontTyre,
            TyreDebugData rightFrontTyre,
            TyreDebugData leftRearTyre,
            TyreDebugData rightRearTyre,
            AeroDebugData aero) : this()
        {
            Velocity = velocity;
            Engine = engine;
            LeftFrontTyre = leftFrontTyre;
            RightFrontTyre = rightFrontTyre;
            LeftRearTyre = leftRearTyre;
            RightRearTyre = rightRearTyre;
            Aero = aero;
        }
    }
}