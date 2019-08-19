using goFriend.iOS;
using NLog;
using Xamarin.Forms;

[assembly: Dependency(typeof(NLogLogger))]
namespace goFriend.iOS
{
    public class NLogLogger : Services.ILogger
    {
        private readonly Logger _log;

        public NLogLogger(Logger log)
        {
            _log = log;
        }

        private static string ReformatText(string text)
        {
            return $"DPH|{text}";
        }

        public void Debug(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Debug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Error(text, args);
        }

        public void Fatal(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Fatal(text, args);
        }

        public void Info(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Info(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Trace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            text = ReformatText(text);
            _log.Warn(text, args);
        }
    }
}