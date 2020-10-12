using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace goFriend.Services
{
    public class LoggerMicrosoftImpl : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        public LoggerMicrosoftImpl(Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
        }

        public void Debug(string text, params object[] args)
        {
            logger.LogDebug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            logger.LogError(text, args);
        }

        public void TrackError(Exception ex, IDictionary<string, string> properties = null)
        {
            //Do not TrackError on server side
            logger.LogError(ex.ToString());
        }

        public void Fatal(string text, params object[] args)
        {
            logger.LogCritical(text, args);
        }

        public void Info(string text, params object[] args)
        {
            logger.LogInformation(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            logger.LogTrace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            logger.LogWarning(text, args);
        }
    }
}
