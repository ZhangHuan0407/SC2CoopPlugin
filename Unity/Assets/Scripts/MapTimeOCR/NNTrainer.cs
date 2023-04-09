#if UNITY_EDITOR
using Game;
using OCRProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace MapTimeOCR
{
    /// <summary>
    /// 使用预制截图，将数据转换为byte[] 并手动分类
    /// 再转换为float[]，训练一个能分类的神经网络
    /// </summary>
    public class NNTrainer
    {
        public const string TextureDirectoryPath = "../InMapTime/Texture";
        public const string SampleDirectoryPath = "../InMapTime/NNSample";
        public static readonly Rectangle MapTimeArea = new Rectangle(264, 770, 80, 36);

        [MenuItem("Tools/NNTrainer/SplitTextureToSample")]
        public static void SplitTextureToSample()
        {
            string[] textureFiles = Directory.GetFiles(TextureDirectoryPath, "*.png", SearchOption.TopDirectoryOnly);
            Directory.CreateDirectory(SampleDirectoryPath);
            for (int i = 0; i < 11; i++)
                Directory.CreateDirectory($"{SampleDirectoryPath}/{i}");

            using (Bitmap bitmap = Bitmap.FromFile(textureFiles[0]) as Bitmap)
            {
                using (Bitmap test = new Bitmap(MapTimeArea.Width, MapTimeArea.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(test))
                    {
                        graphics.DrawImage(bitmap, 0, 0, srcRect: MapTimeArea, GraphicsUnit.Pixel);
                    }
                    test.Save($"{SampleDirectoryPath}/test.png", ImageFormat.Png);
                }
            }

            foreach (string textureFile in textureFiles)
            {
                Rectangle recognizeAreaRect = MapTimeArea;
                List<RectAnchor> subAreaRectList;
                byte[] grayBytes;
                using (Bitmap bitmap = Bitmap.FromFile(textureFile) as Bitmap)
                {
                    bool isMSK = textureFile.Contains("蒙斯克");
                    subAreaRectList = MapTime.SplitSubArea(bitmap, isMSK, ref recognizeAreaRect, out grayBytes);
                }
                for (int i = 0; i < subAreaRectList.Count; i++)
                {
                    RectAnchor subAreaRect = subAreaRectList[i];
                    float[] buffer = new float[MapTimeSymbol.Size];
                    string hashName = CRC32.ComputeString($"{Path.GetFileNameWithoutExtension(textureFile)}_{i}").CRC32Str;
                    MapTime.ConvertToNNFormat(recognizeAreaRect, subAreaRect, grayBytes, buffer);
                    //StringBuilder stringBuilder = new StringBuilder();
                    //for (int y = 0; y < subAreaRect.Height; y++)
                    //{
                    //    for (int x = 0; x < subAreaRect.Width; x++)
                    //    {
                    //        int index = ((y + subAreaRect.Top) * recognizeAreaRect.Width + subAreaRect.Left + x) * 3;
                    //        stringBuilder.Append(grayBytes[index].ToString().PadLeft(5));
                    //    }
                    //    stringBuilder.AppendLine();
                    //}
                    //UnityEngine.Debug.Log(stringBuilder);
                    using (Bitmap grayBitmap = new Bitmap(MapTimeSymbol.Width, MapTimeSymbol.Height))
                    {
                        for (int index = 0; index < buffer.Length; index++)
                        {
                            int y = index / MapTimeSymbol.Width;
                            int x = index % MapTimeSymbol.Width;
                            byte value = (byte)(buffer[index] * 255f);
                            grayBitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
                        }
                        grayBitmap.Save($"{SampleDirectoryPath}/{hashName}.png", ImageFormat.Png);
                    }
                    byte[] sampleArray = new byte[MapTimeSymbol.Size];
                    for (int point = 0; point < sampleArray.Length; point++)
                        sampleArray[point] = (byte)(buffer[point] * 256f);
                    File.WriteAllBytes($"{SampleDirectoryPath}/{hashName}.bin", sampleArray);
                }
            }
            UnityEngine.Debug.Log("SplitTextureToSample finish");
        }
    }
}
#endif