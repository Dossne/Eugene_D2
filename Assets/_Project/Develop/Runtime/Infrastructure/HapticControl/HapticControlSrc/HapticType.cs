namespace Infrastructure.HapticControl
{
    public enum HapticType : short
    {
        //Lofelt.NiceVibrations presets: -1 =>999. Do not add here
        None = -1,
        Selection = 0,
        Success = 1,
        Warning = 2,
        Failure = 3,
        LightImpact = 4,
        MediumImpact = 5,
        HeavyImpact = 6,
        RigidImpact = 7,
        SoftImpact = 8,

        //Custom => 1000-32767. Use prefix "C"
        CExtraLight = 1000, //TODO this is example. Can use it
        CExtraPower = 1001, //TODO this is example. Can use it
        CBzBzClip = 1002,   //TODO this is example. Can use it
    }
}