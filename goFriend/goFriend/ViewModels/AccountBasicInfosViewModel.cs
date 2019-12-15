using System;
using System.Linq;
using System.Threading.Tasks;
using goFriend.DataModel;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;

namespace goFriend.ViewModels
{
    public class AccountBasicInfosViewModel : BaseViewModel
    {
        public const string HomeAddress = "Hanoi, Vietnam";
        public static Position? HomePosition;

        public Friend Friend { get; set; }
        public bool PositionDraggable { get; set; }

        public string Name => Friend?.Name;
        public string Email => Friend?.Email;
        public string Gender => Friend?.Gender;
        public DateTime? Birthday => Friend?.Birthday;
        public string ImageUrl => Friend?.GetImageUrl();
        public string GenderByLanguage => Friend?.Gender == "male" ? res.Male : res.Female;

        public async Task<Position> GetPosition()
        {
            if (Friend.Location != null)
            {
                return new Position(Friend.Location.Y, Friend.Location.X);
            }
            else
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.High);
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        return new Position(location.Latitude, location.Longitude);
                    }
                }
                catch (Exception)
                {
                    // GPS disabled
                }

                if (!HomePosition.HasValue)
                {
                    var geoCoder = new Geocoder();
                    var approximateLocations = await geoCoder.GetPositionsForAddressAsync(HomeAddress);
                    HomePosition = approximateLocations.FirstOrDefault();
                }
                return new Position(HomePosition.Value.Latitude, HomePosition.Value.Longitude);
            }
        }
    }
}
