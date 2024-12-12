using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Channel>? Channels { get; set; }
    }
}
