#if UNITY_EDITOR

namespace Game.OCR
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