using System;
using System.Collections.Generic;

namespace goFriend.Services
{
    public interface ILogger
    {
        void Trace(string text, params object[] args);
        void Debug(string text, params object[] args);
        void Info(string text, params object[] args);
        void Warn(string text, params object[] args);
        void Error(string text, params object[] args);
        void TrackError(Exception ex, IDictionary<string, string> properties = null);
        void Fatal(string text, params object[] args);
    }
}
