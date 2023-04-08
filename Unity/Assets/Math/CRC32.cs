/*
 * Copyright (c) 张欢 2360950578@qq.com
 */
using System;
using System.IO;
using System.Text;

namespace Game
{
    /// <summary>
    /// 对列举信息进行 CRC32/MPEG-2 运算并提供摘要信息
    /// 每个方法运算独立，此类型线程安全
    /// 返回结果为 <see cref="CRC32Operator"/>
    /// </summary>
    public static class CRC32
    {
        /// <summary>
        /// 对流中的一段内容执行 CRC32 检验，每次读入 4K 的数据到缓冲并持续进行 CRC 检验
        /// </summary>
        /// <param name="stream">可定位数据流 stream.position = start </param>
        /// <param name="start">读取数据的起始位 0-based</param>
        /// <param name="length">读取数据的总长度</param>
        /// <returns>检验结果</returns>
        public static CRC32Operator ComputeStream(Stream stream, long start, long length)
        {
            byte[] buffer = new byte[4096];
            CRC32Operator crc = new CRC32Operator();
            stream.Position = start;
            while (stream.Position < start + length)
            {
                long readLength = start + length - stream.Position;
                readLength = readLength > 4096 ? 4096 : readLength;
                // readLength 以后的片段是脏数据
                stream.Read(buffer, 0, (int)readLength);
                crc.CircleXorArray(buffer, 0, (int)readLength);
            }
            return crc;
        }

        /// <summary>
        /// 对若干文件执行 CRC32 检验
        /// </summary>
        /// <returns>检验结果</returns>
        public static CRC32Operator[] ComputeFile(params FileInfo[] files)
        {
            CRC32Operator[] result = new CRC32Operator[files.Length];
            for (int index = 0; index < files.Length; index++)
            {
                FileInfo info = files[index];
                result[index] = ComputeFile(info.FullName);
            }
            return result;
        }

        /// <summary>
        /// 对目标文件执行 CRC32 检验
        /// </summary>
        /// <param name="filePath">目标文件路径</param>
        /// <returns>检验结果</returns>
        public static CRC32Operator ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException($"\"{nameof(filePath)}\" is null or empty。", nameof(filePath));
            else if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 1024 * 1024)
            {
                using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                    return ComputeStream(stream, 0L, stream.Length);
            }
            else
                return ComputeByteArray(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// 对目标字符串执行 CRC32 检验
        /// </summary>
        /// <param name="str">待检测字符串</param>
        /// <returns>检验结果</returns>
        public static CRC32Operator ComputeString(string str) => ComputeByteArray(Encoding.Unicode.GetBytes(str));

        /// <summary>
        /// 将待计算数据进行 CRC32 运算，并返回检验结果
        /// </summary>
        /// <returns>检验结果</returns>
        public static CRC32Operator ComputeByteArray(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException($"{nameof(ComputeByteArray)}.{nameof(data)} is null");

            CRC32Operator crc = new CRC32Operator();
            crc.CircleXorArray(data, 0, data.Length);
            return crc;
        }
        /// <summary>
        /// 将待计算数据进行 CRC32 运算，并返回检验结果
        /// </summary>
        /// <param name="data">待计算数据</param>
        /// <param name="start">读取数据的起始位 0-based</param>
        /// <param name="length">读取数据的总长度</param>
        /// <returns>检验结果</returns>
        public static CRC32Operator ComputeByteArray(byte[] data, int start, int length)
        {
            if (data is null)
                throw new ArgumentNullException($"{nameof(ComputeByteArray)}.{nameof(data)} is null");

            CRC32Operator crc = new CRC32Operator();
            crc.CircleXorArray(data, start, length);
            return crc;
        }
    }
}