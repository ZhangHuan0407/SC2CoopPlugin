using UnityEngine;

namespace MapTimeOCR
{
    internal class MapTimeParameter
    {
        public const int Width = 12;
        public const int Height = 16;
        public const int Size = Width * Height;
        public const int RectCountLimit = 10;
        public static readonly string NNModelFileName = Application.streamingAssetsPath + "/" + "WindowsOCR/MapTimeNNModel.bin";
    }
}