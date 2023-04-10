using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.IO
{
    public static class DirectoryInfoExtension
    {
        public static void CopyFilesTo(this DirectoryInfo src, DirectoryInfo des, bool recursion, string searchPattern = "*")
        {
            if (!Directory.Exists(src.FullName))
                throw new DirectoryNotFoundException(src.FullName);
            string srcDirectoryPath = src.FullName.Replace("\\", "/");
            string desDirectoryPath = des.FullName.Replace("\\", "/");
            string[] filesPath = Directory.GetFiles(srcDirectoryPath, searchPattern);
            if (filesPath.Length > 0)
                Directory.CreateDirectory(des.FullName);
            for (int i = 0; i < filesPath.Length; i++)
            {
                string fileSrcPath = filesPath[i];
                fileSrcPath = fileSrcPath.Replace("\\", "/");
                string fileLocalPath = fileSrcPath.Substring(srcDirectoryPath.Length);
                string fileDesPath = $"{fileSrcPath}/{fileLocalPath}";
                File.Copy(fileSrcPath, fileDesPath, true);
            }
            if (recursion)
            {
                foreach (string srcSubDirectoryPath in Directory.GetDirectories(srcDirectoryPath))
                {
                    DirectoryInfo srcSubDirectory = new DirectoryInfo(srcSubDirectoryPath);
                    DirectoryInfo desSubDirectory = new DirectoryInfo($"{des.FullName}/{srcSubDirectory.Name}");
                    CopyFilesTo(srcSubDirectory, desSubDirectory, recursion, searchPattern);
                }
            }
        }
    }
}