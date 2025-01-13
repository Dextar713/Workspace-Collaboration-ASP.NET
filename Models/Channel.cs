using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Channel group is required!")]
        public int? GroupId { get; set; }

        [Required(ErrorMessage = "Channel category is required!")]
        public int? CategoryId { get; set; }

        public virtual Group? Group { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }
    }
}
