using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace GitRepository
{
    public class DownloadResourceTool
    {
        public readonly RepositoryConfig Config;
        private volatile string m_LatestCommit;

        public DownloadResourceTool(RepositoryConfig config)
        {
            Config = config;
        }

        private (ResourceUpdateResult state, string commit) GetLatestCommitVersion()
        {
            WebRequest webRequest = HttpWebRequest.Create(Config.CommitPageUri);
            webRequest.Method = "GET";
            string webPageContent;
            using (WebResponse response = webRequest.GetResponse())
            {
                string characterSet = (response as HttpWebResponse).CharacterSet;
                Encoding encoding = Encoding.GetEncoding(characterSet);
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    webPageContent = reader.ReadToEnd().ToString();
                }
            }
            List<string> allCommits = new List<string>();
            foreach (Match match in Config.CommitRegex.Matches(webPageContent))
            {
                if (match.Success)
                    allCommits.Add(match.Groups["Commit"].Value);
            }
#if UNITY_EDITOR || ALPHA
            if (allCommits.Count > 0)
            {
                string log = string.Join(",  ", allCommits);
                UnityEngine.Debug.Log(allCommits[0] + "\n" + log);
            }
#endif
            if (allCommits.Count > 0)
                return (ResourceUpdateResult.Success, allCommits[0]);
            else
                return (ResourceUpdateResult.RegexError, string.Empty);
        }
        public Task<ResourceUpdateResult> CheckUpdateAsync()
        {
            var task = Task<ResourceUpdateResult>.Run(async () =>
            {
                (ResourceUpdateResult state, string commit) latestCommit = default;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        latestCommit = GetLatestCommitVersion();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(nameof(DownloadResourceTool) + nameof(CheckUpdateAsync) + ex.ToString());
                        latestCommit.state = ResourceUpdateResult.NetworkError;
                        latestCommit.commit = string.Empty;
                    }

                    if (latestCommit.state == ResourceUpdateResult.Success)
                    {
                        string localCommit = string.Empty;
                        Config.IOLock.EnterReadLock();
                        if (File.Exists(Config.LocalCommitVersion))
                            localCommit = File.ReadAllText(Config.LocalCommitVersion);
                        Config.IOLock.ExitReadLock();
                        if (latestCommit.commit == localCommit)
                            return ResourceUpdateResult.Success;
                        else
                            return ResourceUpdateResult.NeedUpdate;
                    }
                    else if (latestCommit.state == ResourceUpdateResult.RegexError)
                        return ResourceUpdateResult.RegexError;
                    await Task.Delay(500);
                }
                return latestCommit.state;
            });
            return task;
        }

        public Task<ResourceUpdateResult> DownloadUpdateAsync()
        {
            m_LatestCommit = null;
            var task = Task<ResourceUpdateResult>.Run(() =>
            {
                ResourceUpdateResult result = default;
                for (int i = 0; i < 3; i++)
                {
                    result = ResourceUpdateResult.NetworkError;
                    try
                    {
                        result = DownloadUpdate_Internal();
                    }
                    catch (Exception ex)
                    {
                        result = ResourceUpdateResult.NetworkError;
                        UnityEngine.Debug.LogError(ex);
                    }
                    if (result == ResourceUpdateResult.Success)
                        break;
                    else if (result != ResourceUpdateResult.NetworkError)
                        break;
                }
                return result;
            });
            return task;
        }
        private ResourceUpdateResult DownloadUpdate_Internal()
        {
            (ResourceUpdateResult state, string commit) latestCommit1 = GetLatestCommitVersion();
            if (latestCommit1.state != ResourceUpdateResult.Success)
                return latestCommit1.state;
            m_LatestCommit = latestCommit1.commit;

            // download zip
            WebRequest downloadRequest = HttpWebRequest.Create(Config.HttpCloneUri);
            Debug.Log(downloadRequest.RequestUri);
            downloadRequest.Method = "GET";
            Directory.CreateDirectory(GameDefined.TempDirectory);
            string zipFileName = $"{GameDefined.TempDirectory}/download_{commit}.zip";
            using (WebResponse downloadResponse = downloadRequest.GetResponse())
            {
                if (downloadResponse.ContentType.ToLowerInvariant().Contains("text/html"))
                {
                    using (StreamReader streamReader = new StreamReader(downloadResponse.GetResponseStream()))
                    {
                        string content = streamReader.ReadToEnd();
                        Debug.LogError(content);
                        return ResourceUpdateResult.UnhandledException;
                    }
                }
                Stream responseStream = downloadResponse.GetResponseStream();
                byte[] buffer = new byte[2040];
                using (FileStream tempFileStream = new FileStream(zipFileName, FileMode.Create, FileAccess.Write))
                {
                    int size = 0;
                    do
                    {
                        size = responseStream.Read(buffer, 0, 2040);
                        tempFileStream.Write(buffer, 0, size);
                    } while (size > 0);
                }
            }

            (ResourceUpdateResult state, string commit) latestCommit2 = default;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    latestCommit2 = GetLatestCommitVersion();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
                if (latestCommit2.state == ResourceUpdateResult.Success)
                    break;
            }
            if (latestCommit2.state != ResourceUpdateResult.Success)
                return latestCommit2.state;
            if (latestCommit1.commit != latestCommit2.commit)
            {
                m_LatestCommit = latestCommit2.commit;
                return ResourceUpdateResult.NeedUpdate;
            }

            // delete old files and unzip
            Config.IOLock.EnterWriteLock();
            try
            {
                if (Directory.Exists(Config.LocalDirectory))
                    Directory.Delete(Config.LocalDirectory, true);
                Directory.CreateDirectory(Config.LocalDirectory);
                UnZip(zipFileName, Config.LocalDirectory);
                File.WriteAllText(Config.LocalCommitVersion, latestCommit2.commit);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return ResourceUpdateResult.UnhandledException;
            }
            finally
            {
                Config.IOLock.ExitWriteLock();
            }
            Config.UpdateTimes++;
            return ResourceUpdateResult.Success;
        }
        private void UnZip(string zipFileName, string desDirectory)
        {
            byte[] buffer = new byte[2040];
            using (FileStream fileStreamIn = new FileStream(zipFileName, FileMode.Open, FileAccess.Read))
            {
                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn))
                {
                    while (zipInStream.GetNextEntry() is ZipEntry entry)
                    {
                        WriteFile(zipInStream, entry);
                    }
                }
            }
            void WriteFile(ZipInputStream zipInStream, ZipEntry entry)
            {
                if (!entry.IsFile)
                    return;
                string fileSubPath = entry.Name.Substring(entry.Name.IndexOf("/") + 1);
                FileInfo fileInfo = new FileInfo($"{desDirectory}/{fileSubPath}");
                Directory.CreateDirectory(fileInfo.DirectoryName);
                using (FileStream fileStreamOut = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                {
                    int size;
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    fileStreamOut.Close();
                };
            }
        }
    }
}