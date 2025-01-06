using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discord2.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The description is required!")]
        public string Description { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public virtual ICollection<Membership>? Memberships { get; set; }

        public virtual ICollection<Channel>? Channels { get; set; }

        [NotMapped]
        public virtual ICollection<AppUser>? Members { get; set; }

        [NotMapped]
        public virtual ICollection<GroupRole>? GroupRoles { get; set; }

    }
}
