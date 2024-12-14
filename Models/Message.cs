using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The message cannot be empty!")]
        public string Content { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        [Required]
        public int? ChannelId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public virtual Channel? Channel { get; set; }

        //public virtual AppUser? User { get; set; }
    }
}
