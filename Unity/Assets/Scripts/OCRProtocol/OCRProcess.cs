using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Game;

namespace Game.OCR
{
    public static class OCRProcess
    {
#if UNITY_EDITOR
        public const string OCRPricessPath = "../WindowsOCR/bin/Debug/net5.0/WindowsOCR.exe";
#elif UNITY_STANDALONE_WIN
        public const string OCRPricessPath = "WindowsOCR/WindowsOCR.exe";
#else
        public const string OCRPricessPath = "";
#endif

        public static async Task StartNewOCRProcessAsync()
        {
            // 非windows没有需要启动的进程，临时处理下
#if !UNITY_STANDALONE_WIN
            await Task.Delay(1);
            return;
#endif
            if (OCRConnectorA.Instance != null)
                throw new Exception($"have another OCRConnector instance");
            await Task.Delay(1);

            OCRConnectorA connector = new OCRConnectorA();
            OCRConnectorA.Instance = connector;
            _ = Task.Run(async () =>
            {
                connector.AcceptTCPSync();
                Ping_Request request = new Ping_Request(new Random().Next());
                (HeadData, Ping_Response) response = await connector.SendRequestAsync<Ping_Response>(ProtocolId.Ping, request);
                if (OCRConnectorA.Instance != null)
                {
                    connector.Dispose();
                    throw new Exception($"have another OCRConnector instance");
                }
            });

            int currentProcessId;
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                currentProcessId = currentProcess.Id;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = new FileInfo(OCRPricessPath).FullName,
                Arguments = $"-port={connector.Port} -mainprocess={currentProcessId}",
#if !UNITY_EDITOR && !ALPHA
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
            };
            using (Process process = Process.Start(processStartInfo))
            {
                LogService.System("OCRProcess", $"start ocr process {OCRPricessPath} {processStartInfo.Arguments} id {process.Id}");
            }
        }
    }
}