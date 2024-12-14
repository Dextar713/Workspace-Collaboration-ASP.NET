using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]  
        public string Content { get; set; }

        public DateTime Date { get; set; } 

        public int? ChannelId { get; set; }

        //public string? UserId { get; set; }

        public virtual Channel? Channel { get; set; }

        //public virtual AppUser? User { get; set; }
    }
}
