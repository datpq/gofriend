using goFriend.Services;
using UIKit;

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