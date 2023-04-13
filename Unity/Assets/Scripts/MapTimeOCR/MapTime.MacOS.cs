#if UNITY_STANDALONE_OSX
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using LittleNN;

namespace Game.OCR
{
    public class MapTime : IDisposable
    {
        //public Bitmap ScreenShot;
        public readonly NeuralNetwork NNModel;

        public MapTime(NeuralNetwork model)
        {
            NNModel = model;
        }

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
            Debug.Log("MapTime Dispose");
            //ScreenShot?.Dispose();
            throw new NotImplementedException();
        }
    }
}
#endif