using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }  

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public virtual ICollection<Membership>? Memberships { get; set; }
    }
}
