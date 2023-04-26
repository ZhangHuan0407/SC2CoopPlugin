using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Game.OCR
{
    public class OCRConnectorB : IDisposable
    {
        public static OCRConnectorB Instance;

        private TcpClient m_TcpClient;
        private NetworkStream m_NetworkStream;
        public bool Connected => m_TcpClient.Connected;
        private Stopwatch m_ProcessingStopwatch;
        public OCRAdapterBase OCRAdapter { get; set; }
        internal Dictionary<ProtocolId, AdapterAction> Processing_Handle;

        private Thread m_Thread;

        public OCRConnectorB()
        {
            Processing_Handle = new Dictionary<ProtocolId, AdapterAction>();
            m_ProcessingStopwatch = new Stopwatch();
        }

        public void ConnectServiceSync(int port)
        {
            m_TcpClient = new TcpClient(new IPEndPoint(IPAddress.Loopback, 0));
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    m_TcpClient.Connect(new IPEndPoint(IPAddress.Loopback, port));
                }
                catch (Exception e)
                {
                    Console.WriteLine(nameof(ConnectServiceSync) + e);
                }
                if (m_TcpClient.Client.Connected)
                    break;
                else
                    Thread.Sleep(2000);
            }

            m_TcpClient.SendTimeout = 300;
            m_NetworkStream = m_TcpClient.GetStream();
            OCRAdapter.BindListener(Processing_Handle);

            m_Thread = new Thread(Circle);
            m_Thread.Start();
        }

        private void Circle()
        {
            byte[] buffer = new byte[2048];
            int point = 0;
            while (m_TcpClient.Connected)
            {
                try
                {
                    int readHeadBytesCount = m_NetworkStream.Read(buffer, point, HeadData.Size - point);
                    point += readHeadBytesCount;
                    if (readHeadBytesCount == 0)
                        Thread.Sleep(20);
                    if (point != HeadData.Size)
                        continue;
                    HeadData headData = ReadStructFromBytes<HeadData>(buffer, 0, HeadData.Size);
                    WriteLog($"accept request {headData}");
                    m_ProcessingStopwatch.Restart();
                    point = 0;
                    while (true)
                    {
                        WriteLog($"read package {point} / {headData.PackageLength}");
                        if (point != headData.PackageLength)
                        {
                            int readPackageBytesCount = m_NetworkStream.Read(buffer, point, headData.PackageLength - point);
                            point += readPackageBytesCount;
                            Thread.Sleep(2);
                            continue;
                        }
                        point = 0;
                        byte[] packageBytes = new byte[headData.PackageLength];
                        Array.Copy(buffer, packageBytes, headData.PackageLength);
                        ProcessingRequest(headData, packageBytes, 
                                          out HeadData responseHead, out byte[] packageResponse);
                        WriteStructToBytes(responseHead, buffer, 0, HeadData.Size);
                        m_NetworkStream.Write(buffer, 0, HeadData.Size);
                        m_NetworkStream.Write(packageResponse, 0, packageResponse.Length);
                        m_ProcessingStopwatch.Stop();
                        WriteLog($"reply head {responseHead}, Processing: {m_ProcessingStopwatch.ElapsedMilliseconds}ms");
                        // kill self process if necessary
                        OCRAdapter.AfterResponse();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void ProcessingRequest(HeadData requestHead, byte[] requestPackage,
                                      out HeadData responseHead, out byte[] responsePackage)
        {
            ProtocolResponse response;
            try
            {
                ProtocolRequest request = new ProtocolRequest(requestHead, requestPackage.ToJSONObject());
                response = Processing_Handle[request.Head.ProtocolId].Invoke(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                response = ProtocolResponse.Error(ErrorCode.UnhandledServerException);
            }
            responsePackage = response.Package.ToBytes();
            responseHead = new HeadData()
            {
                PackageLength = responsePackage.Length,
                ProtocolId = requestHead.ProtocolId,
                SequenceId = requestHead.SequenceId,
                StatusCode = response.StatusCode,
            };
        }

        public void Dispose()
        {
            m_TcpClient?.Dispose();
            m_Thread?.Join();
            OCRAdapter?.Dispose();
        }

        public static T ReadStructFromBytes<T>(byte[] bytes, int point, int size) where T : struct
        {
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, point, buffer, size);
            T t = Marshal.PtrToStructure<T>(buffer);
            Marshal.FreeHGlobal(buffer);
            return t;
        }
        public static void WriteStructToBytes<T>(T t, byte[] bytes, int point, int size) where T : struct
        {
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(t, buffer, false);
            Marshal.Copy(buffer, bytes, point, size);
            Marshal.FreeHGlobal(buffer);
        }

        public static void WriteLog(string log)
        {
#if DEBUG
            Console.WriteLine(log);
#endif
        }
    }
}