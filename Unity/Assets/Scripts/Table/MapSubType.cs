using System;

namespace Table
{
    [Serializable]
    public enum MapSubType
    {
        Unknown,
        A = 0x01,
        B = 0x02,
        AorB = A + B,
    }
}