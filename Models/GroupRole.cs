using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class GroupRole
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Group Role Name is required")]
        public string Name { get; set; }

        public bool CanWrite { get; set; } = true;

        public bool HasSecretChannelsAccess { get; set; } = false;

        public bool CanManipulateUsers { get; set; } = false;

        public virtual ICollection<Membership>? UserRoleMemberships { get; set; }
    }
}
