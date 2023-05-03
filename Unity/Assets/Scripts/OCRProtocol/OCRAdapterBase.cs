using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.OCR
{
    public abstract class OCRAdapterBase : IDisposable
    {
        protected bool HaveInited { get; set; }
        protected abstract string DefaultLanguageTag { get; set; }

        protected event Action AfterResponse_Handle;

        public OCRAdapterBase()
        {
            HaveInited = false;
        }

        public virtual void BindListener(in Dictionary<ProtocolId, AdapterAction> processingHandle)
        {
            processingHandle[ProtocolId.AvailableRecognizerLanguages] = AvailableRecognizerLanguages;
            processingHandle[ProtocolId.InitParameters] = InitParameters;
            processingHandle[ProtocolId.Ping] = Ping;
            processingHandle[ProtocolId.ProcessExit] = ProcessExit;
            processingHandle[ProtocolId.RecognizeWindowArea] = RecognizeWindowArea;
        }

        protected abstract ProtocolResponse AvailableRecognizerLanguages(ProtocolRequest request);
        protected abstract ProtocolResponse InitParameters(ProtocolRequest request);
        private ProtocolResponse Ping(ProtocolRequest request)
        {
            int random = JSONMap.ParseJSON<Ping_Request>(request.Package).Random;
            Ping_Response response = new Ping_Response() { PlusOne = random + 1 };
            return new ProtocolResponse(ErrorCode.OK, JSONMap.ToJSON(response));
        }
        private ProtocolResponse ProcessExit(ProtocolRequest request)
        {
            AfterResponse_Handle += () => Process.GetCurrentProcess().Kill();
            return new ProtocolResponse(ErrorCode.OK, JSONMap.ToJSON(new ProcessExit_Response()));
        }
        protected abstract ProtocolResponse RecognizeWindowArea(ProtocolRequest request);

        //protected bool GameProcessIsNotExists()
        //{
        //    if (StarCraftProcessId <= 0)
        //        return false;
        //    Process gameProcess = null;
        //    try
        //    {
        //        // if process is not exists, it will throw exception.
        //        gameProcess = Process.GetProcessById(StarCraftProcessId);
        //        return gameProcess.HasExited;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        if (gameProcess != null)
        //        {
        //            gameProcess.Dispose();
        //        }
        //    }
        //}

        public void AfterResponse()
        {
            AfterResponse_Handle?.Invoke();
            AfterResponse_Handle = null;
        }

#if !UNITY_STANDALONE
        public void CommandLineTest()
        {
            Dictionary<ProtocolId, AdapterAction> processingHandle = new Dictionary<ProtocolId, AdapterAction>();
            BindListener(in processingHandle);
            System.Drawing.Size screenSize = WindowsOCR.MonitorHelper.GetResolution();
            JSONObject package = JSONMap.ToJSON(new InitParameters_Request(DefaultLanguageTag,
                                                                           new RectAnchor(0, 0, screenSize.Width, screenSize.Height)));
            processingHandle[ProtocolId.InitParameters](new ProtocolRequest(default, package));
            Console.WriteLine(new RectAnchor(272, 786, 100, 52).ToString());
            while (true)
            {
                Console.WriteLine("wait input:");
                string input = Console.ReadLine();
                bool parse = RectAnchor.TryParse(input, out RectAnchor rectAnchor);
                if (!parse)
                    continue;
                RecognizeWindowArea_Request.Task[] tasks = new RecognizeWindowArea_Request.Task[]
                {
                    new RecognizeWindowArea_Request.Task()
                    {
                        Tag = "Debug",
                        RectAnchor = rectAnchor,
                    }
                };
                package = JSONMap.ToJSON(new RecognizeWindowArea_Request(tasks, debug: true));
                ProtocolResponse response = processingHandle[ProtocolId.RecognizeWindowArea](new ProtocolRequest(default, package));
                Console.WriteLine("response:");
                Console.WriteLine(response.Package.ToString(true));
            }
        }
#endif

        public abstract void Dispose();
    }
}