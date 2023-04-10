using System;

namespace GitRepository
{
    public enum ResourceUpdateResult
    {
        Unknown,
        RegexError,
        NetworkError,
        NeedUpdate,
        UnhandledException,
        Success,
    }
}