﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discord2.Models
{
    public class AppUser: IdentityUser
    {
        public bool IsBanned { get; set; } = false;

        public virtual ICollection<Membership>? Memberships { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }

        public string? Bio {  get; set; }

        public string? AvatarPath { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        [NotMapped]
        public int TotalGroups { get; set; }

        [NotMapped]
        public int DaysActive { get; set; }
    }
}
