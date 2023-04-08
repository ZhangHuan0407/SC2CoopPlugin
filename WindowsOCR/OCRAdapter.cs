using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using OCRProtocol;
using System.Threading.Tasks;

namespace WindowsOCR
{
#pragma warning disable CA1416
    /// <summary>
    /// 解析协议内容，并提供OCR服务的类
    /// 线程不安全，协议必须按照收到顺序执行
    /// </summary>
    public class OCRAdapter : OCRAdapterBase
    {
        protected override string DefaultLanguageTag { get; set; } = "zh-Hans-CN";

        private OcrEngine m_OcrEngine;
        private MemoryStream m_MemoryStream;
        private Bitmap m_ScreenBitmap;
        private Graphics m_ScreenGraphics;

        public OCRAdapter()
        {
            m_MemoryStream = new MemoryStream(600 * 400 * 4);
        }

        protected override ProtocolResponse AvailableRecognizerLanguages(ProtocolRequest request)
        {
            AvailableRecognizerLanguages_Response response = new AvailableRecognizerLanguages_Response()
            {
                Languages = new List<AvailableRecognizerLanguages_Response.LanguageItem>(),
            };
            foreach (Language language in OcrEngine.AvailableRecognizerLanguages)
            {
                AvailableRecognizerLanguages_Response.LanguageItem item = new AvailableRecognizerLanguages_Response.LanguageItem()
                {
                    DisplayName = language.DisplayName,
                    LanguageTag = language.LanguageTag,
                    NativeName = language.NativeName,
                };
                response.Languages.Add(item);
            }
            return new ProtocolResponse(ErrorCode.OK, JSONMap.ToJSON(response));
        }
        protected override ProtocolResponse InitParameters(ProtocolRequest request)
        {
            InitParameters_Request initParametersRequest = JSONMap.ParseJSON<InitParameters_Request>(request.Package);
            InitParameters_Response response = new InitParameters_Response();
            {
                if (m_OcrEngine == null || initParametersRequest.LanguageTag != m_OcrEngine.RecognizerLanguage.LanguageTag)
                {
                    if (string.IsNullOrEmpty(initParametersRequest.LanguageTag))
                        m_OcrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                    else
                        m_OcrEngine = OcrEngine.TryCreateFromLanguage(new Language(initParametersRequest.LanguageTag));
                }
                response.OcrInitSuccess = m_OcrEngine != null;
            }

            m_ScreenGraphics?.Dispose();
            m_ScreenBitmap?.Dispose();
            m_ScreenBitmap = new Bitmap(initParametersRequest.ScreenSize.Width, initParametersRequest.ScreenSize.Height);
            m_ScreenGraphics = Graphics.FromImage(m_ScreenBitmap);

            HaveInited = response.OcrInitSuccess;
            return new ProtocolResponse(HaveInited ? ErrorCode.OK : ErrorCode.ArgumentError, JSONMap.ToJSON(response));
        }

        internal class StdSubAreaSize
        {
            public const int Width = 12;
            public const int Height = 16;
            public const int Size = Width * Height;
        }
        private struct FuckGDIRect
        {
            public int Left;
            public int Right;
            public int Bottom;
            public int Top;

            public int Width => Right - Left;
            public int Height => Bottom - Top;

            public FuckGDIRect(int left, int top, int right, int bottom)
            {
                Left = left;
                Right = right;
                Bottom = bottom;
                Top = top;
            }
        }
        protected override ProtocolResponse RecognizeWindowArea(ProtocolRequest request)
        {
            if (!HaveInited)
                return ProtocolResponse.Error(ErrorCode.OcrNotInit);

            RecognizeWindowArea_Request recognizeRequest = JSONMap.ParseJSON<RecognizeWindowArea_Request>(request.Package);
            m_ScreenGraphics.CopyFromScreen(Point.Empty, Point.Empty, m_ScreenBitmap.Size);
            if (recognizeRequest.Debug)
                m_ScreenBitmap.Save("FullBitmap.png", ImageFormat.Png);
            for (int i = 0; i < recognizeRequest.TaskList.Length; i++)
            {
                RecognizeWindowArea_Request.Task task = recognizeRequest.TaskList[i];
                if (task.RectAnchor.Left < 0 || task.RectAnchor.Top < 0 ||
                    task.RectAnchor.Width  < 1 || task.RectAnchor.Height < 1)
                    return ProtocolResponse.Error(ErrorCode.ArgumentError);
            }
            RecognizeWindowArea_Response.Result[] results = RecognizeWindowAreaAsync(recognizeRequest).GetAwaiter().GetResult();
            return new ProtocolResponse(ErrorCode.OK, JSONMap.ToJSON(new RecognizeWindowArea_Response(results)));
        }
        private async Task<RecognizeWindowArea_Response.Result[]> RecognizeWindowAreaAsync(RecognizeWindowArea_Request recognizeRequest)
        {
            RecognizeWindowArea_Response.Result[] results = new RecognizeWindowArea_Response.Result[recognizeRequest.TaskList.Length];
            for (int i = 0; i < recognizeRequest.TaskList.Length; i++)
            {
                RectAnchor rectAnchor = recognizeRequest.TaskList[i].RectAnchor;
                Rectangle recognizeRect = new Rectangle(rectAnchor.Left, rectAnchor.Top, rectAnchor.Width, rectAnchor.Height);
                CopySubBitmapToStream(recognizeRect);
                var getDecoder = BitmapDecoder.CreateAsync(m_MemoryStream.AsRandomAccessStream());
                await getDecoder;
                BitmapDecoder decoder = getDecoder.GetResults();
                var getSoftBMP = decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await getSoftBMP;
                OcrResult ocrResult;
                using (SoftwareBitmap softwareBitmap = getSoftBMP.GetResults())
                {
                    var getRecognize = m_OcrEngine.RecognizeAsync(softwareBitmap);
                    await getRecognize;
                    ocrResult = getRecognize.GetResults();
                }
                results[i] = new RecognizeWindowArea_Response.Result()
                {
                    Tag = recognizeRequest.TaskList[i].Tag,
                    Contents = ocrResult.Lines.Select(i => i.Text.Replace(" ", string.Empty)).ToArray(),
                };
            }
            return results;
        }

        //private void CopyBitmapToStream()
        //{
        //    m_MemoryStream.Seek(0, SeekOrigin.Begin);
        //    m_ScreenBitmap.Save(m_MemoryStream, ImageFormat.Bmp);
        //    m_MemoryStream.SetLength(m_MemoryStream.Position);
        //    m_MemoryStream.Seek(0, SeekOrigin.Begin);
        //}
        private void CopySubBitmapToStream(Rectangle srcRegion)
        {
            m_MemoryStream.Seek(0, SeekOrigin.Begin);
            Rectangle destRegion = new Rectangle(0, 0, srcRegion.Width, srcRegion.Height);
            using (Bitmap destBitmap = new Bitmap(destRegion.Width, destRegion.Height))
            {
                using (Graphics destGraphics = Graphics.FromImage(destBitmap))
                {
                    destGraphics.DrawImage(m_ScreenBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
                }
                destBitmap.Save(m_MemoryStream, ImageFormat.Bmp);
#if DEBUG || ALPHA
                destBitmap.Save("SubBitmap.png", ImageFormat.Png);
#endif
            }
            m_MemoryStream.SetLength(m_MemoryStream.Position);
            m_MemoryStream.Seek(0, SeekOrigin.Begin);
        }

        public override void Dispose()
        {
            m_MemoryStream.Dispose();
            m_ScreenGraphics?.Dispose();
            m_ScreenGraphics = null;
            m_ScreenBitmap?.Dispose();
            m_ScreenBitmap = null;
        }
    }
#pragma warning restore CA1416
}