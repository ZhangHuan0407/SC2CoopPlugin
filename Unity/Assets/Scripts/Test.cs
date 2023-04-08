using Game;
using LittleNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


public class Test : MonoBehaviour
{
    public Text Label;
    public Image image;
    private int i;
    public void OnClick()
    {
        if (i++ % 2 == 0)
        {
            RectTransform rectTrans = (image.transform as RectTransform);
            rectTrans.sizeDelta /= 2;
        }
        else
        {
            RectTransform rectTrans = (image.transform as RectTransform);
            rectTrans.sizeDelta *= 2;
        }
        if (i == 3)
        {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(1920, 1080))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(1920, 1080));
                }
                bitmap.Save("nn.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

    public void OnClick2()
    {
        try
        {
            NeuralNetwork neuralNetwork = new NeuralNetwork(2, new int[] { 2 }, 1);
            for (int i = 0; i < 100; i++)
            {
                neuralNetwork.Train(new float[] { 1f, 0f, }, new float[] { 1f });
                neuralNetwork.Train(new float[] { 1f, 1f, }, new float[] { 0f });
            }
            neuralNetwork.SaveTo("test.bin");
            NeuralNetwork.LoadFrom("test.bin");
        }
        catch (System.Exception e)
        {
            Label.text = e.ToString();
            LogService.Error("OnClick2", e);
        }
    }

    public object obj;
    public void OnClick3()
    {
        try
        {
            Assembly assembly = Assembly.LoadFile(Path.Combine("WindowsOCR", "WindowsOCR.dll"));
            System.Type type = assembly.GetType("WindowsOCR.OCRAdapter");
            MethodInfo methodInfo = type.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
            if (methodInfo is null)
                throw new NullReferenceException(nameof(methodInfo));
            obj = methodInfo.Invoke(null, Array.Empty<object>());
            MethodInfo methodInfoB = type.GetMethod("Recognize", BindingFlags.Static | BindingFlags.Instance);
            if (methodInfoB is null)
                throw new NullReferenceException(nameof(methodInfoB));
            //void Recognize(string[] taskList, out string[][] results)
            object[] parameters = new object[2];
            // "X:(?<X>[0-9]+),Y:(?<Y>[0-9]+),Width:(?<Width>[0-9]+),Height(?<Height>[0-9]+)"
            parameters[0] = "X:0,Y:0,Width:1920,Height:1080";
            methodInfoB.Invoke(obj, parameters);
            File.WriteAllLines("output.txt", (parameters[1] as string[][])[0]);
        }
        catch (System.Exception e)
        {
            Label.text = e.ToString();
            LogService.Error("OnClick3", e);
        }

    }
}
