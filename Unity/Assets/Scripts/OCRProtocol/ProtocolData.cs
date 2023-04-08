using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OCRProtocol
{
    public enum ErrorCode : int
    {
        Unknown = 0,
        OK = 200,
        UnhandledClientException = 400,
        ArgumentError = 499,
        UnhandledServerException = 500,
        OcrNotInit = 501,
        StarCraftProcessHasExited = 900,
    }
    public enum ProtocolId : int
    {
        Error,
        AvailableRecognizerLanguages,
        InitParameters,
        Ping,
        ProcessExit,
        RecognizeWindowArea,
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = HeadData.Size)]
    public struct HeadData
    {
        public const int Size = 32;

        [FieldOffset(0)]
        public int PackageLength;
        [FieldOffset(4)]
        public ProtocolId ProtocolId;
        [FieldOffset(8)]
        public int SequenceId;
        [FieldOffset(12)]
        public ErrorCode StatusCode;

        public override string ToString() => $"{ProtocolId}, PackageLength: {PackageLength}, SequenceId: {SequenceId}, StatusCode: {StatusCode}";
    }

    public struct ProtocolRequest
    {
        public HeadData Head;
        public JSONObject Package;

        public ProtocolRequest(HeadData head, JSONObject package)
        {
            Head = head;
            Package = package;
        }
    }
    public struct ProtocolResponse
    {
        public ErrorCode StatusCode;
        public JSONObject Package;

        public ProtocolResponse(ErrorCode statusCode, JSONObject package)
        {
            StatusCode = statusCode;
            Package = package;
        }
        public static ProtocolResponse Error(ErrorCode errorCode) => new ProtocolResponse(errorCode, new JSONObject(JSONObject.Type.OBJECT));
    }
    public delegate ProtocolResponse AdapterAction(ProtocolRequest protocolData);

    // 获取当前 Ocr 可用语言，返回所有可用语言
    [Serializable]
    public struct AvailableRecognizerLanguages_Request
    {
    }
    [Serializable]
    public struct AvailableRecognizerLanguages_Response
    {
        [Serializable]
        public struct LanguageItem
        {
            public string DisplayName;
            public string LanguageTag;
            public string NativeName;
        }
        public List<LanguageItem> Languages;
    }

    [Serializable]
    public struct Ping_Request
    {
        public int Random;

        public Ping_Request(int random)
        {
            Random = random;
        }
    }
    [Serializable]
    public struct Ping_Response
    {
        public int PlusOne;
    }

    [Serializable]
    public struct ProcessExit_Request
    {

    }
    [Serializable]
    public struct ProcessExit_Response
    {
    }

    [Serializable]
    public struct InitParameters_Request
    {
        // Ocr使用此语言作为工作语言, string.Empty 指向 UserProfileLanguages
        public string LanguageTag;
        public RectAnchor ScreenSize;

        public InitParameters_Request(string languageTag, RectAnchor screenSize)
        {
            LanguageTag = languageTag;
            ScreenSize = screenSize;
        }
    }
    [Serializable]
    public struct InitParameters_Response
    {
        // Ocr 是否初始化成功
        public bool OcrInitSuccess;
    }

    [Serializable]
    public struct RecognizeWindowArea_Request
    {
        [Serializable]
        public struct Task
        {
            public string Tag;
            public RectAnchor RectAnchor;
        }

        public Task[] TaskList;
        public bool Debug;

        public RecognizeWindowArea_Request(Task[] taskList, bool debug = false)
        {
            TaskList = taskList;
            Debug = debug;
        }
    }
    [Serializable]
    public struct RecognizeWindowArea_Response
    {
        [Serializable]
        public struct Result
        {
            public string Tag;
            public string[] Contents;
        }

        public Result[] ResultList;

        public RecognizeWindowArea_Response(Result[] resultList)
        {
            ResultList = resultList;
        }
    }
}