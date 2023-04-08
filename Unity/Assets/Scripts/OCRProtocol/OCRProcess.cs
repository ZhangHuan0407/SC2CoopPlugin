using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Game;

namespace OCRProtocol
{
    public static class OCRProcess
    {
        private static int GetRandomUnusedPort()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            HashSet<int> usedPort = new HashSet<int>();
            foreach (IPEndPoint endPoint in properties.GetActiveTcpListeners())
                usedPort.Add(endPoint.Port);
            Random random = new Random();
            int i = 0;
            while (true)
            {
                if (i++ > 500)
                    throw new Exception("Can not get unused tcp port.");
                int port = random.Next(1001, 60000);
                if (usedPort.Contains(port))
                    continue;
                else
                    return port;
            }
        }

        public const string WinOCRFileName = "WindowsOCR/WindowsOCR.exe";
        public const string MacOSOCRFileName = "MacOSOCR/MacOSOCR.exe";
        public static async Task StartNewOCRProcessAsync()
        {
            if (OCRConnector.Instance != null)
                throw new Exception($"have another OCRConnector instance");
            await Task.Delay(1);
            int port = GetRandomUnusedPort();
            int currentProcessId;
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                currentProcessId = currentProcess.Id;
            }
            string fileName;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                fileName = WinOCRFileName;
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                fileName = MacOSOCRFileName;
            else
                throw new NotImplementedException();
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = $"-port={port} -mainprocess={currentProcessId}",
#if !UNITY_EDITOR && !ALPHA
                CreateNoWindow = true,
#endif
            };
            using (Process p = Process.Start(processStartInfo))
            {
                LogService.System("OCRProcess", $"start ocr process {fileName} {processStartInfo.Arguments}");
            }

            OCRConnector connector = new OCRConnector();
            connector.ConnectServiceSync(port);

            Ping_Request request = new Ping_Request(new Random().Next());
            (HeadData, Ping_Response) response = await connector.SendRequestAsync<Ping_Response>(ProtocolId.Ping, request);
            if (OCRConnector.Instance != null)
            {
                connector.Dispose();
                throw new Exception($"have another OCRConnector instance");
            }
            OCRConnector.Instance = connector;
        }
    }
}