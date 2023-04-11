#if UNITY_STANDALONE_OSX
using System;
using System.Collections.Generic;
using System.Drawing;
using LittleNN;

namespace Game.OCR
{
    public class MapTime : IDisposable
    {
        public Bitmap ScreenShot;
        public NeuralNetworkModel NNModel;

        public MapTime() { }

        public void UpdateScreenShot()
        {
            throw new NotImplementedException("MacOS");
        }
        public MapTimeParseResult TryParse(bool isMSK, RectAnchor recognizeAreaRect, out int seconds)
        {
            throw new NotImplementedException("MacOS");
        }

        public void Dispose()
        {
            ScreenShot?.Dispose();
            throw new NotImplementedException();
        }
    }
}
#endif