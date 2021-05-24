using goFriend.iOS.Services;
using goFriend.Services;
using Xamarin.Forms;
using UIKit;

[assembly: Dependency(typeof(IOSDevice))]
namespace goFriend.iOS.Services
{
    public class IOSDevice : IDevice
    {
        public string GetIdentifier()
        {
            return UIDevice.CurrentDevice.IdentifierForVendor.ToString();
        }
    }
}