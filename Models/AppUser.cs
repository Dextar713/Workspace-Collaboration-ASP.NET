using Microsoft.AspNetCore.Identity;

namespace Discord2.Models
{
    public class AppUser: IdentityUser
    {
        public bool IsBanned { get; set; } = false;

        public virtual ICollection<Membership>? Memberships { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }
    }
}
