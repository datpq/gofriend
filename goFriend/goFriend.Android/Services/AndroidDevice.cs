using goFriend.Droid.Services;
using goFriend.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDevice))]
namespace goFriend.Droid.Services
{
    public class AndroidDevice : IDevice
    {
        public string GetIdentifier()
        {
            return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }
    }
}