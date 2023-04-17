using System;

namespace GitRepository
{
    public enum ResourceUpdateResult
    {
        Unknown,
        RegexError,
        NetworkError,
        NeedUpdateResource,
        NeedUpdateClent,
        UnhandledException,
        Success,
    }
}