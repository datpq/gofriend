using goFriend.iOS;
using NLog;
using Xamarin.Forms;

[assembly: Dependency(typeof(NLogLogger))]
namespace goFriend.iOS
{
    public class NLogLogger : Services.ILogger
    {
        private Logger log;

        public NLogLogger(Logger log)
        {
            this.log = log;
        }

        private string ReformatText(string text)
        {
            return $"DPH|{text}";
        }

        public void Debug(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Debug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Error(text, args);
        }

        public void Fatal(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Fatal(text, args);
        }

        public void Info(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Info(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Trace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            text = ReformatText(text);
            log.Warn(text, args);
        }
    }
}