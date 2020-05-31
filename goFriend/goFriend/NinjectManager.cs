using goFriend.Services;
using Ninject;
using Ninject.Modules;
using Xamarin.Forms;

namespace goFriend
{
    public static class NinjectManager
    {
        private static IKernel _ninjectKernel;

        public static void Wire(INinjectModule module)
        {
            _ninjectKernel = new StandardKernel(module);
        }

        public static T Resolve<T>()
        {
            return _ninjectKernel.Get<T>();
        }
    }

    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            var logger = DependencyService.Get<ILogManager>().GetLog();
            var mediaService = DependencyService.Get<IMediaService>();
            Bind(typeof(IStorageService)).ToMethod(context => new StorageService(logger, mediaService)).InSingletonScope();
        }
    }
}
