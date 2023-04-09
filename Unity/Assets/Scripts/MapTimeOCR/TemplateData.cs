#if UNITY_EDITOR

namespace MapTimeOCR
{
    public struct TemplateData
    {
        public int SymbolValue;
        public byte[] Data;

        public TemplateData(int symbolValue, byte[] data)
        {
            SymbolValue = symbolValue;
            Data = data;
        }
    }
}
#endif