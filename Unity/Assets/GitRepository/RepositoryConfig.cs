using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace GitRepository
{
    [Serializable]
    public class RepositoryConfig : IDisposable
    {
        private enum Platform
        {
            Error,
            /// <summary>
            /// CSDN
            /// </summary>
            Gitcode,
            /// <summary>
            /// 码云访问公开仓库的公开信息也需要设置登录token...离谱
            /// </summary>
            Gitee,
            Github,
        }

        public const string MasterBranch = "master";

        /// <summary>
        /// eg: https://gitcode.net/qq_34919016/sc2coopplugin-resource
        /// </summary>
        public readonly string RepositoryUri;
        /// <summary>
        /// eg: qq_34919016
        /// </summary>
        public readonly string RepositoryOwener;
        /// <summary>
        /// eg: sc2coopplugin-resource
        /// </summary>
        public readonly string RepositoryName;
        public readonly string LocalDirectory;
        /// <summary>
        /// 由于存在多线程读写，为保证不出现"删除一个已打开文件"的bug,
        /// 直接上个读写锁...
        /// </summary>
        public readonly ReaderWriterLockSlim IOLock;
        public readonly string Branch;
        public readonly Regex CommitRegex;
        private readonly Platform m_Platform;

        public volatile int UpdateTimes;

        public string HttpCloneUri
        {
            get
            {
                switch (m_Platform)
                {
                    //case Platform.Github:
                    //    return $"{RepositoryUri}/archive/refs/heads/{Branch}.zip";
                    //Gitee需要token
                    //case Platform.Gitee:
                    // CSDN的路径好诡异
                    case Platform.Gitcode:
                        return $"{RepositoryUri}/-/archive/{Branch}/{RepositoryName}-{Branch}.zip";
                    default:
                        throw new NotImplementedException(m_Platform.ToString());
                }
            }
        }

        public string CommitPageUri
        {
            get
            {
                switch (m_Platform)
                {
                    //case Platform.Github:
                    //case Platform.Gitee:
                    //    // https://gitee.com/ZhangHuan0407/SC2CoopPlugin-Resource/commits/master
                    //    return $"{RepositoryUri}/commits/{Branch}";
                    case Platform.Gitcode:
                        // https://gitcode.net/qq_34919016/sc2coopplugin-resource/-/commits/master
                        return $"{RepositoryUri}/-/commits/{Branch}";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string LocalCommitVersion => $"{LocalDirectory}/Commit";

        public RepositoryConfig(string repositoryUri, string localDirectory, string branch = MasterBranch)
        {
            RepositoryUri = repositoryUri;
            LocalDirectory = localDirectory;
            IOLock = new ReaderWriterLockSlim();
            Branch = branch;
            if (repositoryUri.Contains("gitee.com/"))
                m_Platform = Platform.Gitee;
            else if (repositoryUri.Contains("github.com/"))
                m_Platform = Platform.Github;
            else if (repositoryUri.Contains("gitcode.net/"))
                m_Platform = Platform.Gitcode;
            else
                throw new NotImplementedException("m_Platform");
            string[] owenerWithRepository = null;
            switch (m_Platform)
            {
                case Platform.Gitee:
                    owenerWithRepository = repositoryUri.Substring(repositoryUri.IndexOf("gitee.com/") + "gitee.com/".Length).Split('/');
                    RepositoryOwener = owenerWithRepository[0];
                    RepositoryName = owenerWithRepository[1];
                    CommitRegex = new Regex("<a class=\"commit-short-id\" id=\"short-id\" value=\"(?<Commit>[0-9a-z]+)\" " +
                        "href=\"/" + RepositoryOwener + "/" + RepositoryName + "/commit/\\k<Commit>\">(?<ShortCommit>[0-9a-z]+)</a>");
                    throw new NotImplementedException("没有token, 无法实现");
                    break;
                case Platform.Github:
                    // Github 因为国内不稳定，滞后开发
                    owenerWithRepository = repositoryUri.Substring(repositoryUri.IndexOf("github.com/") + "github.com/".Length).Split('/');
                    RepositoryOwener = owenerWithRepository[0];
                    RepositoryName = owenerWithRepository[1];
                    CommitRegex = new Regex("\\<a href=\"" + RepositoryOwener + "/" + RepositoryName + "/commit/(?<Commit>[0-9a-z]+)\"" +
                        " class=\"d-none js-permalink-shortcut\" data-hotkey=\"y\"\\>Permalink\\</a\\>");
                    throw new NotImplementedException();
                    break;
                case Platform.Gitcode:
                    owenerWithRepository = repositoryUri.Substring(repositoryUri.IndexOf("gitcode.net/") + "gitcode.net/".Length).Split('/');
                    RepositoryOwener = owenerWithRepository[0];
                    RepositoryName = owenerWithRepository[1];
                    CommitRegex = new Regex("\\<a class=\"commit-row-message item-title js-onboarding-commit-item \"" +
                        " href=\"/" + RepositoryOwener + "/" + RepositoryName + "/-/commit/(?<Commit>[0-9a-z]+)\"\\>.+\\</a\\>");
                    break;
            }
            UpdateTimes = 0;
        }

        public void Dispose()
        {
            IOLock.Dispose();
        }
    }
}