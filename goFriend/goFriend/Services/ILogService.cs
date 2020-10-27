using System.Reflection;

namespace goFriend.Services
{
    public interface ILogService
    {
        void Initialize(Assembly assembly, string assemblyName);
    }
}
