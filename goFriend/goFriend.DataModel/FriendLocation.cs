using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using NetTopologySuite.Geometries;


namespace goFriend.DataModel
{
    public class FriendLocation
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "GEOMETRY")]
        [JsonConverter(typeof(GeoPointConverter))]
        public Point Location { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
