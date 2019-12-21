using System;
using goFriend.DataModel;
using Xamarin.Forms.Maps;

namespace goFriend.ViewModels
{
    public class AccountBasicInfosViewModel : BaseViewModel
    {
        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public int FixedCatsCount { get; set; }

        public Friend Friend { get; set; }
        public bool PositionDraggable { get; set; }

        public string Name => Friend?.Name;
        public string Email => Friend?.Email;
        public string Gender => Friend?.Gender;
        public string Address => Friend?.Address;
        public string CountryName => Friend?.CountryName;
        public DateTime? Birthday => Friend?.Birthday;
        public string ImageUrl => Friend?.GetImageUrl();
        public string GenderByLanguage => Friend?.Gender == "male" ? res.Male : res.Female;
    }
}
