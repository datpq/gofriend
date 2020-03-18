using System;
using goFriend.DataModel;
using Xamarin.Forms.GoogleMaps;

namespace goFriend.ViewModels
{
    public class AccountBasicInfosViewModel : BaseViewModel
    {
        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public int FixedCatsCount { get; set; }

        public Friend Friend { get; set; }
        public bool Editable { get; set; }

        public string Name => Friend?.Name;
        public string Email => Friend?.Email;
        public string Gender => Friend?.Gender;
        public string Address => Friend?.Address;
        public string CountryName => Friend?.CountryName;
        public DateTime? Birthday => Friend?.Birthday;
        public string ImageUrl => Friend?.GetImageUrl();
        public string GenderByLanguage => Friend?.Gender == "male" ? res.Male : Friend?.Gender == "female" ? res.Female : string.Empty;
        public bool ShowLocation => App.User.Id == Friend.Id ? Friend.ShowLocation == true :
            App.User.Location != null && App.User.ShowLocation == true && Friend.Location != null && Friend.ShowLocation == true;
    }
}
