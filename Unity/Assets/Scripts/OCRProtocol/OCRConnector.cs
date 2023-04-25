using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Game;
using UnityEngine;

namespace Game.OCR
{
    public class OCRConnector : IDisposable
    {
        public static OCRConnector Instance;

        private TcpClient m_TcpClient;
        public bool Connected => m_TcpClient?.Connected ?? false;
        private NetworkStream m_NetworkStream;
        private static volatile int m_SequenceId;
        public volatile bool NeedSyncParameters;

        public OCRConnector()
        {
            NeedSyncParameters = true;
        }

        public void ConnectServiceSync(int port)
        {
            
            m_TcpClient = new TcpClient(new IPEndPoint(IPAddress.Loopback, 0));
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    m_TcpClient.Connect(new IPEndPoint(IPAddress.Loopback, port));
                }
                catch (Exception e)
                {
                    LogService.Error(nameof(ConnectServiceSync), e);
                }
                if (m_TcpClient.Client.Connected)
                    break;
                else
                    Thread.Sleep(3000);
            }

            if (!m_TcpClient.Client.Connected)
                throw new Exception("Can not connect to Ocr process.");
            m_TcpClient.SendTimeout = 300;
            m_NetworkStream = m_TcpClient.GetStream();
        }

        public Task<(HeadData, TResponse)> SendRequestAsync<TResponse>(ProtocolId protocolId, ValueType package) where TResponse : struct
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Process starCraftProcess = Global.StarCraftProcess;
                if (starCraftProcess == null || starCraftProcess.HasExited)
                    return (new HeadData() { ProtocolId = protocolId, StatusCode = ErrorCode.StarCraftProcessHasExited }, default);
                if (!m_TcpClient.Client.Connected)
                    return (new HeadData() { ProtocolId = protocolId, StatusCode = ErrorCode.OcrNotInit }, default);
                if (protocolId == ProtocolId.RecognizeWindowArea && NeedSyncParameters)
                {
                    InitParameters_Request request = new InitParameters_Request(Global.UserSetting.InGameLanguage, 
                                                                                screenSize: new RectAnchor(0, 0, GameDefined.ScreenWidth, GameDefined.ScreenHeight));
                    (HeadData head, InitParameters_Response package) response = SendRequest_Internal<InitParameters_Response>(ProtocolId.InitParameters, request);
                    NeedSyncParameters = response.head.StatusCode == ErrorCode.OK && response.package.OcrInitSuccess;
                    if (NeedSyncParameters)
                    {
                        HeadData head = response.head;
                        head.ProtocolId = protocolId;
                        return (head, default);
                    }
                }
                return SendRequest_Internal<TResponse>(protocolId, package);
            });
        }
        private (HeadData, TResponse) SendRequest_Internal<TResponse>(ProtocolId protocolId, ValueType package) where TResponse : struct
        {
            lock (this)
            {
                int sequenceId = ++m_SequenceId;
                try
                {
                    byte[] headBuffer = new byte[HeadData.Size];
                    // send request
                    {
                        byte[] packageBytes = JSONMap.ToJSON(package).ToBytes();
                        HeadData head = new HeadData()
                        {
                            PackageLength = packageBytes.Length,
                            ProtocolId = protocolId,
                            SequenceId = sequenceId,
                        };
                        WriteStructToBytes(head, headBuffer, 0, HeadData.Size);
                        m_NetworkStream.Write(headBuffer, 0, HeadData.Size);
                        m_NetworkStream.Write(packageBytes, 0, head.PackageLength);
                    }
                    // get response
                    {
                        int point = 0;
                        do
                        {
                            Thread.Sleep(1);
                            int readHeadBytesCount = m_NetworkStream.Read(headBuffer, point, HeadData.Size - point);
                            point += readHeadBytesCount;
                        } while (point != HeadData.Size);
                        HeadData responseHeadData = ReadStructFromBytes<HeadData>(headBuffer, 0, HeadData.Size);
                        point = 0;
                        byte[] responseBytes = new byte[responseHeadData.PackageLength];
                        while (true)
                        {
                            int readPackageBytesCount = m_NetworkStream.Read(responseBytes, point, responseHeadData.PackageLength - point);
                            point += readPackageBytesCount;
                            if (point != responseHeadData.PackageLength)
                            {
                                Thread.Sleep(1);
                                continue;
                            }
                            return (responseHeadData, JSONMap.ParseJSON<TResponse>(responseBytes.ToJSONObject()));
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.Error("OCRConnector.SendRequest_Internal", e);
                    HeadData head = new HeadData()
                    {
                        PackageLength = 0,
                        ProtocolId = protocolId,
                        SequenceId = sequenceId,
                        StatusCode = ErrorCode.UnhandledClientException,
                    };
                    return (head, default);
                }
            }
        }

        public void Dispose()
        {
            Debug.Log("OCRConnector Dispose");
            if (m_TcpClient != null && m_TcpClient.Connected)
            {
                Task task = Task.Run(() =>
                {
                    SendRequest_Internal<ProcessExit_Response>(ProtocolId.ProcessExit, new ProcessExit_Request());
                });
                Task.WaitAll(new Task[] { task }, 1200);
                m_TcpClient.Dispose();
                m_TcpClient = null;
            }
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
    }
}