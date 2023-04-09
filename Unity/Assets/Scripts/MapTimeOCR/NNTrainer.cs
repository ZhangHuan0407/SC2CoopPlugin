#if UNITY_EDITOR
using Game;
using LittleNN;
using OCRProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
                    float[] buffer = new float[MapTimeParameter.Size];
                    string hashName = CRC32.ComputeString($"{Path.GetFileNameWithoutExtension(textureFile)}_{i}").CRC32Str;
                    MapTime.ConvertToNNFormat(recognizeAreaRect, subAreaRect, grayBytes, buffer);
                    using (Bitmap grayBitmap = new Bitmap(MapTimeParameter.Width, MapTimeParameter.Height))
                    {
                        for (int index = 0; index < buffer.Length; index++)
                        {
                            int y = index / MapTimeParameter.Width;
                            int x = index % MapTimeParameter.Width;
                            byte value = (byte)(buffer[index] * 255f);
                            grayBitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
                        }
                        grayBitmap.Save($"{SampleDirectoryPath}/{hashName}.png", ImageFormat.Png);
                    }
                    byte[] sampleArray = new byte[MapTimeParameter.Size];
                    for (int point = 0; point < sampleArray.Length; point++)
                        sampleArray[point] = (byte)(buffer[point] * 255f);
                    File.WriteAllBytes($"{SampleDirectoryPath}/{hashName}.bin", sampleArray);
                    //for (int symbolValue = 0; symbolValue < 11; symbolValue++)
                    //{
                    //    string testFileName = $"{SampleDirectoryPath}/{symbolValue}/{hashName}.bin";
                    //    if (File.Exists(testFileName))
                    //        File.WriteAllBytes(testFileName, sampleArray);
                    //}
                }
            }
            UnityEngine.Debug.Log("SplitTextureToSample finish");
        }

        [MenuItem("Tools/NNTrainer/TrainNewModel")]
        public static void TrainNewModel()
        {
            List<TemplateData> allTemplates = new List<TemplateData>();
            foreach (string directoryPath in Directory.GetDirectories(SampleDirectoryPath))
            {
                string directoryName = new DirectoryInfo(directoryPath).Name;
                int symbolValue = int.Parse(directoryName);
                foreach (string filePath in Directory.GetFiles(directoryPath, "*.bin"))
                    allTemplates.Add(new TemplateData(symbolValue, File.ReadAllBytes(filePath)));
            }
            Task.Run(async () =>
            {
                NeuralNetwork neuralNetwork = new NeuralNetwork(MapTimeParameter.Size, new int[] { MapTimeParameter.Size }, 11);
                for (int i = 0; i < 2000; i++)
                {
                    float totalLoss = 0f;
                    for (int templateIndex = 0; templateIndex < allTemplates.Count; templateIndex++)
                    {
                        TemplateData templateData = allTemplates[templateIndex];
                        float[] input = new float[MapTimeParameter.Size];
                        for (int index = 0; index < templateData.Data.Length; index++)
                            input[index] = templateData.Data[index] / 255f;
                        float[] target = new float[11];
                        target[templateData.SymbolValue] = 1f;
                        float loss = neuralNetwork.Train(input, target);
                        totalLoss += loss;
                    }
                    if (i % 50 == 49)
                        UnityEngine.Debug.Log($"{i} loss {totalLoss / allTemplates.Count}");
                    await Task.Delay(1);
                }
            });
        }
    }
}
#endif