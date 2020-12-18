using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace goFriend.DataModel
{
    public class Friend : ICloneable
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string Name { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string FirstName { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string LastName { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string MiddleName { get; set; }

        //[Required]
        [Column(TypeName = "VARCHAR(20)")]
        public string FacebookId { get; set; }

        [Column(TypeName = "VARCHAR(50)")]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public string Gender { get; set; }

        public bool Active { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        //private Point _location;
        [Column(TypeName = "GEOMETRY")]
        [JsonConverter(typeof(GeoPointConverter))]
        public Point Location { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string Address { get; set; }
        [Column(TypeName = "NVARCHAR(30)")]
        public string CountryName { get; set; }

        [JsonIgnore]
        [Column(TypeName = "VARCHAR(170)")]
        public string DeviceInfo { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string Info { get; set; }

        [JsonIgnore]
        public byte[] Image { get; set; }

        [JsonIgnore]
        [Column(TypeName = "VARCHAR(400)")]
        public string FacebookToken { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public ThirdPartyLogin ThirdPartyLogin { get; set; }

        [Column(TypeName = "VARCHAR(400)")]
        [JsonIgnore]
        public string ThirdPartyToken { get; set; }

        [Column(TypeName = "VARCHAR(50)")]
        public string ThirdPartyUserId { get; set; }

        public Guid Token { get; set; }

        public bool? ShowLocation { get; set; }

        [JsonIgnore]
        public ICollection<GroupFriend> GroupFriends { get; set; }
        [JsonIgnore]
        public FriendLocation FriendLocation { get; set; }

        public override string ToString()
        {
            return $"{FacebookId}|{Id}|{Name}|{Email}";
        }

        public string ToFullString()
        {
            return $"{FacebookId}|{Id}|{Name}|{Email}|{LastName}|{MiddleName}|{FirstName}|{Birthday}|{Gender}";
        }

        public object Clone()
        {
            var result = new Friend();
            CopyToIfNull(result);
            return result;
        }

        public void CopyToIfNull(Friend friend)
        {
            var isChanged = false;
            if (string.IsNullOrEmpty(friend.FacebookId))
            {
                isChanged = true;
                friend.FacebookId = FacebookId;
            }
            if (string.IsNullOrEmpty(friend.Name))
            {
                isChanged = true;
                friend.Name = Name;
            }
            if (string.IsNullOrEmpty(friend.FirstName))
            {
                isChanged = true;
                friend.FirstName = FirstName;
            }
            if (string.IsNullOrEmpty(friend.LastName))
            {
                isChanged = true;
                friend.LastName = LastName;
            }
            if (string.IsNullOrEmpty(friend.MiddleName))
            {
                isChanged = true;
                friend.MiddleName = MiddleName;
            }
            if (friend.Birthday == null)
            {
                isChanged = true;
                friend.Birthday = Birthday;
            }
            if (string.IsNullOrEmpty(friend.Gender))
            {
                isChanged = true;
                friend.Gender = Gender;
            }
            if (string.IsNullOrEmpty(friend.Email))
            {
                isChanged = true;
                friend.Email = Email;
            }
            if (friend.CreatedDate == null)
            {
                friend.CreatedDate = DateTime.Now;
            }
            if (friend.DeviceInfo != DeviceInfo)
            {
                isChanged = true;
                friend.DeviceInfo = DeviceInfo;
            }
            if (Image != null)
            {
                isChanged = true;
                friend.Image = Image;
            }
            if (isChanged)
            {
                friend.ModifiedDate = DateTime.Now;
            }
        }
    }

    public enum ThirdPartyLogin
    {
        Facebook,
        Apple
    }

    public class GeoPointConverter : JsonConverter<Point>
    {
        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            serializer.Serialize(writer, value.X);
            writer.WritePropertyName("Y");
            serializer.Serialize(writer, value.Y);
            writer.WriteEndObject();
        }

        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            try
            {
                var jsonObject = JObject.Load(reader);
                return new Point((double) jsonObject["X"], (double) jsonObject["Y"]);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
