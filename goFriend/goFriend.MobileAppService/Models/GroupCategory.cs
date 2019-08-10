﻿using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.MobileAppService.Models
{
    public class GroupCategory
    {
        public int Id { get; set; }
        public Group Group { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat1 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat2 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat3 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat4 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat5 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat6 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat7 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat8 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat9 { get; set; }
    }
}