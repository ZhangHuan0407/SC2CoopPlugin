using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OCRProtocol;

namespace WindowsOCR
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            JSONMap.CallFromLoadingThread();
            JSONMap.RegisterDefaultType();
            JSONMap.RegisterAllTypes();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            int port = 0;
            Process mainProcess = null;
            bool unitTest = false;
            foreach (string argument in args)
            {
                string[] inputs = argument.Split('=');
                switch (inputs[0].ToLowerInvariant().TrimStart('-'))
                {
                    case "port":
                        port = int.Parse(inputs[1].ToLowerInvariant());
                        break;
                    case "mainprocess":
                        mainProcess = Process.GetProcessById(int.Parse(inputs[1].ToLowerInvariant()));
                        break;
                    case "unittest":
                        unitTest = true;
                        break;
                    default:
                        break;
                }
            }
            if (unitTest)
            {
                Console.WriteLine($"enable unit test");
                new OCRAdapter().CommandLineTest();
                return;
            }

            // todo kill last WindowsOCR process

            ServiceConnector.Instance = new ServiceConnector()
            {
                OCRAdapter = new OCRAdapter(),
            };
            ServiceConnector.Instance.StartRecieveSync(port);

            // 主线程进入摸鱼状态
            while (!mainProcess.HasExited &&
                ServiceConnector.Instance.Connected)
            {
                Thread.Sleep(5000);
#if DEBUG
                Console.WriteLine($"MainProcess HasExited: {mainProcess.HasExited}, Connected: {ServiceConnector.Instance.Connected}");
#endif
            }
            ServiceConnector.Instance.Dispose();
        }
    }
}
