﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.MobileAppService.Models
{
    public class Friend
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        public string FirstName { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        public string LastName { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string MiddleName { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public string FacebookId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(50)")]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public string Gender { get; set; }

        public ICollection<GroupFriend> GroupFriends { get; set; }
    }
}