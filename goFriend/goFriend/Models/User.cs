using System;
using System.Collections.Generic;
using System.Text;

namespace goFriend.Models
{
    public class User
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacebookId { get; set; }

        public bool IsMale { get; set; }

        public override string ToString()
        {
            return $"{FacebookId}|{Name}";
        }

        public string Avatar
        {
            get
            {
                return string.IsNullOrEmpty(FacebookId) ? (IsMale ? "default_male.jpg" : "default_female.jpg")
                    : $"https://graph.facebook.com/{FacebookId}/picture?type=normal";
            }
        }
    }
}
