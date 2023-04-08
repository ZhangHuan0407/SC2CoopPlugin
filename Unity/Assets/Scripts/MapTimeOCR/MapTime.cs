using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OCRProtocol;
using LittleNN;

namespace MapTimeOCR
{
    public class MapTime
    {
        private static byte[] m_BytesCache;
        private static object m_Lock = new object();
        private static byte[] RentBytesCache(int dataLength)
        {
            lock (m_Lock)
            {
                if (m_BytesCache is null || m_BytesCache.Length < dataLength)
                    return new byte[dataLength];
                byte[] result = m_BytesCache;
                m_BytesCache = null;
                return result;
            }
        }
        private static void RevertBytesCache(byte[] bytes)
        {
            m_BytesCache = bytes;
        }

        public static Rectangle UpTo4(Rectangle recognizeRect)
        {
            recognizeRect.Width = (recognizeRect.Width * 24 + 31) / 32;
            return recognizeRect;
        }
        public static MapTimeParseResult TryParse(Bitmap bitmap, NeuralNetwork neuralNetwork, Rectangle recognizeAreaRect,
                                                  out int seconds)
        {
            seconds = -1;
            List<RectAnchor> subAreaRectList = SplitSubArea(bitmap, ref recognizeAreaRect, out byte[] grayBytes);
            if (subAreaRectList.Count < 3 || subAreaRectList.Count >= MapTimeSymbol.RectCountLimit)
                return MapTimeParseResult.RectCountError;

            subAreaRectList.Sort((l, r) => l.Left.CompareTo(r.Left));
            List<int> numberSymbols = ParseSymbol(neuralNetwork, recognizeAreaRect, subAreaRectList, grayBytes);
            RevertBytesCache(grayBytes);

            if (numberSymbols.Count < 3)
                return MapTimeParseResult.ParseNumberCountError;

            int baseValue = 1;
            seconds = 0;
            // 愚蠢的人类时间进制，你都全球通用60进制了，为啥不把基础的数字运算也改成60?
            for (int i = numberSymbols.Count - 1; i >= 0; i -= 2)
            {
                int numberSymbolA = numberSymbols[i];
                int numberSymbolB = 0;
                if (i - 1 >= 0)
                {
                    numberSymbolB = numberSymbols[i - 1];
                    if (numberSymbolB >= 7)
                        return MapTimeParseResult.NumberInvalid;
                }
                seconds += baseValue * (numberSymbolA + numberSymbolB * 10);
                baseValue *= 60;
            }
            return MapTimeParseResult.WellDone;
        }

        public static List<RectAnchor> SplitSubArea(Bitmap bitmap, ref Rectangle recognizeAreaRect, out byte[] grayBytes)
        {
            recognizeAreaRect = UpTo4(recognizeAreaRect);
            Rectangle recognizeRect = recognizeAreaRect;
            BitmapData bitmapData = bitmap.LockBits(recognizeRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int dataLength = recognizeRect.Height * recognizeRect.Width * 3;
            byte[] grayMap = RentBytesCache(dataLength);
            grayBytes = grayMap;
            Marshal.Copy(bitmapData.Scan0, grayMap, 0, dataLength);
            bitmap.UnlockBits(bitmapData);

            // convert rgb to gray map, and get lowRange, highRange
            long totalSum = 0L;
            byte lowValue, hightValue;
            {
                int point = 0;
                int area = recognizeRect.Width * recognizeRect.Height;
                for (int y = 0; y < recognizeRect.Height; y++)
                {
                    for (int x = 0; x < recognizeRect.Width; x++)
                    {
                        byte q = grayMap[point];
                        byte w = grayMap[point + 1];
                        byte e = grayMap[point + 2];
                        byte average = (byte)((q + w + e) / 3);
                        grayMap[point] = average;
                        totalSum += average;
                        point += 3;
                    }
                }
                lowValue = (byte)(0.33f * totalSum / area);
                if (lowValue > 40)
                    lowValue = 40;
                hightValue = (byte)(0.5f * totalSum / area + 127.5f);
            }

            // pick high light area
            List<RectAnchor> subAreaRectList = new List<RectAnchor>();
            //System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            bool[,] havePicked = new bool[recognizeRect.Height, recognizeRect.Width];
            {
                for (int y = 0; y < recognizeRect.Height; y++)
                {
                    for (int x = 0; x < recognizeRect.Width; x++)
                    {
                        int point = (y * recognizeRect.Width + x) * 3;
                        //stringBuilder.Append(grayMap[point].ToString().PadLeft(5));
                        if (!havePicked[y, x] && grayMap[point] > hightValue)
                        {
                            //int stirngBuilderLength = stringBuilder.Length;
                            RectAnchor rect = new RectAnchor(x, y, 0, 0);
                            Pick(x, y, ref rect);
                            if (rect.Width > 0 && rect.Width < MapTimeSymbol.Width &&
                                rect.Height > 0 && rect.Height < MapTimeSymbol.Height)
                            {
                                rect.Right++;
                                rect.Bottom++;
                                subAreaRectList.Add(rect);
                                //stringBuilder.AppendLine(rect.ToString());
                            }
                            //else
                            //    stringBuilder.Length = stirngBuilderLength;
                            if (subAreaRectList.Count >= MapTimeSymbol.RectCountLimit)
                                return subAreaRectList;
                        }
                    }
                    //stringBuilder.AppendLine();
                }
                //File.WriteAllText("test.txt", stringBuilder.ToString());
            }
            // 如果是低亮度，继续扩展相邻十字
            // 如果是高亮度，继续扩展包围8格
            void Pick(int pointX, int pointY, ref RectAnchor subAreaRect)
            {
                int grayPoint = (pointY * recognizeRect.Width + pointX) * 3;
                if (pointX < 0 || pointY < 0 || pointX >= recognizeRect.Width || pointY >= recognizeRect.Height)
                    return;
                else if (havePicked[pointY, pointX] || grayMap[grayPoint] < lowValue)
                    return;
                if (pointX < subAreaRect.Left)
                    subAreaRect.Left = pointX;
                if (pointX > subAreaRect.Right)
                    subAreaRect.Right = pointX;
                if (pointY < subAreaRect.Top)
                    subAreaRect.Top = pointY;
                if (pointY > subAreaRect.Bottom)
                    subAreaRect.Bottom = pointY;
                havePicked[pointY, pointX] = true;
                Pick(pointX - 1, pointY, ref subAreaRect);
                Pick(pointX + 1, pointY, ref subAreaRect);
                Pick(pointX, pointY - 1, ref subAreaRect);
                Pick(pointX, pointY + 1, ref subAreaRect);
                if (grayMap[grayPoint] > hightValue)
                {
                    Pick(pointX - 1, pointY - 1, ref subAreaRect);
                    Pick(pointX - 1, pointY + 1, ref subAreaRect);
                    Pick(pointX + 1, pointY - 1, ref subAreaRect);
                    Pick(pointX + 1, pointY - 1, ref subAreaRect);
                }
            }
            return subAreaRectList;
        }

        public static List<int> ParseSymbol(NeuralNetwork neuralNetwork, Rectangle recognizeAreaRect, List<RectAnchor> subAreaRectList, byte[] grayBytes)
        {
            List<int> numberSymbols = new List<int>();
            float[] input = new float[MapTimeSymbol.Size];
            for (int i = 0; i < subAreaRectList.Count; i++)
            {
                ConvertToNNFormat(recognizeAreaRect, subAreaRectList[i], grayBytes, input);
                float[] eval = neuralNetwork.Forward(input);
                float maxValue = -1f;
                int maxIndex = 10;
                for (int j = 0; j < eval.Length; j++)
                    if (eval[j] > maxValue)
                    {
                        maxValue = eval[j];
                        maxIndex = j;
                    }
                if (maxIndex != 10 && maxValue > 0.7f)
                    numberSymbols.Add(maxIndex);
            }
            return numberSymbols;
        }
        public static void ConvertToNNFormat(Rectangle recognizeAreaRect, RectAnchor subAreaRect, byte[] grayBytes, float[] buffer)
        {
            int offsetX = (MapTimeSymbol.Width - subAreaRect.Width) / 2;
            int offsetY = (MapTimeSymbol.Height - subAreaRect.Height) / 2;
            if (offsetX > 1)
            {
                subAreaRect.Left -= 2;
                subAreaRect.Width += 4;
                if (subAreaRect.Left < 0)
                    subAreaRect.Left = 0;
                if (subAreaRect.Right > recognizeAreaRect.Right)
                    subAreaRect.Right = recognizeAreaRect.Right;
                offsetX = (MapTimeSymbol.Width - subAreaRect.Width) / 2;
            }
            if (offsetY > 1)
            {
                subAreaRect.Top -= 2;
                subAreaRect.Bottom += 4;
                if (subAreaRect.Top < 0)
                    subAreaRect.Top = 0;
                if (subAreaRect.Bottom > recognizeAreaRect.Bottom)
                    subAreaRect.Bottom = recognizeAreaRect.Bottom;
                offsetY = (MapTimeSymbol.Height - subAreaRect.Height) / 2;
            }

            for (int y = 0; y < subAreaRect.Height; y++)
            {
                for (int x = 0; x < subAreaRect.Width; x++)
                {
                    int pixelIndex = (subAreaRect.Top + y) * recognizeAreaRect.Width + (subAreaRect.Left + x);
                    byte grayValue = grayBytes[pixelIndex * 3];
                    int index = (offsetY + y) * MapTimeSymbol.Width + (offsetX + x);
                    buffer[index] = grayValue / 255.99f;
                }
            }
        }
    }
}