using NLog;
using System;
using System.Collections.Generic;

namespace goFriend.MobileAppService.Helpers
{
    public class LoggerImpl : Services.ILogger
    {
        private readonly Logger logger;

        public LoggerImpl(Logger logger)
        {
            this.logger = logger;
        }

        public void Debug(string text, params object[] args)
        {
            logger.Debug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            logger.Error(text, args);
        }

        public void TrackError(Exception ex, IDictionary<string, string> properties = null)
        {
            //Do not TrackError on server side
            logger.Error(ex);
        }

        public void Fatal(string text, params object[] args)
        {
            logger.Fatal(text, args);
        }

        public void Info(string text, params object[] args)
        {
            logger.Info(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            logger.Trace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            logger.Warn(text, args);
        }
    }
}
