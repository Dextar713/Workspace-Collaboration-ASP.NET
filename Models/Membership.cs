using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discord2.Models
{
    public class Membership
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int? GroupId { get; set; }

        public virtual AppUser? User { get; set; }

        public virtual Group? Group { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;
    }
}
