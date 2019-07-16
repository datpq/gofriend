using System;

namespace goFriend.Models
{
    public class User
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FacebookId { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }

        public string Gender { get; set; }

        public override string ToString()
        {
            return $"{FacebookId}|{Name}|{Email}";
        }

        public string Avatar =>
            string.IsNullOrEmpty(FacebookId) ? (Gender == "male" ? "default_male.jpg" : "default_female.jpg")
                : $"https://graph.facebook.com/{FacebookId}/picture?type=normal";
    }
}
