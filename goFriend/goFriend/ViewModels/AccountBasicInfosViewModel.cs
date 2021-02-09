using System;
using System.Windows.Input;
using goFriend.DataModel;
using Xamarin.Forms.GoogleMaps;
using System.Linq;
using System.Collections.Generic;

namespace goFriend.ViewModels
{
    public class AccountBasicInfosViewModel : BaseViewModel
    {
        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public int FixedCatsCount { get; set; }

        private Friend _friend;
        public Friend Friend
        {
            get => _friend;
            set
            {
                _friend = value;
                Name = _friend.Name;
                Email = _friend.Email;
                Gender = _friend.Gender;
                Address = _friend.Address;
                CountryName = _friend.CountryName;
                Birthday = _friend.Birthday;
                Phone = _friend.Phone;
                Relationship = _friend.Relationship;
            }
        }
        public bool Editable { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }

        private string _gender;
        public string Gender {
            get => _gender;
            set
            {
                _gender = value;
                if (_gender == null)
                {
                    _genderByLanguage = null;
                }
                else
                {
                    Constants.GenderDictionary.TryGetValue(_gender, out _genderByLanguage);
                }
            }
        }
        private string _genderByLanguage;
        public string GenderByLanguage
        {
            get => _genderByLanguage;
            set
            {
                _genderByLanguage = value;
                if (_genderByLanguage == null)
                {
                    _gender = null;
                }
                else
                {
                    var kvp = Constants.GenderDictionary.SingleOrDefault(x => x.Value == _gender);
                    if (!kvp.Equals(default(KeyValuePair<string, string>)))
                    {
                        _genderByLanguage = kvp.Key;
                    }
                }
            }
        }

        public string Address { get; set; }
        public string CountryName { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }

        private string _relationship;
        public string Relationship
        {
            get => _relationship;
            set
            {
                _relationship = value;
                if (_relationship == null)
                {
                    _relationshipByLanguage = null;
                }
                else
                {
                    Constants.RelationshipDictionary.TryGetValue(_relationship, out _relationshipByLanguage);
                }
            }
        }
        private string _relationshipByLanguage;
        public string RelationshipByLanguage
        {
            get => _relationshipByLanguage;
            set
            {
                _relationshipByLanguage = value;
                if (_relationshipByLanguage == null)
                {
                    _relationship = null;
                }
                else
                {
                    var kvp = Constants.RelationshipDictionary.SingleOrDefault(x => x.Value == _relationshipByLanguage);
                    if (!kvp.Equals(default(KeyValuePair<string, string>)))
                    {
                        _relationship = kvp.Key;
                    }
                }
            }
        }

        public string ImageUrl => Friend?.GetImageUrl();
        public bool ShowLocation => App.User.Id == Friend.Id ? Friend.ShowLocation == true :
            App.User.Location != null && App.User.ShowLocation == true && Friend.Location != null && Friend.ShowLocation == true;

        public ICommand CommandDetail { get; set; }
    }
}
