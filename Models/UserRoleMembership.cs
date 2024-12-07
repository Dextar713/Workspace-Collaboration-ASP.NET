using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class UserRoleMembership
    {
        [Key]
        public int Id { get; set; }

        public string? RoleName { get; set; }

        public virtual GroupRole? GroupRole { get; set; }

        public int? MembershipId { get; set; }

        public virtual Membership? Membership { get; set; }
    }
}
