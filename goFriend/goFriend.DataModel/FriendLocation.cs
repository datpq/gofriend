using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using NetTopologySuite.Geometries;
using System.Linq;
using System.Collections.Generic;

namespace goFriend.DataModel
{
    public class FriendLocation
    {
        [Key]
        public int Id { get; set; }

        public int FriendId { get; set; }

        [JsonIgnore]
        public Friend Friend { get; set; }

        [Column(TypeName = "GEOMETRY")]
        [JsonConverter(typeof(GeoPointConverter))]
        public Point Location { get; set; }

        [Column(TypeName = "VARCHAR(400)")]
        public string SharingInfo { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [JsonIgnore]
        [NotMapped]
        public double DistanceToMyLocation { get; set; } // used in client side

        [JsonIgnore]
        [NotMapped]
        public readonly List<GroupFriend> GroupFriends = new List<GroupFriend>();

        public double GetRadiusInKm(int groupId)
        {
            var radius = double.Parse(SharingInfo.Split(Extension.SepMain)
                .Single(x => int.Parse(x.Split(Extension.SepSub)[0]) == groupId)
                .Split(Extension.SepSub)[1]);
            return radius == 0 ? double.MaxValue : radius;
        }
    }
}
