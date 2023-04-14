using System;

namespace Table
{
    [Flags]
    [Serializable]
    public enum UnitLabel
    {
        None = 0,
        LightArmour = 0x0000_0001,
        HeavyArmour = 0x0000_0002,
        HeavyUnit = 0x0000_0004,
        Bio = 0x0000_0010,
        Mechanical = 0x0000_0020,
        Building = 0x0000_0040,
        Hero = 0x0000_0080,
        PsionicPower = 0x0000_0100,
        Investigation = 0x0000_0200,
        Untransportable = 0x0000_0400,
        TimeLimit = 0x0000_0800,
        MapBoss = 0x0000_1000,
        LoneWolf = 0x0000_2000,
        CannotOperated = 0x0000_4000,
        FlightTarget = 0x0001_0000,
        GroundTarget = 0x0002_0000,
    }
}